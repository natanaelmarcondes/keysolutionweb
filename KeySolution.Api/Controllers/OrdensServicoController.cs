using KeySolution.Api.Models;
using Microsoft.AspNetCore.Mvc;
using SqlKata;
using SqlKata.Execution;

namespace KeySolution.Api.Controllers
{
    [ApiController]
    [Route("api/ordens-servico")]
    public sealed class OrdensServicoController : ControllerBase
    {
        private readonly QueryFactory _db;

        private const int DefaultPageSize = 18;
        private const int MaxPageSize = 100;

        public OrdensServicoController(QueryFactory db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Listar([FromQuery] OrdemServicoListarRequest request)
        {
            if (request.Page < 1)
                request.Page = 1;

            if (request.PageSize <= 0)
                request.PageSize = DefaultPageSize;

            if (request.PageSize > MaxPageSize)
                request.PageSize = MaxPageSize;

            request.Texto = string.IsNullOrWhiteSpace(request.Texto) ? null : request.Texto.Trim();
            request.FiltroCampo = string.IsNullOrWhiteSpace(request.FiltroCampo) ? null : request.FiltroCampo.Trim();
            request.FiltroUsuarioNome = string.IsNullOrWhiteSpace(request.FiltroUsuarioNome) ? null : request.FiltroUsuarioNome.Trim();

            // Se o usuário digitar apenas números na pesquisa textual, interpreta como número da O.S.
            if (!request.Numero.HasValue &&
                !string.IsNullOrWhiteSpace(request.Texto) &&
                long.TryParse(request.Texto, out long numeroPelaPesquisa))
            {
                request.Numero = numeroPelaPesquisa;
                request.Texto = null;
            }

            // Ao pesquisar por número, ignora os demais filtros.
            if (request.Numero.HasValue)
            {
                request.Texto = null;
                request.FiltroCampo = null;
                request.FiltroUsuarioId = null;
                request.FiltroUsuarioNome = null;
                request.StatusId = null;
                request.StatusIds = null;
            }

            Query baseQuery = _db.Query("workorder as os")
                .Join("aaauser as ua", "ua.USER_ID", "os.CREATEDBYID")
                .Join("aaauser as ar", "ar.USER_ID", "os.REQUESTERID")
                .LeftJoin("workorderstates as ws", "ws.WORKORDERID", "os.WORKORDERID")
                .LeftJoin("aaauser as ao", "ao.USER_ID", "ws.OWNERID")
                .LeftJoin("statusdefinition as st", "st.STATUSID", "ws.STATUSID");

            // Status que devem ficar fora da listagem padrão.
            string[] defaultExcludedStatusNames =
            {
                "Closed",
                "Resolved",
                "Descontinuado",
                "Caiu no esquecimento"
            };

            bool hasExplicitStatusFilter =
                !string.IsNullOrWhiteSpace(request.StatusIds) ||
                request.StatusId.HasValue;

            if (!request.Numero.HasValue && !hasExplicitStatusFilter)
            {
                string[] excludedUpper = defaultExcludedStatusNames
                    .Select(s => s.Trim().ToUpperInvariant())
                    .ToArray();

                baseQuery = baseQuery.Where(q =>
                    q.WhereNull("st.STATUSNAME")
                     .OrWhereRaw(
                        "UPPER(TRIM(st.STATUSNAME)) NOT IN (?, ?, ?, ?)",
                        excludedUpper[0],
                        excludedUpper[1],
                        excludedUpper[2],
                        excludedUpper[3])
                );
            }

            if (request.Numero.HasValue)
            {
                baseQuery = baseQuery.Where("os.WORKORDERID", request.Numero.Value);
            }

            if (!request.Numero.HasValue && !string.IsNullOrWhiteSpace(request.Texto))
            {
                baseQuery = baseQuery.WhereRaw(
                    "(os.TITLE LIKE ? OR os.DESCRIPTION LIKE ?)",
                    $"%{request.Texto}%",
                    $"%{request.Texto}%"
                );
            }

            List<long> selectedStatusIds = ObterStatusIds(request);

            if (!request.Numero.HasValue && selectedStatusIds.Count > 0)
            {
                baseQuery = baseQuery.WhereIn("ws.STATUSID", selectedStatusIds);
            }

            List<long>? filtroUsuarioIds = ObterFiltroUsuarioIds(request);

            if (!request.Numero.HasValue &&
                !string.IsNullOrWhiteSpace(request.FiltroCampo) &&
                filtroUsuarioIds != null &&
                filtroUsuarioIds.Count > 0)
            {
                switch (request.FiltroCampo)
                {
                    case "cliente":
                        baseQuery = baseQuery.WhereIn("os.REQUESTERID", filtroUsuarioIds);
                        break;

                    case "aberta":
                        baseQuery = baseQuery.WhereIn("os.CREATEDBYID", filtroUsuarioIds);
                        break;

                    case "responsavel":
                        baseQuery = baseQuery.WhereIn("ws.OWNERID", filtroUsuarioIds);
                        break;

                    case "criado":
                        baseQuery = baseQuery
                            .Join("workorderhistory as wc", j => j
                                .On("wc.WORKORDERID", "os.WORKORDERID")
                                .Where("wc.OPERATION", "CREATE"))
                            .WhereIn("wc.OPERATIONOWNERID", filtroUsuarioIds);
                        break;
                }
            }

            long totalItems = baseQuery.Clone()
                .SelectRaw("COUNT(DISTINCT os.WORKORDERID)")
                .FirstOrDefault<long>();

            List<OrdemServicoListItemDto> items = baseQuery.Clone()
                .Select(
                    "os.WORKORDERID as OsCodigo",
                    "os.CREATEDTIME as OsData",
                    "os.TITLE as OsTitulo",
                    "ar.FIRST_NAME as Cliente",
                    "ua.FIRST_NAME as UsuarioAbertura",
                    "st.STATUSNAME as Status",
                    "ws.ISREAD as IsRead"
                )
                .SelectRaw("COALESCE(ao.FIRST_NAME, ar.FIRST_NAME) as UsuarioResponsavel")
                .Distinct()
                .OrderByDesc("os.WORKORDERID")
                .ForPage(request.Page, request.PageSize)
                .Get<OrdemServicoListItemDto>()
                .ToList();

            PagedResultDto<OrdemServicoListItemDto> result = new()
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems
            };

            return Ok(result);
        }

