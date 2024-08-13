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

    private readonly Regex sanitizePattern = new("['-]");

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public bool SavePlayerName(String playerName)
    {
        if (Validate(playerName))
        {
            Players[playerName] = true;
            Save();
            return true;
        }

        return false;
    }

    private bool Validate(String playerName)
    {
        var playerNameSplits = playerName.Split(" ");
        foreach (var split in playerNameSplits)
        {
            // Remove special characters (') and (-), then check if alphabet only
            var sanitized = sanitizePattern.Replace(split, "");
            if (!Regex.IsMatch(sanitized, @"^[A-Z][a-z]+$"))
            {
                return false;
            }
        }

        return playerNameSplits.Length is 1 or 2;
    }
}
