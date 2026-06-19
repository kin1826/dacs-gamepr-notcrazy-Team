using Microsoft.EntityFrameworkCore;
using GameNotCrazy.API.Models;

namespace GameNotCrazy.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<PaymentRequest> PaymentRequests { get; set; }
}