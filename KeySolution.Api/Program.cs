using System.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// =====================================================
// CONTROLLERS
// =====================================================
builder.Services.AddControllers();

// =====================================================
// SWAGGER
// =====================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================================================
// CORS - ANGULAR
// =====================================================
string angularUrl = builder.Configuration["Cors:Angular"] ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularCors", policy =>
    {
        policy
            .WithOrigins(angularUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// =====================================================
// AUTENTICAÇÃO POR COOKIE
// =====================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/api/auth/unauthorized";
        options.LogoutPath = "/api/auth/logout";
        options.AccessDeniedPath = "/api/auth/forbidden";

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        options.Cookie.Name = ".KeySolution.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

// =====================================================
// AUTORIZAÇÃO
// =====================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProgramacaoOnly", policy =>
    {
        policy.RequireAssertion(ctx =>
        {
            static bool MatchProgramacao(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                value = value.Trim();

                return value.Contains("Programação", StringComparison.OrdinalIgnoreCase)
                    || value.Contains("Programacao", StringComparison.OrdinalIgnoreCase);
            }

            bool hasSetor = ctx.User.Claims
                .Where(c => c.Type == "Setor")
                .Any(c => MatchProgramacao(c.Value));

            bool hasQueue = ctx.User.Claims
                .Where(c => c.Type == "Queue")
                .Any(c => MatchProgramacao(c.Value));

            return hasSetor || hasQueue;
        });
    });
});

// =====================================================
// CONEXÃO MYSQL / MARIADB
// =====================================================
builder.Services.AddScoped<IDbConnection>(_ =>
{
    string? cs = builder.Configuration.GetConnectionString("Maria");

    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("ConnectionString 'Maria' não foi configurada no appsettings.json.");

    return new MySqlConnection(cs);
});

// =====================================================
// SQLKATA
// =====================================================
builder.Services.AddScoped<QueryFactory>(sp =>
{
    IDbConnection conn = sp.GetRequiredService<IDbConnection>();
    MySqlCompiler compiler = new MySqlCompiler();

    return new QueryFactory(conn, compiler);
});

// =====================================================
// BUILD APP
// =====================================================
WebApplication app = builder.Build();

// =====================================================
// PIPELINE HTTP
// =====================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "KEYSOLUTION API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "KEYSOLUTION API v1");
        options.RoutePrefix = string.Empty;
    });
}

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AngularCors");

// IMPORTANTE: primeiro autentica, depois autoriza
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();