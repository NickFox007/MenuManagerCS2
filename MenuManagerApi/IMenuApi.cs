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
        public IMenu NewMenu(string title, MenuType forcetype = MenuType.Default);
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
