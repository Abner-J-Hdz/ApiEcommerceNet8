namespace ApiEcommerce.Models.Dtos;
public class UserRegisterDto
{
	public string? Id { get; set; }

	public required string Name { get; set; }
	public required string UserName { get; set; }
	public  string? Password { get; set; }
	public  string? Role { get; set; }
}
