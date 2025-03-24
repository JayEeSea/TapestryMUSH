cmake_minimum_required(VERSION 3.10)
project(TapestryMUSH)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED True)

# Add all cpp files in the src directory to the SOURCES variable
file(GLOB SOURCES "src/*.cpp")

# Create executable
add_executable(tapestrymush ${SOURCES})

# Tell the compiler where to find header files
target_include_directories(tapestrymush PRIVATE ${PROJECT_SOURCE_DIR}/hdrs)