        private List<long> ObterStatusIds(OrdemServicoListarRequest request)
        {
            List<long> selectedStatusIds = new();

            if (!string.IsNullOrWhiteSpace(request.StatusIds))
            {
                string[] partes = request.StatusIds.Split(
                    new[] { ',', ';', ' ' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (string parte in partes)
                {
                    if (long.TryParse(parte.Trim(), out long statusId) && statusId > 0)
                        selectedStatusIds.Add(statusId);
                }
            }
            else if (request.StatusId.HasValue && request.StatusId.Value > 0)
            {
                selectedStatusIds.Add(request.StatusId.Value);
            }

            return selectedStatusIds
                .Distinct()
                .ToList();
        }

        private List<long>? ObterFiltroUsuarioIds(OrdemServicoListarRequest request)
        {
            if (request.FiltroUsuarioId.HasValue && request.FiltroUsuarioId.Value > 0)
            {
                return new List<long>
                {
                    request.FiltroUsuarioId.Value
                };
            }

            if (!string.IsNullOrWhiteSpace(request.FiltroUsuarioNome))
            {
                List<long> ids = _db.Query("aaauser")
                    .WhereLike("FIRST_NAME", $"%{request.FiltroUsuarioNome}%")
                    .OrderBy("FIRST_NAME")
                    .Select("USER_ID")
                    .Limit(200)
                    .Get<long>()
                    .ToList();

                if (ids.Count == 0)
                {
                    return new List<long>
                    {
                        -1
                    };
                }

                return ids;
            }

            return null;
        }
    }
}