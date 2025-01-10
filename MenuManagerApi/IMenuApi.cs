using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    public interface IMenuApi
    {
        public IMenu GetMenu(string title, Action<CCSPlayerController> back_action = null, Action<CCSPlayerController> reset_action = null);

        // Deprecated, only for backward compatibility
        [Obsolete("Method used only for backward compatibility, not for develope.", true)]
        public IMenu NewMenu(string title, Action<CCSPlayerController> back_action = null);
        //

        public IMenu GetMenuForcetype(string title, MenuType type, Action<CCSPlayerController> back_action = null, Action<CCSPlayerController> reset_action = null);

        // Deprecated, only for backward compatibility
        [Obsolete("Method used only for backward compatibility, not for develope.", true)]
        public IMenu NewMenuForcetype(string title, MenuType type, Action<CCSPlayerController> back_action = null);
        // 

        public void CloseMenu(CCSPlayerController player);
        public MenuType GetMenuType(CCSPlayerController player);
        public bool HasOpenedMenu(CCSPlayerController player);
    }

    public enum MenuType
    {
        Default = -1,
        ChatMenu = 0,
        ConsoleMenu = 1,
        CenterMenu = 2,
        ButtonMenu = 3
    }
}
