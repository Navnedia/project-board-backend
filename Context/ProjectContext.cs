using Microsoft.EntityFrameworkCore;
using ProjectBoard.Models;

class ProjectContext: DbContext {

    public DbSet<Project> Projects { get; set; }

    public ProjectContext(DbContextOptions options): base(options) { }
}