using Microsoft.EntityFrameworkCore;

namespace auction.Models
{
    public class TestContext: DbContext
    {
        public TestContext(DbContextOptions options) : base (options) {}
        public DbSet<User> Users {get;set;}
        public DbSet<Auction> Auctions {get;set;}
    }
}