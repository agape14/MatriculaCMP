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

		public DbSet<Usuario> Usuarios { get; set; }
		public DbSet<Menu> Menu { get; set; }
		public DbSet<Perfil> Perfil { get; set; }
		public DbSet<PerfilMenu> PerfilMenu { get; set; }
        // public DbSet<NombreClase> NombreTablaEnBaseDeDatos { get; set; }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<ConsejoRegional> ConsejoRegionales { get; set; }
        public DbSet<EstadoCivil> EstadoCiviles { get; set; }
        public DbSet<TipoDocumento> TipoDocumentos { get; set; }
        public DbSet<GrupoSanguineo> GrupoSanguineos { get; set; }
        public DbSet<Universidad> Universidades { get; set; }
        public DbSet<Ubigeo> Ubigeos { get; set; }
        public DbSet<ZonaDomicilio> ZonaDomicilios { get; set; }
        public DbSet<ViaDomicilio> ViaDomicilios { get; set; }
        public DbSet<Educacion> Educaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Educacion>()
                .HasOne(e => e.Persona)          // Cada Educación tiene una Persona
                .WithMany(p => p.Educaciones)    // Cada Persona tiene muchas Educaciones
                .HasForeignKey(e => e.PersonaId) // La FK es PersonaId
                .OnDelete(DeleteBehavior.Cascade); // Opcional: Eliminar en cascada
        }
    }
}
