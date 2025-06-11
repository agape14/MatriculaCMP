using MatriculaCMP.Shared;
using Microsoft.EntityFrameworkCore;

namespace MatriculaCMP.Data
{
    public class SgdDbContext: DbContext
    {
        public SgdDbContext(DbContextOptions<SgdDbContext> options) : base(options) { }

        public DbSet<PersonaSGD> Persona { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonaSGD>().ToTable("Persona", "General");
        }
    }
}
