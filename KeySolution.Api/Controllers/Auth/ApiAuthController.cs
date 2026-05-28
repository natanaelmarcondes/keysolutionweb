using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using System.Security.Claims;
using KeySolution.Api.Models.DTO;
using KeySolution.Api.Models.DTO.Auth;

namespace KeySolution.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class ApiAuthController : ControllerBase
    {
        private readonly QueryFactory _db;

        public ApiAuthController(QueryFactory db)
        {
            _db = db;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            if (model == null)
            {
                return BadRequest(new LoginResponseDTO
                {
                    Sucesso = false,
                    Mensagem = "Dados de login não informados."
                });
            }

            if (string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Senha))
            {
                return BadRequest(new LoginResponseDTO
                {
                    Sucesso = false,
                    Mensagem = "Informe email e senha."
                });
            }

            var usuario = _db.Query("usuarios")
                .Join("setores", "setores.set_codigo", "usuarios.set_codigo")
                .Where("usuarios.usr_email", model.Email)
                .Select(
                    "usuarios.usr_codigo",
                    "usuarios.USER_ID",
                    "usuarios.usr_nome",
                    "usuarios.usr_email",
                    "usuarios.usr_nivel",
                    "usuarios.usr_senha_hash",
                    "setores.set_nome as setor"
                )
                .FirstOrDefault<LoginUsuarioDTO>();

            if (usuario == null)
            {
                return Unauthorized(new LoginResponseDTO
                {
                    Sucesso = false,
                    Mensagem = "Usuário ou senha inválidos."
                });
            }

            if (string.IsNullOrWhiteSpace(usuario.usr_senha_hash) ||
                !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.usr_senha_hash))
            {
                return Unauthorized(new LoginResponseDTO
                {
                    Sucesso = false,
                    Mensagem = "Usuário ou senha inválidos."
                });
            }

            var queues = _db.Query("queue_technician as qt")
                .Join("queuedefinition as qd", "qt.QUEUEID", "qd.QUEUEID")
                .Where("qt.TECHNICIANID", usuario.USER_ID)
                .Select("qd.QUEUENAME")
                .OrderBy("qd.QUEUENAME")
                .Get<string>()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.usr_codigo.ToString()),
                new Claim(ClaimTypes.Name, usuario.usr_nome ?? ""),
                new Claim(ClaimTypes.Email, usuario.usr_email ?? ""),
                new Claim(ClaimTypes.Role, usuario.usr_nivel ?? ""),

                new Claim("USER_ID", usuario.USER_ID.ToString()),
                new Claim("Setor", usuario.setor ?? "")
            };

            foreach (var queue in queues)
            {
                claims.Add(new Claim("Queue", queue));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.LembrarUsuario,
                AllowRefresh = model.LembrarUsuario
            };

            if (model.LembrarUsuario)
            {
                authProps.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps
            );

            return Ok(new LoginResponseDTO
            {
                Sucesso = true,
                Mensagem = "Login realizado com sucesso.",
                UsuarioCodigo = usuario.usr_codigo,
                UserId = usuario.USER_ID,
                Nome = usuario.usr_nome ?? "",
                Email = usuario.usr_email ?? "",
                Nivel = usuario.usr_nivel ?? "",
                Setor = usuario.setor ?? "",
                Queues = queues
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var usuarioCodigo = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nome = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var nivel = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue("USER_ID");
            var setor = User.FindFirstValue("Setor");

            var queues = User.Claims
                .Where(c => c.Type == "Queue")
                .Select(c => c.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            return Ok(new LoginResponseDTO
            {
                Sucesso = true,
                Mensagem = "Usuário autenticado.",
                UsuarioCodigo = int.TryParse(usuarioCodigo, out var cod) ? cod : 0,
                UserId = long.TryParse(userId, out var uid) ? uid : 0,
                Nome = nome ?? "",
                Email = email ?? "",
                Nivel = nivel ?? "",
                Setor = setor ?? "",
                Queues = queues
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return Ok(new
            {
                sucesso = true,
                mensagem = "Logout realizado com sucesso."
            });
        }
    }
}
