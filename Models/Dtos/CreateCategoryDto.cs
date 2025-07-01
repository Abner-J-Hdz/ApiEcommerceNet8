using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;
public class CreateCategoryDto
{
	[Required(ErrorMessage = "Nombre es obligatorio")]
	[MaxLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
	[MinLength(3, ErrorMessage = "El nombre debe tener al menos 3 caracteres")]
	public string Name { get; set; } = string.Empty;

}
