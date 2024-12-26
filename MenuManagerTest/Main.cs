using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using MenuManager;

public class MenuManagerTest : BasePlugin
{
    public override string ModuleName => "MenuManager [Test]";
    public override string ModuleVersion => "1.1.2";
    public override string ModuleAuthor => "Nick Fox";
    public override string ModuleDescription => "MenuManager Test Module";

    private IMenuApi? _api;
    private readonly PluginCapability<IMenuApi?> _pluginCapability = new("menu:nfcore");    

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = _pluginCapability.Get();
        if (_api == null) Console.WriteLine("MenuManager Core not found...");
    }

    [ConsoleCommand("mm_test", "Test menu!")]
    public void OnCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player != null)
        {            
            var menu = _api.GetMenu("Ticklingig");
            for (int i = 0; i < 10; i++)
                menu.AddMenuOption($"itemline{i}", (player, option) => { player.PrintToChat($"Selected: {option.Text}"); });
            menu.Open(player);
            
        }
        

    }
}