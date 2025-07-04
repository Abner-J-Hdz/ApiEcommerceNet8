using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
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

	public UserRepository(ApplicationDbContext db, IConfiguration configuration)
	{
		_dbo = db;
		secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
	}

	public User? GetUser(int userId)
	{
		return _dbo.Users.FirstOrDefault(u => u.Id == userId);
	}

	public ICollection<User> GetUsers()
	{
		return _dbo.Users.OrderBy(x => x.UserName).ToList();
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

		var user = await _dbo.Users.FirstOrDefaultAsync<User>(u => u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

		if(user is null)
		{
			return new UserLoginResponseDto
			{
				Token = "",
				User = null,
				Message = "Username no encontrado"
			};
		}

		if(!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
		{
			return new UserLoginResponseDto
			{
				Token = "",
				User = null,
				Message = "Credenciales son incorrectas"
			};
		}

		//generacion del jwt TODO: ESTUDIAR
		var handlerToken = new JwtSecurityTokenHandler();

		if (string.IsNullOrWhiteSpace(secretKey))
			throw new InvalidOperationException("Secret key no está configurada");

		var key = Encoding.UTF8.GetBytes(secretKey);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity
			(
				new[]
				{
					new Claim("id", user.Id.ToString()),
					new Claim("userName", user.UserName),
					new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
				}
			),
			Expires = DateTime.UtcNow.AddHours(2),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var token = handlerToken.CreateToken(tokenDescriptor);

		return new UserLoginResponseDto()
		{
			Token = handlerToken.WriteToken(token),
			User = new UserRegisterDto()
			{
				UserName = user.UserName,
				Name = user.Name ?? "",
				Role = user.Role,
				Password = user.Password ?? ""
			},
			Message = "Usuario logueado correctamente"
		};
	}

	public async Task<User?> Register(CreateUserDto createUserDto)
	{
		var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

		var user = new User()
		{
			UserName = createUserDto.Username ?? "No Username",
			Name = createUserDto.Name,
			Password = encriptedPassword,
			Role = createUserDto.Role
		};

		_dbo.Users.Add(user);
		await _dbo.SaveChangesAsync();
		return user;
	}
}
