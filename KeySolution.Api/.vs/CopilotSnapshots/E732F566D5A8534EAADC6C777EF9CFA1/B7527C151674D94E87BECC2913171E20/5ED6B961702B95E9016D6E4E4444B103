using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;

namespace KeySolution.Controllers
{
    [ApiController]
    [Route("api/usuarios-lookup")]
    [Authorize]
    public class UsuariosLookupController : ControllerBase
    {
        private readonly QueryFactory _db;

        public UsuariosLookupController(QueryFactory db)
        {
            _db = db;
        }

        [HttpGet("nomes")]
        public IActionResult ListarNomes()
        {
            var nomes = _db.Query("usuarios")
                .WhereNotNull("usr_nome")
                .Where("usr_nome", "!=", "")
                .Distinct()
                .Select("usr_nome")
                .OrderBy("usr_nome")
                .Get<string>()
                .ToList();

            return Ok(nomes);
        }
    }
}