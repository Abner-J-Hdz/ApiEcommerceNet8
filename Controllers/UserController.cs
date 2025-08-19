using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[Authorize] //habilita endpoint privados
				//[ApiVersion("1.0")]//Establecemos las versiones que soporta este controllador
				//[ApiVersion("2.0")]//Establecemos las versiones que soporta este controllador
	[ApiVersionNeutral]
	public class UserController : ControllerBase
	{
	   private readonly IUserRepository _userRepository;

	   public UserController(IUserRepository userRepository)
	   {
		   _userRepository = userRepository;
	   }

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(typeof(List<UserDto>),StatusCodes.Status200OK)]
		public IActionResult GetUsers()
		{
			var users = _userRepository.GetUsers();

		   var userDto = users.Adapt<List<UserDto>>();

			return Ok(userDto);
		}

		[HttpGet("{idUser}", Name = "GetUser")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
		public IActionResult GetUser(string idUser)
		{
			if (string.IsNullOrEmpty(idUser))
			{
				ModelState.AddModelError("CustomError", "IdUser invalido");
				return BadRequest(ModelState);
			}

			var user = _userRepository.GetUser(idUser);

			if(user is null)
				return NotFound($"Usuario con id {idUser} no encontrado");

		   var userDto = user.Adapt<UserDto>();

			return Ok(userDto);
		}

		[HttpPost(Name = "RegisterUser")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
		{
			if(createUserDto is null || !ModelState.IsValid)
				return BadRequest(ModelState);

			if (string.IsNullOrWhiteSpace(createUserDto.Username))
				return BadRequest("UserName es requerido");

			if (!_userRepository.IsUniqueUser(createUserDto.Username))
				return BadRequest("Usuario ya existe");

			var result = await _userRepository.Register(createUserDto);

			if(result is null)
				return StatusCode(StatusCodes.Status500InternalServerError, "Error al registrar el usuario");

			return CreatedAtRoute("GetUser", new { idUser = result.Id }, result);
		}


		[HttpPost("Login", Name = "LoginUser")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(UserLoginDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> LoginUser([FromBody] UserLoginDto loginUserDto)
		{
			if (loginUserDto is null || !ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userRepository.Login(loginUserDto);

			if (user is null)
				return Unauthorized();

			if(user.User is null)
			{
				return BadRequest(user.Message);
			}

			return Ok(user);
		}
	}
}
