namespace ApiEcommerce.Models.Dtos;
public class UpdateProductDto
{
	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public decimal Price { get; set; }

	public string ImgUrl { get; set; } = string.Empty;

	public string SKU { get; set; } = string.Empty;//PROD-001-BLK-M

	public int Stock { get; set; }

	public DateTime? UpdateDate { get; set; } = null;

	//relacion con el modelo de categoria
	public int CategoryId { get; set; }
}
