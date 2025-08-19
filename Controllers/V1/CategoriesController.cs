using ApiEcommerce.Constants;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers.V1
{
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[ApiVersion("1.0")]//Establecemos las versiones que soporta este controllador
	[Authorize(Roles = "Admin")] //habilita endpoint privados
								 //[EnableCors("AllowSpecificOrigin")]//Configuracion de cors a  nivel de controlador
								 //[EnableCors(PolicyNames.AllowSpecificOrigin)]//Configuracion de cors a  nivel de controlador
	public class CategoriesController : ControllerBase
	{

		private readonly ICategoryRepository _categoryRepository;


	   public CategoriesController(ICategoryRepository categoryRepository)
		{
			_categoryRepository = categoryRepository;

		}


		[HttpGet]
		[MapToApiVersion("1.0")]//decimos a que version pertenece este endpoint
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[AllowAnonymous]//hace que un metodo sea publico
						//[EnableCors("AllowSpecificOrigin")]// Configuracion de Cors a nivel de metodo
		[Obsolete("Este método está obsoleto. Use GetCategoriesById de la version 2 en su lugar." +
			"")]
		public IActionResult GetCategories()
		{
			var categories = _categoryRepository.GetCategories();
		   var categoriesDto = categories.Adapt<List<CategoryDto>>();
			return Ok(categoriesDto);
			//var categories = _categoryRepository.GetCategories();
			//var categoriesDto = new List<CategoryDto>();
			//foreach (var category in categories)
			//{
			//	categoriesDto.Add(_mapper.Map<CategoryDto>(category));
			//}
			//return Ok(categoriesDto);

		}

		[HttpGet("{id:int}", Name = "GetCategory")]//obtener un id del query string
		[ResponseCache(CacheProfileName = CacheProfiles.Default10)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[AllowAnonymous]//hace que un metodo sea publico
		public IActionResult GetCategory(int id)//igual al del httpget
		{
			Console.WriteLine($"Categoria con el ID {id} a las {DateTime.Now}");

			var category = _categoryRepository.GetCategory(id);

			Console.WriteLine($"Categoria con el ID {id}");

			if (category is null)
				return NotFound($"La categoria con id {id} no existe");

		   var categoriesDto = category.Adapt<CategoryDto>();

			return Ok(category);
		}

		[HttpPost]//obtener un id del query string
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult PostCategory([FromBody] CreateCategoryDto categoryDto)
		{
			if (categoryDto == null)
				return BadRequest(ModelState);

			if (_categoryRepository.CategoryExists(categoryDto.Name))
			{
				ModelState.AddModelError("CustomError", "La categoria ya existe");
				return BadRequest(ModelState);
			}

		   var category = categoryDto.Adapt<Category>();

			if (!_categoryRepository.CreateCategory(category))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al guardar el registro {category.Name}");
				return StatusCode(500, ModelState);
			}

			return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
		}


		[HttpPatch("{id:int}", Name = "UpdateCategory")]//obtener un id del query string
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
		{
			if (updateCategoryDto == null)
				return BadRequest(ModelState);

			if (!_categoryRepository.CategoryExists(id))
				return NotFound($"La categoria con id {id} no existe");


			if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
			{
				ModelState.AddModelError("CustomError", "La categoria ya existe");
				return BadRequest(ModelState);
			}

		   var category = updateCategoryDto.Adapt<Category>();
			category.Id = id; // Aseguramos que el ID sea el correcto
			if (!_categoryRepository.UpdateCategory(category))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al guardar el registro {category.Name}");
				return StatusCode(500, ModelState);
			}

			return CreatedAtRoute("GetCategory", new { id = category.Id }, category);
		}

		[HttpDelete("{id:int}", Name = "DeleteCategory")]//obtener un id del query string
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult DeleteCategory(int id)
		{
			if (!_categoryRepository.CategoryExists(id))
				return NotFound($"La categoria con id {id} no existe");

			var category = _categoryRepository.GetCategory(id);

			if (category is null)
			{
				return NotFound($"La categoria con el Id {id} no existe");
			}

			if (!_categoryRepository.DeleteCategory(category))
			{
				ModelState.AddModelError("CustomError", $"Ocurrió un error al eliminar el registro {category.Name}");
				return StatusCode(500, ModelState);
			}

			return NoContent();
		}

	}
}
//Notas para video
//tipo de retorno de datos de un metodo de un controllador
//Ejemplo: OK() Ok(object), BadRequest(), NotFound(), NoContent(), CreatedAtRoute(), CreatedAtAction() etc
///decoradores de tipos de peticions post, get, put, delete
///LOs tipo de datos que puede recibir un controlador son: 
///ejemplos: FromBody, FromQuery, FromRoute, FromHeader, FromForm y como obtenerlos y manipularlos