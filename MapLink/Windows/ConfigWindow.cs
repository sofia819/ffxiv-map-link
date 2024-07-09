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

    public ConfigWindow(Plugin plugin) : base("Map Link Config###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

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

        ImGui.Spacing();

        foreach (var player in Configuration.Players)
        {
            var isPlayerEnabled = player.Value;

            ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(0, 128, 0, 255));
            if (ImGui.Checkbox(player.Key, ref isPlayerEnabled))
            {
                Configuration.Players[player.Key] = isPlayerEnabled;
                Configuration.Save();
            }

            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(255, 0, 0, 255));
            ImGui.PushID(player.Key);
            if (ImGui.SmallButton("X"))
            {
                Configuration.Players.Remove(player.Key);
                Configuration.Save();
                ImGui.PopID();
            }

            ImGui.PopStyleColor();

            ImGui.Spacing();
        }
    }
}
