using Microsoft.EntityFrameworkCore;
using Notify.Models;

namespace Notify.Data;

// DbContext é a ponte entre o código C# e a base de dados
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet representa a tabela Notes — db.Notes.Add(...), db.Notes.ToList(), etc.
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite não suporta listas nativamente — guardamos as tags como CSV ("tag1,tag2")
        // e convertemos para List<string> ao ler, e vice-versa ao escrever
        modelBuilder.Entity<Note>()
            .Property(n => n.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
    }
}