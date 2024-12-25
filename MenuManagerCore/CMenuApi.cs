using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuManager;

namespace MenuManager
{
    internal class CMenuApi: IMenuApi
    {
        MenuManagerCore plugin;
        //public CApi(BasePlugin _plugin)
        public CMenuApi(MenuManagerCore _plugin)
        {
            plugin = _plugin;
        }

        public IMenu NewMenu(string title, Action<CCSPlayerController> back_action = null, Action<CCSPlayerController> reset_action = null)
        {
            return new MenuInstance(title, back_action, reset_action);
        }

        public IMenu NewMenuForcetype(string title, MenuType type, Action<CCSPlayerController> back_action = null, Action<CCSPlayerController> reset_action = null)
        {
            return new MenuInstance(title, back_action, reset_action, type);
        }

        public void CloseMenu(CCSPlayerController player)
        {
            Control.CloseMenu(player);
        }

        public MenuType GetMenuType(CCSPlayerController player)
        {
            return Misc.GetCurrentPlayerMenu(player);
        }
       
    }
}
