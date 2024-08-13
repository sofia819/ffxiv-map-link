using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
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
            MinimumSize = new Vector2(220, 200),
            MaximumSize = new Vector2(220, 350)
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
        ImGui.PushItemWidth(200);
        if (
            ImGui.InputTextWithHint(
                "",
                "Player Name",
                ref buffer,
                30,
                ImGuiInputTextFlags.EnterReturnsTrue
            )
        )
        {
            var playerName = buffer;
            Plugin.ChatGui.Print(
                configuration.SavePlayerName(playerName)
                    ? $"{playerName} added successfully"
                    : $"Failed to add {playerName}",
                Plugin.PluginName
            );
        }
        ImGui.PopItemWidth();

        ImGui.Spacing();

        if (configuration.Players.Count == 0)
            return;

        // Player list
        if (ImGui.BeginTable("Players", 3, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("X", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("O", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Player");

            foreach (var player in configuration.Players)
            {
                ImGui.PushID(player.Key);

                // Remove player
                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
                {
                    configuration.Players.Remove(player.Key);
                    configuration.Save();
                    ImGui.PopID();
                }

                // Enable player
                ImGui.TableNextColumn();
                var isPlayerEnabled = player.Value;
                if (ImGui.Checkbox("", ref isPlayerEnabled))
                {
                    configuration.Players[player.Key] = isPlayerEnabled;
                    configuration.Save();
                }

                // Player name
                ImGui.TableNextColumn();
                ImGui.Text(player.Key);

                ImGui.PopID();
            }
        }
        ImGui.EndTable();
    }
}
