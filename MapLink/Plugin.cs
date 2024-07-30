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
using Lumina.Excel.GeneratedSheets2;
using MapLink.Windows;

namespace MapLink;

public sealed class Plugin : IDalamudPlugin
{
    private const string PluginName = "MapLink";
    private const string MapLinkCommand = "/mpl";
    private const string MapLinkConfigCommand = "/mpl cfg";
    private readonly TimeSpan timeBetweenMapLinks = TimeSpan.FromSeconds(20);

    public readonly WindowSystem WindowSystem = new(PluginName);
    private MapLinkPayload? lastMapLink;

    private DateTime lastUpdate = DateTime.MinValue;

    public Plugin(IChatGui chatGui, IGameGui gameGui, IDataManager dataManager)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(MapLinkCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle on/off"
        });
        CommandManager.AddHandler(MapLinkConfigCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens settings"
        });
        CommandManager.AddHandler($"{MapLinkCommand} Player Name", new CommandInfo(OnCommand)
        {
            HelpMessage = "Add player to filtered list"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        ChatGui = chatGui;
        GameGui = gameGui;
        DataManager = dataManager;


        ChatGui.ChatMessage += Chat_OnChatMessage;
    }

    public IChatGui ChatGui { get; }
    public IGameGui GameGui { get; }
    public IDataManager DataManager { get; }

    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    public Configuration Configuration { get; init; }
    private ConfigWindow ConfigWindow { get; init; }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(MapLinkCommand);
    }

    private void Chat_OnChatMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!Configuration.IsPluginEnabled)
            return;

        // Filter Sonar and players
        if (!shouldFollowMessageFromSender(sender)) return;

        foreach (var payload in message.Payloads)
            if (payload is MapLinkPayload mapLinkPayload &&
                shouldFollowMapLink(mapLinkPayload))
            {
                PlaceMapMarker(mapLinkPayload.TerritoryType.RowId, mapLinkPayload.XCoord, mapLinkPayload.YCoord);
                if (Configuration.IsLoggingEnabled)
                {
                    ChatGui.Print(
                        $"{sender.TextValue} posts a map link",
                        PluginName);
                }
            }
    }

    public void PlaceMapMarker(uint territoryTypeRowId, float coordX, float coordY)
    {
        var map = DataManager.GetExcelSheet<TerritoryType>().GetRow(territoryTypeRowId).Map;
        GameGui.OpenMapWithMapLink(new MapLinkPayload(territoryTypeRowId, map.Row, coordX, coordY));
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
                if (args.Split(" ").Length == 2)
                {
                    Configuration.Players[args] = true;
                    Configuration.Save();
                }

                break;
        }
    }

    private bool shouldFollowMessageFromSender(SeString sender)
    {
        if (sender.TextValue.ToLower().Equals("sonar"))
        {
            return false;
        }
        
        var players = Configuration.Players;
        /*
         * 1. Check if there are any filtered players
         * 2. Check if all entries are disabled
         * 3. Check filter
         */
        return players.Keys.Count == 0 || players.Values.All(enabled => !enabled) ||
               (players.ContainsKey(sender.TextValue) &&
                players[sender.TextValue]);
    }

    private bool shouldFollowMapLink(MapLinkPayload payload)
    {
        /*
         * 1. If map coordinates are different or
         * 2. Certain amount of time has elapsed since last identical map link
         */
        var currentTime = DateTime.Now;
        if (payload.RawX != lastMapLink?.RawX || payload.RawY != lastMapLink?.RawY ||
            (lastUpdate - currentTime).Duration().Seconds > timeBetweenMapLinks.Seconds)
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
