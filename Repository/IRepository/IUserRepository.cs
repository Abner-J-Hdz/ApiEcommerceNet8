using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

namespace ApiEcommerce.Repository.IRepository;
public interface IUserRepository
{
	// GetUsers
	//        → Devuelve todos los usuarios en ICollection del tipo User.
	ICollection<ApplicationUser> GetUsers();

	//    - GetUser
	//        → Recibe un id y devuelve un solo objeto User o null si no se encuentra.
	ApplicationUser? GetUser(string userId);

	//    - IsUniqueUser
	//        → Recibe un nombre de usuario y devuelve un bool indicando si el nombre de usuario es único.
	bool IsUniqueUser(string userName);

	//    - Login
	//        → Recibe un objeto UserLoginDto y devuelve un UserLoginResponseDto de forma asíncrona (Task).
	Task<UserLoginResponseDto?> Login(UserLoginDto userLoginDto);

	//    - Register
	//        → Recibe un objeto CreateUserDto y devuelve un objeto User de forma asíncrona (Task).
	Task<UserDataDto?> Register(CreateUserDto createUserDto);
}
