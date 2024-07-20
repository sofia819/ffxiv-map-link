using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace MapLink;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public bool IsConfigWindowMovable { get; set; } = true;

    public bool IsPluginEnabled { get; set; } = true;

    public bool IsLoggingEnabled { get; set; } = true;
    
    public bool IgnoreSonar  { get; set; } = true;

    public Dictionary<string, bool> Players { get; set; } = new();

    public int Version { get; set; } = 1;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
