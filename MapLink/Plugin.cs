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
    private const string MapLinkMainCommand = "/maplink";
    private const string MapLinkOnArg = "on";
    private const string MapLinkOffArg = "off";

    public readonly WindowSystem WindowSystem = new("MapLink");

    public Plugin(IChatGui chatGui, IGameGui gameGui, IDataManager dataManager)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(MapLinkMainCommand, new CommandInfo(OnCommand)
        {
            HelpMessage = "Display main menu"
        });
        CommandManager.DispatchCommand(MapLinkMainCommand, MapLinkOnArg, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle on"
        });
        CommandManager.DispatchCommand(MapLinkMainCommand, MapLinkOffArg, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggle off"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

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
    private MainWindow MainWindow { get; init; }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(MapLinkMainCommand);
    }

    private void Chat_OnChatMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (!Configuration.IsPluginEnabled)
            return;

        var players = Configuration.Players;
        var showMessage = players.Keys.Count == 0 || players.Keys.Contains(sender.TextValue);
        foreach (var payload in message.Payloads)
            if (showMessage && payload is MapLinkPayload mapLinkPayload)
                PlaceMapMarker(mapLinkPayload.TerritoryType.RowId, mapLinkPayload.XCoord, mapLinkPayload.YCoord);
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
                ToggleMainUI();
                break;
            case MapLinkOnArg:
                Configuration.IsPluginEnabled = true;
                Configuration.Save();
                break;
            case MapLinkOffArg:
                Configuration.IsPluginEnabled = false;
                Configuration.Save();
                break;
        }
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void ToggleConfigUI()
    {
        ConfigWindow.Toggle();
    }

    public void ToggleMainUI()
    {
        MainWindow.Toggle();
    }
}
