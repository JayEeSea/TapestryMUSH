using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TapestryMUSH.Data;
using TapestryMUSH.Core;
using TapestryMUSH.Core.Services;
using System.Windows.Input;
using TapestryMUSH.Core.Commands;
using TapestryMUSH.Server.Commands;
using ICommand = TapestryMUSH.Core.Commands.ICommand;
using TapestryMUSH.Data.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        config.AddJsonFile(path, optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<AccountService>();
    })
    .Build();

// Create scope and resolve AccountService
using var scope = host.Services.CreateScope();
var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
var lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

if (!dbContext.Rooms.Any())
{
    var arrival = new Room
    {
        Name = "The Arrival Chamber",
        Description = "A sleek, metallic room dimly lit by recessed strips of blue lighting. This is where all newcomers awaken.",
        ExitsJson = "{}"
    };

    dbContext.Rooms.Add(arrival);
    await dbContext.SaveChangesAsync();
}

var commandRouter = new CommandRouter(new ICommand[]
{
    new LookCommand(),
    new SayCommand(),
    new PoseCommand(),
    new GoCommand(),
    new DigCommand(),
    new OpenCommand(),
    new TeleportCommand(),
    new SetCommand(),
    new BriefCommand(),
    new ShutdownCommand(lifetime),
    new ShutdownCommand(scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>())
});

// Pass it into the Telnet server
var telnetServer = new TelnetServer(4201, accountService, commandRouter);
await telnetServer.StartAsync(CancellationToken.None);