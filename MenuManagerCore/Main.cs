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
    public override string ModuleVersion => "0.7";
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

        Control.Init(this);
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
            var menu = _api.NewMenu(Localizer["menumanager.select_type"]);
            menu.PostSelectAction = PostSelectAction.Close;
            menu.AddMenuOption(Localizer["menumanager.console"], (player, option) => { Misc.SelectPlayerMenu(player, MenuType.ConsoleMenu); });
            menu.AddMenuOption(Localizer["menumanager.chat"], (player, option) => { Misc.SelectPlayerMenu(player, MenuType.ChatMenu); });
            menu.AddMenuOption(Localizer["menumanager.center"], (player, option) => { Misc.SelectPlayerMenu(player, MenuType.CenterMenu); });
            menu.AddMenuOption(Localizer["menumanager.control"], (player, option) => { Misc.SelectPlayerMenu(player, MenuType.ButtonMenu); });
            menu.Open(player);
        }

    }
}