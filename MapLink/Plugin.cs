using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using MapLink.Windows;

namespace MapLink;

public sealed class Plugin : IDalamudPlugin
{
    public readonly WindowSystem WindowSystem = new(PluginName);
    public Configuration Configuration { get; init; }
    public const string PluginName = "MapLink";

    private const string MapLinkCommand = "/mpl";
    private const string MapLinkConfigCommand = "/mpl cfg";
    private readonly TimeSpan timeBetweenMapLinks = TimeSpan.FromSeconds(20);
    private ConfigWindow ConfigWindow { get; init; }
    private MapLinkPayload? lastMapLink;
    private DateTime lastUpdate = DateTime.MinValue;

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal static IGameGui GameGui { get; private set; } = null!;

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(
            MapLinkCommand,
            new CommandInfo(OnCommand) { HelpMessage = "Toggles on/off" }
        );
        CommandManager.AddHandler(
            MapLinkConfigCommand,
            new CommandInfo(OnCommand) { HelpMessage = "Opens settings" }
        );
        CommandManager.AddHandler(
            $"{MapLinkCommand} Player Name",
            new CommandInfo(OnCommand) { HelpMessage = "Adds player to filtered list" }
        );

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(MapLinkCommand);
        CommandManager.RemoveHandler(MapLinkConfigCommand);
    }

    private void OnCommand(string command, string args)
    {
        switch (args)
        {
            case "":
                Configuration.IsPluginEnabled = !Configuration.IsPluginEnabled;
                ChatGui.Print(Configuration.IsPluginEnabled ? "ON" : "OFF", PluginName);
                Configuration.Save();
                break;
            case "cfg":
                ToggleConfigUI();
                break;
            default:
                // handle player name
                Configuration.SavePlayerName(args);

                break;
        }
    }

    private void OnChatMessage(
        XivChatType type,
        int timestamp,
        ref SeString sender,
        ref SeString message,
        ref bool isHandled
    )
    {
        if (!Configuration.IsPluginEnabled)
            return;

        foreach (var payload in message.Payloads)
            if (payload is MapLinkPayload mapLinkPayload && ShouldFollowMapLink(mapLinkPayload))
            {
                // Filter Sonar and players
                if (!ShouldFollowMessageFromSender(sender))
                    return;

                GameGui.OpenMapWithMapLink(mapLinkPayload);
                if (Configuration.IsLoggingEnabled)
                {
                    ChatGui.Print($"{sender.TextValue} posts a map link", PluginName);
                }
            }
    }

    private bool ShouldFollowMessageFromSender(SeString sender)
    {
        if (sender.TextValue.ToLower().Equals("sonar"))
            return false;

        var players = Configuration.Players;
        /*
         * 1. Check if there are any filtered players
         * 2. Check if all entries are disabled
         */
        if (players.Keys.Count == 0 || players.Values.All(enabled => !enabled))
        {
            return true;
        }

        // 3. Check if player name exists in filter and is enabled
        foreach (var playerKey in players.Keys)
        {
            if (sender.TextValue.Contains(playerKey))
            {
                return players[playerKey];
            }
        }

        return false;
    }

    private bool ShouldFollowMapLink(MapLinkPayload payload)
    {
        /*
         * 1. If map coordinates are different or
         * 2. Certain amount of time has elapsed since last identical map link
         */
        var currentTime = DateTime.Now;
        if (
            payload.RawX != lastMapLink?.RawX
            || payload.RawY != lastMapLink?.RawY
            || (lastUpdate - currentTime).Duration().Seconds > timeBetweenMapLinks.Seconds
        )
        {
            lastUpdate = currentTime;
            lastMapLink = payload;
            return true;
        }

        return false;
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void ToggleConfigUI()
    {
        ConfigWindow.Toggle();
    }
}
