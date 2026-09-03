using Microsoft.EntityFrameworkCore;
using biblibnj.Entities;

namespace biblibnj.Context

{
    public class BiblibnjDbContext : DbContext
    {
        public BiblibnjDbContext(DbContextOptions<BiblibnjDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<FilaEspera> FilaEspera { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamento Usuarios (RN01)
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Nome).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Perfil).HasMaxLength(20).IsRequired();
            });

            // Mapeamento Livros (RN02, RN03)
            modelBuilder.Entity<Livro>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ISBN).IsUnique();
                entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Autor).HasMaxLength(150).IsRequired();
                entity.Property(e => e.ISBN).HasMaxLength(20).IsRequired();
            });

            // Mapeamento Emprestimos
            modelBuilder.Entity<Emprestimo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId);

                entity.HasOne(e => e.Livro)
                      .WithMany()
                      .HasForeignKey(e => e.LivroId);
            });

            // Mapeamento FilaEspera (RN06 - Unicidade por Usuario e Livro)
            modelBuilder.Entity<FilaEspera>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.LivroId, e.UsuarioId }).IsUnique();

                entity.HasOne(e => e.Livro)
                      .WithMany()
                      .HasForeignKey(e => e.LivroId);

                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId);
            });
        }
    }
}

