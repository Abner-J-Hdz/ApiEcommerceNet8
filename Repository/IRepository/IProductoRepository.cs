using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository;
public interface IProductoRepository
{
	//GetProducts
	ICollection<Product> GetProducts();

	ICollection<Product> GetProductsInPages(int pageNumber, int pageSize);

	int GetTotalProducts();

	//GetProductsForCategory
	ICollection<Product> GetProductsForCategory(int categoryId);

	//    - SearchProduct
	//        → Recibe un nombre y devuelve los productos
	//          que coincidan en ICollection del tipo Product.
	ICollection<Product> SearchProducts(string searchTerm);

	//GetProduct
	Product? GetProduct(int productId);

	//    - BuyProduct
	//        → Recibe el nombre del producto y una cantidad,
	//          y devuelve un bool indicando si la compra fue exitosa.
	bool BuyProduct(string name, int quantity);

	//- ProductExists (por id)
	//        → Recibe un id y devuelve un bool
	//          indicando si existe el producto.
	bool ProductExists(int productId);

	//    - ProductExists (por nombre)
	//        → Recibe un nombre y devuelve un bool
	//          indicando si existe el producto.
	bool ProductExists(string name);

	//    - CreateProduct
	//        → Recibe un objeto Product 
	//          y devuelve un bool indicando si la creación fue exitosa.
	bool CreateProduct(Product product);

	//    - UpdateProduct
	//        → Recibe un objeto Product
	//          y devuelve un bool indicando si la actualización fue exitosa.
	bool UpdateProduct(Product product);

	//    - DeleteProduct
	//        → Recibe un objeto Product
	//          y devuelve un bool indicando si la eliminación fue exitosa.
	bool DeleteProduct(Product product);

	//    - Save
	//        → Devuelve un bool indicando
	//          si los cambios se guardaron correctamente
	bool Save();
}
