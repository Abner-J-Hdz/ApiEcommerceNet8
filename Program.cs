using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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


/* Configurar servicio de autenticacion */
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
	throw new InvalidOperationException("Secretkey no está configurado");
}
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false;//desactiva el uso de https en produccion deberia ser true
	options.SaveToken = true;// guardar el token en el contexto de autencicacion
	//parametros de validacion para el token
	options.TokenValidationParameters = new TokenValidationParameters()//parametros del jwt
	{
		ValidateIssuerSigningKey = true,//que esté valido el token
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),// se valida la firma del token
		ValidateIssuer = false,//no se valida el cliente
		ValidateAudience = false//no se valida la audiencia o cliente, true valida quien lo consume
	};
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
	//configuracion para poder añadir el token a los request de swagger
  options =>
  {
	  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	  {
		  Description = "Nuestra API utiliza la Autenticación JWT usando el esquema Bearer. \n\r\n\r" +
					  "Ingresa la palabra a continuación el token generado en login.\n\r\n\r" +
					  "Ejemplo: \"12345abcdef\"",
		  Name = "Authorization",
		  In = ParameterLocation.Header,
		  Type = SecuritySchemeType.Http,
		  Scheme = "Bearer"
	  });
	  options.AddSecurityRequirement(new OpenApiSecurityRequirement()
	{
	  {
		new OpenApiSecurityScheme
		{
		  Reference = new OpenApiReference
		  {
			Type = ReferenceType.SecurityScheme,
			Id = "Bearer"
		  },
		  Scheme = "oauth2",
		  Name = "Bearer",
		  In = ParameterLocation.Header
		},
		new List<string>()
	  }
	});
  }
);


/* INICIO CONFIGURACION DE CORS  nota: escribir antes del builder*/
builder.Services.AddCors(options =>
{
	options.AddPolicy(PolicyNames.AllowSpecificOrigin,
		builder =>
		{
			builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
		});
});
/* FIN CONFIGURACION DE CORS */

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

/* Uso de la configuracion de cors */
app.UseCors(PolicyNames.AllowSpecificOrigin);

//PARA HABILITAR EL AUTHORIZE Y ALLOW ANONYMOUS Siempre antes de UseAuthorization
app.UseAuthentication();

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

Microsoft.AspNetCore.Authentication.JwtBearer --esto para configurar luego la authenticacion con jwt en el program
//usar la misma veersion del sdk con JwtBearer, sino dará problemas

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