using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnvLoader : MonoBehaviour
{
    private static Dictionary<string, string> envVariables = new Dictionary<string, string>();
    private static bool isLoaded = false;

    // Called automatically when the script is loaded
    void Awake()
    {
        LoadAndPrintEnv();
    }

    // Load .env file and print all variables
    public void LoadAndPrintEnv()
    {
        if (isLoaded) return;

        string filePath = Path.Combine(Application.dataPath, ".env");
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
                {
                    string[] parts = line.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim().Trim('"');
                        envVariables[key] = value;
                        Debug.Log($"[ENV] {key} = {value}");
                    }
                }
            }

            isLoaded = true;
            Debug.Log("Environment variables loaded and printed successfully.");
        }
        else
        {
            Debug.LogWarning(".env file not found at: " + filePath);
        }
    }

    // Accessor to get an environment variable
    public static string Get(string key)
    {
        if (envVariables.TryGetValue(key, out string value))
        {
            return value;
        }
        return null;
    }
}
