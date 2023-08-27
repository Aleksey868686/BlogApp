using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data
{
    public class MyAppDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public MyAppDataContext(DbContextOptions<MyAppDataContext> contextOptions) : base(contextOptions)
        {
            Database.EnsureCreated();
        }
    }
}
