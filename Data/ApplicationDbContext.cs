using ApiEcommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Data;
//para que la migracion funcione bien usando identity heredamos de IdentityDbcontext
//para que se pueda hacer la migracion bien
public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}


	public DbSet<Category> Categories { get; set; }
	public DbSet<Product> Products { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<ApplicationUser> ApplicationUsers { get; set; }//usamos ApplicationUser por que hereda de user de identity

}
