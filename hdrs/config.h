#pragma once

#include <map>
#include <string>

std::string get_config_value(const std::string& key, const std::string& default_value);

extern std::map<std::string, std::string> config;