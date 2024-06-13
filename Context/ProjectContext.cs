using Microsoft.EntityFrameworkCore;
using ProjectBoard.Models;

class ProjectContext: DbContext {

    public DbSet<Project> Projects { get; set; }
    public DbSet<User> Users { get; set; }

    public ProjectContext(DbContextOptions options): base(options) { }

    public ProjectContext(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite("Data Source=./Database/ProjectsDb.db");
    }
}