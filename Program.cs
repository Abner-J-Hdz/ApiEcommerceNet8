using ApiEcommerce.Constants;
using ApiEcommerce.Data;
using ApiEcommerce.Mapping;
using ApiEcommerce.Models;
using ApiEcommerce.Repository;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");

//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(dbConnectionString)
  .UseSeeding((context, _) =>
  {
	  var appContext = (ApplicationDbContext)context;
	  DataSeeder.SeedData(appContext);
  })
);


/* Configuracion de cache*/
/*
 Claro, esta secci�n de c�digo configura el servicio de Response Caching en una aplicaci�n ASP.NET Core. 
Este servicio permite almacenar en cach� las respuestas HTTP para mejorar el rendimiento y reducir 
la carga en el servidor. Vamos a desglosarlo:
 */
builder.Services.AddResponseCaching(options =>
{
	/*
1.	AddResponseCaching:
�	Este m�todo registra el middleware de Response Caching en el contenedor de servicios de la aplicaci�n. 
	Esto significa que el middleware estar� disponible para su uso en el pipeline de solicitudes HTTP.
�	El middleware de Response Caching almacena en cach� las respuestas HTTP que cumplen con ciertos criterios, 
	como tener encabezados espec�ficos (Cache-Control, Expires, etc.).	 
	 */

	options.MaximumBodySize = 1024 * 1024;//1M

	/*
�	Este par�metro indica si las rutas de las solicitudes deben ser sensibles a may�sculas y min�sculas al determinar si una respuesta est� en cach�.	 
	 */
	/*
�	Si se establece en true, las rutas como /api/product y /API/Product se tratar�n como diferentes y tendr�n entradas de cach� separadas.
�	Esto es importante en sistemas donde las rutas pueden diferir por el uso de may�sculas y min�sculas.	 
	 */
	options.UseCaseSensitivePaths = true;
});
/*Para que esta configuraci�n funcione, es necesario habilitar el middleware de Response Caching e
 * n el pipeline de la aplicaci�n, lo cual ya se hace m�s adelante en el archivo con esta l�nea:*/


// Register the repository services antes de usarlos en los controladores, sino daran errores
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductoRepository, ProductRepository>();//Add scoped permite que se inyecte la dependencia en el controlador
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Configurar Mapster
builder.Services.AddMapster();
MapsterConfig.RegisterMappings();

//Configurar servicios de identity framwework core
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();


/* Configurar servicio de autenticacion */
var secretKey = builder.Configuration.GetValue<string>("ApiSettings:SecretKey");
if (string.IsNullOrEmpty(secretKey))
{
	throw new InvalidOperationException("Secretkey no est� configurado");
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
		ValidateIssuerSigningKey = true,//que est� valido el token
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),// se valida la firma del token
		ValidateIssuer = false,//no se valida el cliente
		ValidateAudience = false//no se valida la audiencia o cliente, true valida quien lo consume
	};
});

/*configurar perfiles de cach�  de manera global para luego poderlos usar en los controladores*/
builder.Services.AddControllers(options =>
{
	options.CacheProfiles.Add(CacheProfiles.Default10, CacheProfiles.Profile10);
	options.CacheProfiles.Add(CacheProfiles.Default20, CacheProfiles.Profile20);

});




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
	//configuracion para poder a�adir el token a los request de swagger
  options =>
  {
	  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	  {
		  Description = "Nuestra API utiliza la Autenticaci�n JWT usando el esquema Bearer. \n\r\n\r" +
					  "Ingresa la palabra a continuaci�n el token generado en login.\n\r\n\r" +
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
	  options.SwaggerDoc("v1", new OpenApiInfo
	  {
		  Version = "v1",
		  Title = "Api Ecommerce",
		  Description = "Api para gestionar productos y usuarios",
		  TermsOfService = new Uri("https://example.com/terms"),
		  Contact = new OpenApiContact
		  {
			  Name = "anrcode",
			  Url = new Uri("https://anrcode.com")
		  }, 
		  License = new OpenApiLicense
		  {
			  Name = "Licencia de uso",
			  Url = new Uri("https://example.com/license")
		  }
	  });

	  options.SwaggerDoc("v2", new OpenApiInfo
	  {
		  Version = "v2",
		  Title = "Api Ecommerce v2",
		  Description = "Api para gestionar productos y usuarios",
		  TermsOfService = new Uri("https://example.com/terms"),
		  Contact = new OpenApiContact
		  {
			  Name = "anrcode",
			  Url = new Uri("https://anrcode.com")
		  },
		  License = new OpenApiLicense
		  {
			  Name = "Licencia de uso",
			  Url = new Uri("https://example.com/license")
		  }
	  });
  }
);


/* INICIO CONFIGURACION DE VERSIONAMIENTO */

var apiVersioningBuilder = builder.Services.AddApiVersioning(option =>
{
	option.AssumeDefaultVersionWhenUnspecified = true;//asume una version por defecto
	option.DefaultApiVersion = new ApiVersion(1, 0);// indicamos la version por defecto
	option.ReportApiVersions = true;//esto para que devuelva  info en los header de respuesta, as�: api-supported-versions: 1.0 
	//option.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"));//?api-version //OSEA va a ser necesario que se pase en la url
});

apiVersioningBuilder.AddApiExplorer(option =>
{
	option.GroupNameFormat = "'v'VVV";//v1,v2,v3 => indincamos esto con esta linea  de codigo
	option.SubstituteApiVersionInUrl = true;//habilitar� el uso de version en la url en cada peticion en la ruta
	//pero hay que configurar el controlador, en el apiVersioningBuilder y setear la prop

});

/* FIN CONFIGURACION DE VERSIONAMIENTO */

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
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
	});
}

// para que el api soporte tener imagenes, css, algun javascript, o archivo
app.UseStaticFiles();

app.UseHttpsRedirection();

/* Uso de la configuracion de cors */
app.UseCors(PolicyNames.AllowSpecificOrigin);

/*Habilita el uso de cach�*/
app.UseResponseCaching();

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
//usar la misma veersion del sdk con JwtBearer, sino dar� problemas


Para el uso de versionamiento instalamos
Asp.Versioning.Mvc 8.0.1
Asp.Versioning.Mvc.ApiExplorer 8.0.1

PARA EL USO DE IDENTITY PARA AUTENTICACION Y AUTORIZACION
Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.17


Instalar Entity Framework Core Tools en la consola del administrador de paquetes:
dotnet tool install --global dotnet-ef


Segundo paso es crear la migraci�n inicial para crear la base de datos y las tablas correspondientes:
dotnet ef migrations add InitialMigration

Tercer paso crear la base de datos con el siguiente comando:
dotnet ef database update

segundo migracion
dotnet ef migrations add createTableProduct

comdando para remover migracion:

 */