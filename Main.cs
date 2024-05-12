using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using PlayerSettings;


namespace MenuManager;
public class MenuManagerCore : BasePlugin
{
    public override string ModuleName => "MenuManager [Core]";
    public override string ModuleVersion => "0.4.1";
    public override string ModuleAuthor => "Nick Fox";
    public override string ModuleDescription => "All menus interacts in one core";

    private IMenuApi? _api;
    private ISettingsApi? _settings;
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");
    private readonly PluginCapability<ISettingsApi?> _settingsCapability = new("settings:nfcore");
    public override void Load(bool hotReload)
    {
        _api = new CMenuApi(this);
        Capabilities.RegisterPluginCapability(_pluginCapability, () => _api);

        Control.SetPlugin(this);
        RegisterListener<Listeners.OnTick>(() => { Control.OnPluginTick(); });
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _settings = _settingsCapability.Get();
        if (_settings == null) Console.WriteLine("PlayerSettings core not found...");
        Misc.SetSettingApi(_settings);
    }

    public override void Unload(bool hotReload){
        Control.Clear();
    }

    [ConsoleCommand("css_menus", "Choose menu type")]
    public void OnCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null)
        {
            var menu = _api.NewMenu("Выбор меню");
            menu.AddMenuOption("Консольное", (player, option) => { Misc.SelectPlayerMenu(player, MenuType.ConsoleMenu); });
            menu.AddMenuOption("Чат-меню", (player, option) => { Misc.SelectPlayerMenu(player, MenuType.ChatMenu); });
            menu.AddMenuOption("Стандартное (центр)", (player, option) => { Misc.SelectPlayerMenu(player, MenuType.CenterMenu); });
            menu.AddMenuOption("Управляемое (центр)", (player, option) => { Misc.SelectPlayerMenu(player, MenuType.ButtonMenu); });
            menu.Open(player);

        }

    }
}