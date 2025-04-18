# TapestryMUSH

**TapestryMUSH** is a modern reimagining of the classic text-based MUSH (Multi-User Shared Hallucination) server, written in C# using the .NET platform. Designed for telnet-based roleplay and storytelling communities, it blends the spirit of legacy MUSH servers like PennMUSH with the power, modularity, and maintainability of contemporary software development practices.

> _“Weave your story. Shape your universe.”_

---

## 🌌 Features

- ✅ **Telnet Interface** – Connect using any standard telnet client (e.g. MUSHclient)
- 🧩 **Modular Command System** – Easily extendable with aliases and command routing
- 🛠️ **Object-Oriented Architecture** – Rooms, players, and exits modelled as persistent entities
- 🏗️ **Room & Exit Creation** – Use `@dig`, `@open`, `go`/`move`, and direct exit commands
- 🧙 **Permission System** – Access levels including `WIZARD`, `ROYALTY`, and more
- 🔒 **Authentication** – User creation, login, and secure session handling
- 📜 **Logging & Audit Trails** – Detailed logs for debugging and moderation
- 💾 **MariaDB via Entity Framework Core** – Clean data access with migration support
- 🔧 **External Scripting Support (planned)** – Lua or Python for extending game logic
- 🚀 **Cross-platform Ready** – Developed on Windows, deployable to Linux via .NET and Docker

---

## 💡 Philosophy

TapestryMUSH aims to provide a more maintainable and extensible codebase than legacy C-based MUSH servers, using modern tools and development practices. It is built to:

- Avoid softcode where possible
- Encourage external scripting for complex behaviour
- Stay true to the feel and command structure of traditional MUSH servers
- Prioritise security, reliability, and developer usability

---

## 🧰 Tech Stack

| Component        | Technology       |
|------------------|------------------|
| Language         | C# (.NET 8)      |
| Data Persistence | MariaDB + EF Core|
| Networking       | Telnet (TCP)     |
| Testing Client   | MUSHclient       |
| Deployment       | Linux VPS        |
| Dev Tools        | Visual Studio / VS Code, Docker, GitHub |

---

## 🗂️ Project Structure

```
TapestryMUSH/
├── Core/             # Core logic for commands, routing, and networking
├── Data/             # EF Core DbContext and database models
├── Commands/         # Modular command implementations
├── Services/         # Utility services (logging, security, etc.)
├── TelnetServer/     # TCP listener, session handling, protocol parsing
├── Scripts/          # (Planned) Lua/Python script integration
└── Program.cs        # Application entry point
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [MariaDB](https://mariadb.org/) (local or remote)
- Telnet client (e.g. [MUSHclient](http://www.gammon.com.au/mushclient/))

### Build & Run

```bash
git clone https://github.com/yourusername/TapestryMUSH.git
cd TapestryMUSH
dotnet build
dotnet run
```

Ensure `appsettings.json` is configured to connect to your MariaDB instance.

---

## 👥 Contributing

Contributions are welcome! Please check the [issues](https://github.com/JayEeSea/TapestryMUSH/issues) tab for feature requests and bugs.

To contribute:

1. Fork the repository
2. Create a new branch (`git checkout -b feature/new-command`)
3. Commit your changes
4. Open a Pull Request

---

## 📜 License

TapestryMUSH is released under the MIT License. See [LICENSE](LICENSE) for details.

---

## 🌠 Credits

- Inspired by **PennMUSH** and decades of MUSH storytelling tradition  
- Built with love for text-based worlds and the communities that bring them to life

---

## 🛰️ Roadmap

- [x] Modular command routing
- [x] Room and exit creation
- [x] Movement system
- [x] Authentication and roles
- [ ] Lua/Python scripting support
- [ ] In-game editing tools
- [ ] Web-based admin panel (optional)
