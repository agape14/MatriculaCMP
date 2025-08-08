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
       
        //Tablas maestras existentes en la bd
        public DbSet<Mat_ConsejoRegional> Mat_ConsejoRegional { get; set; }
		public DbSet<MaestroRegistro> MaestroRegistro { get; set; }
		public DbSet<Mat_Pais> MatPaises { get; set; }
		public DbSet<Mat_Ubigeo> MatUbigeos { get; set; }

        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<EstadoSolicitud> EstadoSolicitudes { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Correlativos> Correlativos { get; set; }
        public DbSet<SolicitudHistorialEstado> SolicitudHistorialEstados { get; set; }
        public DbSet<EducacionDocumento> EducacionDocumentos { get; set; }
        public DbSet<Diploma> Diplomas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Educacion>()
                .HasOne(e => e.Persona)          // Cada Educación tiene una Persona
                .WithMany(p => p.Educaciones)    // Cada Persona tiene muchas Educaciones
                .HasForeignKey(e => e.PersonaId) // La FK es PersonaId
                .OnDelete(DeleteBehavior.Cascade); // Opcional: Eliminar en cascada

			// Evita que EF intente crear o modificar la tabla en futuras migraciones
			modelBuilder.Entity<Mat_ConsejoRegional>(entity =>
			{
				entity.ToTable("Mat_ConsejoRegional");
				entity.HasKey(e => e.ConsejoRegional_Key);
			});

			modelBuilder.Entity<MaestroRegistro>(entity =>
			{
				entity.ToTable("MaestroRegistro");
				entity.HasKey(e => e.MaestroRegistro_Key);
			});
			
			modelBuilder.Entity<Mat_Pais>(entity =>
			{
				entity.ToTable("Mat_Pais");
				entity.HasKey(e => e.Pais_key);
			});

			modelBuilder.Entity<Mat_Ubigeo>(entity =>
			{
				entity.ToTable("Mat_Ubigeo");
				entity.HasKey(e => e.UbigeoKey);
			});

            modelBuilder.Entity<Solicitud>()
				.HasOne(s => s.EstadoSolicitud)
				.WithMany(e => e.Solicitudes)
				.HasForeignKey(s => s.EstadoSolicitudId)
				.OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Area)
                .WithMany(a => a.Solicitudes)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Persona>()
                .HasOne(p => p.GrupoSanguineo)
                .WithMany()
                .HasForeignKey(p => p.GrupoSanguineoId)
                .HasPrincipalKey(m => m.MaestroRegistro_Key);

            modelBuilder.Entity<Educacion>()
                .HasOne(e => e.Documento)
                .WithOne(d => d.Educacion)
                .HasForeignKey<EducacionDocumento>(d => d.EducacionId);

        }
    }
}
