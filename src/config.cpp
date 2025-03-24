#include "config.h"
#include "logs.h"
#include <map>

// Define the global configuration map
std::map<std::string, std::string> config;

// Function to retrieve a configuration value with a default fallback
std::string get_config_value(const std::string& key, const std::string& default_value) {
    if (config.count(key)) {
        return config[key]; // Return the value if the key exists
    } else {
        log(WARNING, "Config key '" + key + "' not found, using default value.");
        return default_value; // Return the default value if the key is not found
    }
}