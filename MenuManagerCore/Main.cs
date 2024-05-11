using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Menu;


namespace MenuManager;
public class MenuManagerCore : BasePlugin
{
    public override string ModuleName => "MenuManager [Core]";
    public override string ModuleVersion => "0.3";
    public override string ModuleAuthor => "Nick Fox";
    public override string ModuleDescription => "All menus interacts in one core";

    private IMenuApi? _api;
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");
    public override void Load(bool hotReload)
    {
        _api = new CMenuApi();
        Capabilities.RegisterPluginCapability(_pluginCapability, () => _api);

        Control.SetPlugin(this);
        RegisterListener<Listeners.OnTick>(() => { Control.OnPluginTick(); });        
    }

    public override void Unload(bool hotReload)
    {
        Control.Clear();
    }
}