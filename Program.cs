using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));
// Register the repository services antes de usarlos en los controladores, sino daran errores
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductoRepository, ProductRepository>();//Add scoped permite que se inyecte la dependencia en el controlador
builder.Services.AddScoped<IUserRepository, UserRepository>();


//configurar autommaper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


/* INICIO CONFIGURACION DE CORS */
builder.Services.AddCors(options =>
{
    options.AddPolicy(PolicyNames.AllowSpecificOrigin,
        builder =>
        {
            builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
        });
});
/* FIN CONFIGURACION DE CORS */

app.UseHttpsRedirection();

/* Uso de la configuracion de cors */
app.UseCors(PolicyNames.AllowSpecificOrigin);

app.UseAuthorization();

app.MapControllers();

app.Run();


/*
Librerias para instalar
SDK 8.0
ENTITY FRAMEWORK CORE 9.0.6
ENTITY FRAMEWORK CORE SQL SERVER 9.0.6
ENTITY FRAMEWORK CORE TOOLS 9.0.6
AUTOMAPPER 14.0.0
BCrypt.Net-Next 4.0.3
Instalar Entity Framework Core Tools en la consola del administrador de paquetes:
dotnet tool install --global dotnet-ef


Segundo paso es crear la migración inicial para crear la base de datos y las tablas correspondientes:
dotnet ef migrations add InitialMigration

Tercer paso crear la base de datos con el siguiente comando:
dotnet ef database update

segundo migracion
dotnet ef migrations add createTableProduct

comdando para remover migracion:

 */