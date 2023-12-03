using Microsoft.EntityFrameworkCore;
using WebApiMyNote.Entities;

namespace WebApiMyNote.Data
{
    public class NoteBookDbContext : DbContext
    {
        public NoteBookDbContext(DbContextOptions<NoteBookDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<User> Users => Set<User>();
        public DbSet<NoteBook> Notes => Set<NoteBook>();
    }
}
