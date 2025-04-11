using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TapestryMUSH.Core.Session;
using TapestryMUSH.Data;
using TapestryMUSH.Data.Models;


namespace TapestryMUSH.Core.Services;
public class AccountService
{
    private readonly AppDbContext _dbContext;
    public AppDbContext DbContext => _dbContext;

    public AccountService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> CreateAccountAsync(string username, string password, string email)
    {
        if (await _dbContext.Players.AnyAsync(p => p.Username == username))
            return "That username is already taken.";

        var hash = HashPassword(password);

        var player = new Player
        {
            Username = username,
            PasswordHash = hash,
            Email = email,
            LastLogin = DateTime.UtcNow,
            CurrentRoomId = 1, // The Arrival Chamber
            Flags = new HashSet<string> { "PLAYER" }
        };

        _dbContext.Players.Add(player);
        await _dbContext.SaveChangesAsync();

        return "Account created successfully! You may now connect.";
    }

    // Connection
    public async Task<Player?> ConnectAsync(string username, string password)
    {
        var player = await _dbContext.Players
            .Include(p => p.CurrentRoom)
            .SingleOrDefaultAsync(p => p.Username == username);

        if (player == null)
            return null;

        if (!VerifyPassword(password, player.PasswordHash))
            return null;

        player.LastLogin = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return player;
    }

    // Simple hash using SHA256 for now (swap for bcrypt/argon2 in production)
    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string input, string storedHash)
    {
        var inputHash = HashPassword(input);
        return inputHash == storedHash;
    }

    public async Task<Player?> GetPlayerByClientAsync(ClientSession session)
    {
        if (session.CurrentPlayer != null)
            return session.CurrentPlayer;

        if (session.PlayerId == null)
        {
            Console.WriteLine("[DEBUG] No PlayerId in session.");
            return null;
        }

        Console.WriteLine($"[DEBUG] Loading PlayerId {session.PlayerId}");

        var player = await _dbContext.Players
            .Include(p => p.CurrentRoom)
            .FirstOrDefaultAsync(p => p.Id == session.PlayerId);

        return player;
    }

}
