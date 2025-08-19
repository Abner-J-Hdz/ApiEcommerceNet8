using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiEcommerce.Repository;
public class UserRepository : IUserRepository
{
	private readonly ApplicationDbContext _dbo;

	private string? secretKey;

	private readonly UserManager<ApplicationUser> _userManager;

	private readonly RoleManager<IdentityRole> _rolManager;




   public UserRepository(ApplicationDbContext db, IConfiguration configuration, 
	   UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
	{
		_dbo = db;
		secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
		_userManager = userManager;
		_rolManager = roleManager;

	}

	public ApplicationUser? GetUser(string userId)
	{
		return _dbo.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
	}

	public ICollection<ApplicationUser>? GetUsers()
	{
		return _dbo.ApplicationUsers.OrderBy(x => x.UserName).ToList();
	}

	public bool IsUniqueUser(string userName)
	{
		return !_dbo.Users.Any(x => x.UserName.ToLower().Trim() == userName.ToLower().Trim());
	}

	public async Task<UserLoginResponseDto?> Login(UserLoginDto userLoginDto)
	{
		if (string.IsNullOrEmpty(userLoginDto.Username))
		{
			return new UserLoginResponseDto()
			{
				Token = "",
				User = null,
				Message = "El username es requerido"
			};
		}

		var user = await _dbo.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName !=null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

		if(user is null)
		{
			return new UserLoginResponseDto
			{
				Token = "",
				User = null,
				Message = "Username no encontrado"
			};
		}

		if (userLoginDto.Password == null)
		{
			return new UserLoginResponseDto()
			{
				Token = "",
				User = null,
				Message = "Password requerido"
			};
		}

		//validacion de contraseña usando el  userManager de Identity
		bool isValidPassword = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

		if (!isValidPassword)
		{
			return new UserLoginResponseDto()
			{
				Token = "",
				User = null,
				Message = "Credenciales incorrectas"
			};
		}

		//generacion del jwt TODO: ESTUDIAR
		var handlerToken = new JwtSecurityTokenHandler();

		if (string.IsNullOrWhiteSpace(secretKey))
			throw new InvalidOperationException("Secret key no está configurada");

		var key = Encoding.UTF8.GetBytes(secretKey);

		//obtenemos los roles
		var roles = await _userManager.GetRolesAsync(user);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity
			(
				new[]
				{
					new Claim("id", user.Id.ToString()),
					new Claim("userName", user.UserName ?? string.Empty),
					new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty)
				}
			),
			Expires = DateTime.UtcNow.AddHours(2),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var token = handlerToken.CreateToken(tokenDescriptor);

		return new UserLoginResponseDto()
		{
			Token = handlerToken.WriteToken(token),
			
		   User = user.Adapt<UserDataDto>(),

			Message = "Usuario logueado correctamente"
		};
	}

	public async Task<UserDataDto?> Register(CreateUserDto createUserDto)
	{
		if (string.IsNullOrEmpty(createUserDto.Username))
		{
			throw new ArgumentNullException("El username es requerido");
		}

		if (createUserDto.Password == null)
		{
			throw new ArgumentException("El password es requerido");
		}

		var user = new ApplicationUser()
		{
			UserName = createUserDto.Username,
			Email = createUserDto.Username,
			NormalizedEmail = createUserDto.Username.ToUpper(),
			Name = createUserDto.Name
		};

		//creamos el usuario
		var result = await _userManager.CreateAsync(user, createUserDto.Password);

		//si todo sale bien, añadimos el rol, sino existe lo creamos
		if (result.Succeeded)
		{
			var userRole = createUserDto.Role ?? "User";

			var roleExists = await _rolManager.RoleExistsAsync(userRole);

			//esto es cuestinable por que lo roles ya deberian de existir
			if (!roleExists)
			{
				var identityRole = new IdentityRole(userRole);
				await _rolManager.CreateAsync(identityRole);
			}
			//agregamos el rol al usuario
			await _userManager.AddToRoleAsync(user, userRole);

			//var existUser = await _userManager.FindByNameAsync(createUserDto.Username);

			var createdUser = _dbo.ApplicationUsers.FirstOrDefault(u => u.UserName == createUserDto.Username);

	   return createdUser.Adapt<UserDataDto>();
		}
		var errors = string.Join(",", result.Errors.Select(e => e.Description));

		throw new ApplicationException($"No se pudo realizar el registro: {errors}");
	}
}
