using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;
public class CategoryRepository : ICategoryRepository
{
	private readonly ApplicationDbContext _db;

	public CategoryRepository(ApplicationDbContext db)
	{
		_db = db;
	}

	public bool CategoryExists(int categoryId)
	{
		return _db.Categories.Any(c => c.Id == categoryId);
	}

	public bool CategoryExists(string name)
	{
		return _db.Categories.Any(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
	}

	public bool CreateCategory(Category category)
	{
		category.CreationDate = DateTime.Now;
		_db.Categories.Add(category);
		return Save();
	}

	public bool DeleteCategory(Category category)
	{
		_db.Categories.Remove(category);
		return Save();
	}

	public ICollection<Category> GetCategories()
	{
		return _db.Categories.OrderBy(c => c.Name).ToList();
	}

	public Category? GetCategory(int categoryId)
	{
		return _db.Categories.FirstOrDefault(c => c.Id == categoryId);
	}

	public bool Save()
	{
		return _db.SaveChanges() >= 0; // Devuelve true si se guardaron cambios
	}

	public bool UpdateCategory(Category category)
	{
		category.CreationDate = DateTime.Now;
		_db.Categories.Update(category);
		return Save();
	}
}
