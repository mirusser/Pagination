
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

await using var ctx = new BloggingContext();

// example of offset pagination (by using 'Skip' and 'Take' methods)
var someBlogs = await ctx.Blogs
    .OrderBy(b => b.Id)
    .Skip(20)
    .Take(10) // page size
    .ToArrayAsync();

var someBlogs2 = await ctx.Blogs
    .OrderBy(b => b.Id)
    .Where(b => b.Id > 20)
    .Take(10) // page size
    .ToArrayAsync();
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql("Host=your_host;Username=your_user;Password=super_secret_password;Database=demo")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
}

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime LastUpdated { get; set; }
}