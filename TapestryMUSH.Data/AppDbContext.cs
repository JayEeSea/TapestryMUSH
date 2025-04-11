using Microsoft.EntityFrameworkCore;
using System.Numerics;
using TapestryMUSH.Data.Models;

namespace TapestryMUSH.Data;

public class AppDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Room> Rooms { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.CurrentRoom)
            .WithMany()
            .HasForeignKey(p => p.CurrentRoomId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Room>()
			.HasOne(r => r.NorthRoom)
			.WithMany()
			.HasForeignKey(r => r.NorthRoomId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Room>()
			.HasOne(r => r.SouthRoom)
			.WithMany()
			.HasForeignKey(r => r.SouthRoomId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Room>()
			.HasOne(r => r.EastRoom)
			.WithMany()
			.HasForeignKey(r => r.EastRoomId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Room>()
			.HasOne(r => r.WestRoom)
			.WithMany()
			.HasForeignKey(r => r.WestRoomId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
