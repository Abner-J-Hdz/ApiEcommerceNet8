using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;
public class UserLoginDto
{
	[Required(ErrorMessage = "El Username es requerido")]
	public string? Username { get; set; }


	[Required(ErrorMessage = "El Password es requerido")]
	public string Password { get; set; } = string.Empty;
	
}
