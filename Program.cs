using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// 1. CONFIGURACIÓN DE CORS (Añadido para conectar con tu Frontend de Angular)
// ==========================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Agregar los controladores al contenedor de dependencias
builder.Services.AddControllers();

// Configuración de OpenAPI (Generación del JSON técnico en .NET 9)
builder.Services.AddOpenApi();

builder.Services.Configure<ScalarOptions>(options =>
{
    options.WithTitle("API de Gestión de Eventos");
});

// ==========================================================================
// CONFIGURACIÓN DEL PUERTO PARA RENDER (Evita el "Port scan timeout")
// ==========================================================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

// ==========================================================================
// CONFIGURACIÓN DEL PIPELINE HTTP (Entorno de Desarrollo)
// ==========================================================================
if (app.Environment.IsDevelopment())
{
    // Mapea la ruta del JSON técnico
    app.MapOpenApi();

    // Mapea la interfaz gráfica interactiva
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// ==========================================================================
// 2. ACTIVAR CORS 
// ==========================================================================
app.UseCors("PermitirTodo");

app.UseAuthorization();

app.MapControllers();

app.Run();