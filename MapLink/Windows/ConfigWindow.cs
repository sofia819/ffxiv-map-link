using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGui = ImGuiNET.ImGui;

namespace MapLink.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin)
        : base("Map Link Config###MapLinkConfigWindow")
    {
        Flags = ImGuiWindowFlags.None;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(150, 200),
            MaximumSize = new Vector2(300, float.MaxValue)
        };

        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        // Enable plugin checkbox
        var isPluginEnabled = configuration.IsPluginEnabled;

        if (ImGui.Checkbox("Enabled", ref isPluginEnabled))
        {
            configuration.IsPluginEnabled = isPluginEnabled;
            configuration.Save();
        }

        // Enable logging checkbox
        var isLoggingEnabled = configuration.IsLoggingEnabled;

        if (ImGui.Checkbox("Log", ref isLoggingEnabled))
        {
            configuration.IsLoggingEnabled = isLoggingEnabled;
            configuration.Save();
        }

        ImGui.Spacing();

        ImGui.Text("Players");
        ImGui.Spacing();

        // Player input box
        var buffer = "";
        if (
            ImGui.InputTextWithHint(
                "",
                "Player Name",
                ref buffer,
                1000,
                ImGuiInputTextFlags.EnterReturnsTrue
            ) && buffer.Split(" ").Length is 1 or 2
        )
        {
            var playerName = buffer;
            configuration.Players[playerName] = true;
            configuration.Save();
        }

        ImGui.Spacing();

        // Player list
        foreach (var player in configuration.Players)
        {
            var isPlayerEnabled = player.Value;

            if (ImGui.Checkbox(player.Key, ref isPlayerEnabled))
            {
                configuration.Players[player.Key] = isPlayerEnabled;
                configuration.Save();
            }

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(255, 0, 0, 255));
            ImGui.PushID(player.Key);
            if (ImGui.SmallButton("X"))
            {
                configuration.Players.Remove(player.Key);
                configuration.Save();
                ImGui.PopID();
            }

            ImGui.PopStyleColor();

            ImGui.Spacing();
        }
    }
}
