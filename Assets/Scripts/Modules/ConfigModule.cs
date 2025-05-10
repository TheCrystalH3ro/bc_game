using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class ConfigModule
    {
        public static bool isLoaded = false;

        private static Dictionary<string, string> _config;

        public static Dictionary<string, string> Config 
        { 
            get
            {
                if(_config == null)
                {
                    _config =  new Dictionary<string, string>();
                }

                return _config;
            } 
        }

        public static void LoadConfig()
        {
            Config.Clear();

            string envPath = Path.Combine(Application.streamingAssetsPath, ".env");

            if (File.Exists(envPath))
            {
                foreach (string line in File.ReadAllLines(envPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    Config[key] = value;
                }
            }
        
            isLoaded = true;
        }

        public static string Get(string key, string defaultValue = "")
        {
            if(!isLoaded) LoadConfig();
            return Config.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            if(!isLoaded) LoadConfig();
            return Config.TryGetValue(key, out var value) && bool.TryParse(value, out var result) ? result : defaultValue;
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            if(!isLoaded) LoadConfig();
            return Config.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : defaultValue;
        }
    
    }
}