using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MatriculaCMP.Server.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		}


		public DbSet<Usuario> Usuarios { get; set; }
		public DbSet<Menu> Menu { get; set; }
		public DbSet<Perfil> Perfil { get; set; }
		public DbSet<PerfilMenu> PerfilMenu { get; set; }
		// public DbSet<NombreClase> NombreTablaEnBaseDeDatos { get; set; }
	}
}
