using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Configuration;

namespace MapLink;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool IsPluginEnabled { get; set; } = true;

    public bool IsLoggingEnabled { get; set; } = true;

    public Dictionary<string, bool> Players { get; set; } = new();

    private readonly Regex namePattern = new("^[A-Z][a-z-']{1,14}\\s[A-Z][a-z-']{1,14}$");

    public void SavePlayerName(String playerName)
    {
        if (namePattern.IsMatch(playerName) && playerName.Length <= 21)
        {
            Players[playerName] = true;
            Save();

            Plugin.ChatGui.Print($"{playerName} added successfully");

            return;
        }

        Plugin.ChatGui.Print($"Error adding {playerName}: Invalid format");
    }

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
