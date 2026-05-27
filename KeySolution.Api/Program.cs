using System.Data;
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
            .AllowAnyMethod();
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

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AngularCors");

app.UseAuthorization();

app.MapControllers();

app.Run();