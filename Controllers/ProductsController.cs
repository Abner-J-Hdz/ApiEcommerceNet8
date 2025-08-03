using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{

	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")]  //habilita endpoint privados
	[ApiVersionNeutral]
	//[ApiVersion("1.0")]//Establecemos las versiones que soporta este controllador
	//[ApiVersion("2.0")]//Establecemos las versiones que soporta este controllador

	public class ProductsController : ControllerBase
	{
		private readonly IProductoRepository _productoRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IMapper _mapper;

		public ProductsController(IProductoRepository productoRepository, IMapper mapper, ICategoryRepository categoryRepository)
		{
			_productoRepository = productoRepository;
			_mapper = mapper;
			_categoryRepository = categoryRepository;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[AllowAnonymous]
		public IActionResult GetProducts()
		{
			var products = _productoRepository.GetProducts();
			var productsDto = _mapper.Map<List<ProductDto>>(products);
			return Ok(productsDto);
		}

		[HttpGet("{productId:int}", Name = "GetProduct")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult GetProduct(int productId) {
			var product = _productoRepository.GetProduct(productId);

			if (product is null)
				return NotFound($"El producto con id {productId} no existe");

			var productDto = _mapper.Map<ProductDto>(product);

			return Ok(productDto);
		}

		[HttpPost]//obtener un id del query string
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult PostProduct([FromForm] CreateProductDto createProductDto)
		{
			if (createProductDto == null)
				return BadRequest(ModelState);

			if (_productoRepository.ProductExists(createProductDto.Name))
			{
				ModelState.AddModelError("CustomError", "La categoria ya existe");
				return BadRequest(ModelState);
			}

			//validar si existe la categoria
			if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
			{
				ModelState.AddModelError("CustomError", $"La categoria con id {createProductDto.CategoryId} no existe");
				return BadRequest(ModelState);
			}

			var product = _mapper.Map<Product>(createProductDto);

			//Agregando la imagen
			//validar si no estan mandando la imagen
			if(createProductDto.Image != null)
			{
				UploadProductImage(createProductDto, product);
			}
			else
			{
				product.ImgUrl = "https://placehold.co/300x300/png";
			}


			if (!_productoRepository.CreateProduct(product))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al guardar el registro {product.Name}");
				return StatusCode(500, ModelState);
			}

			//AQUI SI QUEREMOS DEVOLVER EL PRODUCTO CREADO CON LA CATEGORIA, TENDRIAMOS QUE OBTENER EL RECURSO RECIEN CREADO

			var createdProduct = _productoRepository.GetProduct(product.ProductId);
			var productDto = _mapper.Map<ProductDto>(createdProduct);

			//Devuelve un código de estado HTTP 201 Created (indica que el recurso fue creado correctamente).
			//Incluye en la respuesta un header Location que apunta a la URL donde se puede consultar el nuevo recurso.
			//También envía el recurso creado en el cuerpo de la respuesta.
			return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
		}


		[HttpGet("searchProductsByCategory/{categoryId:int}", Name = "GetProductByCategory")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
		//podemos especificar la estructura de la data para saber que es lo que retornamos
		public ActionResult GetProductByCategory(int categoryId)
		{
			//validamos si la categoria existe
			if (!_categoryRepository.CategoryExists(categoryId))
			{
				ModelState.AddModelError("CustomError", $"La categoria con id {categoryId} no existe");
				return BadRequest(ModelState);
			}

			var products = _productoRepository.GetProductsForCategory(categoryId);

			if (products is null || !products.Any())
			{
				return NotFound($"No se encontraron productos para la categoria con id {categoryId}");
			}

			var productsDto = _mapper.Map<List<ProductDto>>(products);

			return Ok(productsDto);
		}

		//Esto da error porque no se puede usar string como tipo de dato en la ruta
		//[HttpGet("searchProductByDescription/{name:string}", Name = "SearchProductByDescription")]
		[HttpGet("searchProductByDescription/{searchTerm}", Name = "SearchProductByDescription")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
		//podemos especificar la estructura de la data para saber que es lo que retornamos
		public ActionResult SearchProducts(string searchTerm)
		{
			if (string.IsNullOrEmpty(searchTerm))
			{
				ModelState.AddModelError("CustomError", $"La descripcion no puede ser vacia");
				return BadRequest(ModelState);
			}

			var products = _productoRepository.SearchProducts(searchTerm);

			if (products is null || !products.Any())
			{
				return NotFound($"No se encontraron productos con la descripción o nombre '{searchTerm}' ");
			}

			var productsDto = _mapper.Map<List<ProductDto>>(products);

			return Ok(productsDto);
		}

		//Esto da error porque no se puede usar string como tipo de dato en la ruta
		//[HttpGet("searchProductByDescription/{name:string}", Name = "SearchProductByDescription")]
		[HttpPatch("BuyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		//podemos especificar la estructura de la data para saber que es lo que retornamos
		public ActionResult BuyProduct(string name, int quantity)
		{
			if (string.IsNullOrEmpty(name) || quantity <= 0)
			{
				ModelState.AddModelError("CustomError", $"El nombre del producto o la cantidad no son validos");
				return BadRequest(ModelState);
			}

			var foundProduct = _productoRepository.ProductExists(name);
			if (!foundProduct)
			{
				ModelState.AddModelError("CustomError", $"El producto con nombre {name} no existe");
				return NotFound(ModelState);
			}

			if (!_productoRepository.BuyProduct(name, quantity))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al comprar el producto {name} o la cantidad solicitada es mayor a la disponible");
				return StatusCode(500, ModelState);
			}

			var units = quantity > 1 ? "unidades" : "unidad";

			return Ok($"Se compró {quantity} {units} del producto '{name}'");
		}

		[HttpPut("{productId:int}", Name = "Update")]//obtener un id del query string
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult UpdateProduct(int productId, [FromForm] UpdateProductDto updateProductDto)
		{
			if (updateProductDto == null)
				return BadRequest(ModelState);

			//validamos si product existe por medio de su ID
			if (!_productoRepository.ProductExists(productId))
			{
				ModelState.AddModelError("CustomError", "El product no existe");
				return BadRequest(ModelState);
			}

			//validar si existe la categoria
			if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
			{
				ModelState.AddModelError("CustomError", $"La categoria con id {updateProductDto.CategoryId} no existe");
				return BadRequest(ModelState);
			}

			var product = _mapper.Map<Product>(updateProductDto);


			//Agregando la imagen
			//validar si no estan mandando la imagen
			if (updateProductDto.Image != null)
            {
                UploadProductImage(updateProductDto, product);
            }
            else
			{
				product.ImgUrl = "https://placehold.co/300x300/png";
			}

			product.ProductId = productId; // Aseguramos que el ID del producto sea el correcto

			if (!_productoRepository.UpdateProduct(product))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al actualizar el registro {product.Name}");
				return StatusCode(500, ModelState);
			}

			return NoContent();
		}

        private void UploadProductImage(dynamic productDto, Product product)
        {
            string fileName = product.ProductId.ToString() + "-" + Guid.NewGuid().ToString() + Path.GetExtension(productDto.Image.FileName);
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductsImages");// carpeta donde se guardará la imagen
                                                                                                          //Sino existe el directorio lo creamos
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }
            //creamos la ruta donde se guardará la imagen
            var filePath = Path.Combine(imagesFolder, fileName);
            //verificamos si ya esiste un archivo igual
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                //si existe lo eliminamos
                file.Delete();
            }
            //vamos a escribir o guradar el archivo
            using var fileStream = new FileStream(filePath, FileMode.Create);
            productDto.Image.CopyTo(fileStream);//la imagen dentro del dto lo pasamos al fileStream

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            product.ImgUrl = $"{baseUrl}/productsImages/{fileName}";
            product.ImageUrlLocal = filePath;
        }

        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IActionResult DeleteProduct(int productId)
		{
			if(productId <= 0)
			{
				ModelState.AddModelError("CustomError", "El id del producto no es valido");
				return BadRequest(ModelState);
			}

			var product = _productoRepository.GetProduct(productId);

			//validar si el producto no es null
			if (product is null)
			{
				ModelState.AddModelError("CustomError", $"El producto con id {productId} no existe");
				return NotFound(ModelState);
			}

			if (!_productoRepository.DeleteProduct(product))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al eliminar el producto {product!.Name}");
				return StatusCode(500, ModelState);
			}

			return NoContent();
		}

	}
}
