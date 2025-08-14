using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Repository;
public class ProductRepository : IProductoRepository
{
	private readonly ApplicationDbContext _dbo;
	public ProductRepository(ApplicationDbContext db)
	{
		_dbo = db;
	}
	public bool BuyProduct(string name, int quantity)
	{
		if(string.IsNullOrEmpty(name) || quantity <= 0)
			return false;

		var product = _dbo.Products.FirstOrDefault(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
		if(product == null || product.Stock < quantity)
			return false; // Producto no encontrado o stock insuficiente
		
		product.Stock -= quantity; // Reducir el stock del producto
		_dbo.Products.Update(product); // Actualizar el producto en la base de datos
		return Save(); // Guardar los cambios y devolver true si se guardaron correctamente
	}

	public bool CreateProduct(Product product)
	{
		if (product is null)
			return false;

		product.CreationDate = DateTime.Now;
		product.UpdateDate = DateTime.Now;
		 _dbo.Products.Add(product); // Agregar el producto a la base de datos
		return Save(); // Guardar los cambios y devolver true si se guardaron correctamente
	}

	public bool DeleteProduct(Product product)
	{
		if (product is null)
			return false;

		_dbo.Products.Remove(product); // Eliminar el producto de la base de datos
		return Save(); // Guardar los cambios y devolver true si se guardaron correctamente
	}

	public Product? GetProduct(int productId)
	{
		if (productId <= 0)
			return null;

		return _dbo.Products.Include( p => p.Category)
			.FirstOrDefault(p => p.ProductId == productId);
	}

	public ICollection<Product> GetProducts()
	{
		// Devuelve todos los productos ordenados por nombre
		return _dbo.Products.Include(p => p.Category)
			.OrderBy(p => p.Name)
			.ToList();
	}

	public ICollection<Product> GetProductsForCategory(int categoryId)
	{
		if (categoryId <= 0)
			return new List<Product>();

		return _dbo.Products.Include(p => p.Category).Where(p => p.CategoryId == categoryId)
			.OrderBy(p => p.Name)
			.ToList();
	}

	public bool ProductExists(int productId)
	{
		if (productId <= 0)
			return false;

		return _dbo.Products
			.Any(p => p.ProductId == productId);
	}

	public bool ProductExists(string name)
	{
		if (string.IsNullOrEmpty(name))
			return false;

		return _dbo.Products
			.Any(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
	}

	public bool Save()
	{
		return _dbo.SaveChanges() >= 0; // Devuelve true si se guardaron cambios
	}

	public ICollection<Product> SearchProducts(string searchTerm)
	{
		if (string.IsNullOrEmpty(searchTerm))
			return new List<Product>();

		//return _dbo.Products
		//	.Where(p => p.Name.ToLower().Contains(name.ToLower()))
		//	.OrderBy(p => p.Name)
		//	.ToList();


		//Ejemplo usando queryable
		IQueryable<Product> query = _dbo.Products;

		// Si el nombre no es nulo o vacío, filtrar por nombre
		if (!string.IsNullOrEmpty(searchTerm))
		{
			query = query.Include(p => p.Category)
				.Where(p => 
					p.Name.ToLower().Contains(searchTerm.ToLower()) || 
					p.Description.ToLower().Contains(searchTerm.ToLower()));
		}

		return query.OrderBy(p => p.Name).ToList();
	}

	public bool UpdateProduct(Product product)
	{
		if (product is null)
			return false;

		product.UpdateDate = DateTime.Now; // Actualizar la fecha de modificación
		_dbo.Products.Update(product); // Actualizar el producto en la base de datos
		return Save(); // Guardar los cambios y devolver true si se guardaron correctamente
	}

	ICollection<Product> IProductoRepository.GetProductsInPages(int pageNumber, int pageSize)
	{
		return _dbo.Products.OrderBy(p => p.ProductId)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize).ToList();
	}

	int IProductoRepository.GetTotalProducts()
	{
		return _dbo.Products.Count();
	}
}
