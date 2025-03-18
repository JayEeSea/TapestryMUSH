# TapestryMUSH
TapestryMUSH is a modern reimagining of classic PennMUSH servers, built in C++ with extensibility, performance, and ease of scripting in mind.

## Features
- Text-based multiplayer world engine
- Modern, efficient networking
- Lua/Python scripting for game logic
- Adaptable for any style of game

## Getting Started

### Prerequisites
- A **C++ compiler** (GCC/Clang/MinGW)
- **CMake** for build automation
- **Git** for version control

### Installation
#### 1.Clone the repository:
```bash
git clone https://github.com/YOUR_USERNAME/TapestryMUSH.git
cd TapestryMUSH
```

#### 2. Build the Project with CMake
```bash
mkdir build
cd build
cmake ..
make
```

#### 3. Run the Server
```bash
./tapestrymush
```

#### 4. Connect to the server
Using Telnet:
```bash
telnet localhost 4000
```

### Troubleshooting
If `cmake ..` fails, ensure CMake is installed:
```bash
sudo apt install cmake -y
```

If `make` gives an error about missing `g++`, install it
```bash
sudo apt install g++ -y

```

## Contributing
Contributions are welcome! Feel free to open an issue or submit a pull request.

## License
[GNU GENERAL PUBLIC LICENSE Version 3](https://github.com/JayEeSea/TapestryMUSH?tab=GPL-3.0-1-ov-file)