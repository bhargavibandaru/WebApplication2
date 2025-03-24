using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;

namespace WebApplication2.Context
{
    public class ApplicationDbContext : DbContext

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<ToDoItem> ToDoItems { get; set; }
        public DbSet<NewToDoItem> NewToDoItem {get; set;}
        public DbSet<Tasks> Tasks {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoItem>().ToTable("todoitems");
            modelBuilder.Entity<NewToDoItem>().ToTable("newtodoitem");
            modelBuilder.Entity<Tasks>().ToTable("tasks");
        }
        

    }
}
