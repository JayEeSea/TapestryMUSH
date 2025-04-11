using System.Net.Sockets;
using System.Net;
using TapestryMUSH.Core.Services;
using TapestryMUSH.Core.Session;
using TapestryMUSH.Core.Commands;

public class TelnetServer
{
    private readonly TcpListener _listener;
    private readonly List<ClientSession> _sessions = new();
    private readonly AccountService _accountService;
    private readonly CommandRouter _commandRouter;

    public TelnetServer(int port, AccountService accountService, CommandRouter commandRouter)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _accountService = accountService;
        _commandRouter = commandRouter;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine("TapestryMUSH Telnet server started on port 4201");

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_listener.Pending())
            {
                var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);
                var session = new ClientSession(tcpClient, _accountService, _commandRouter);
                _sessions.Add(session);
                _ = session.RunAsync();
            }

            await Task.Delay(100);
        }

        _listener.Stop();
    }
}