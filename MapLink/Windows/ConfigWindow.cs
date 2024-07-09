using System;
using System.Numerics;
using System.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGui = ImGuiNET.ImGui;

namespace MapLink.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Map Link Config###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar;

        Size = new Vector2(200, 200);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
            Flags &= ~ImGuiWindowFlags.NoMove;
        else
            Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        var buffer = new byte[1000];
        if (ImGui.InputText("", buffer, 1000, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            var playerName = Encoding.ASCII.GetString(buffer).TrimEnd((Char)0);
            Configuration.Players[playerName] = true;
            Configuration.Save();
            Array.Clear(buffer, 0, buffer.Length);
        }

        foreach (var player in Configuration.Players)
        {
            ImGui.Text(player.Key);
            ImGui.SameLine();
            if (ImGui.Button("-"))
            {
                Configuration.Players.Remove(player.Key);
                Configuration.Save();
            }
        }
    }
}
