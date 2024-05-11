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
        //BasePlugin plugin;
        //public CApi(BasePlugin _plugin)
        public CMenuApi()
        {
            //plugin = _plugin;
        }

        public IMenu NewMenu(string title, MenuType forcetype = MenuType.Default)
        {
            return new MenuInstance(title, forcetype);
        }


       
    }
}
