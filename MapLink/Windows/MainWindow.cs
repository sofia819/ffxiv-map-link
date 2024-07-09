using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MapLink.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    private readonly Plugin Plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("Map Link##With a hidden ID", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(200, 200)
        };

        Plugin = plugin;
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("Players");
        foreach (var player in Configuration.Players)
            ImGui.BulletText($"{player.Key}");

        if (ImGui.Button("Show Settings")) Plugin.ToggleConfigUI();

        ImGui.Spacing();
    }
}
