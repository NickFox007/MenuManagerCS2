using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    public class MenuInstance : IMenu
    {
        MenuType forcetype;

        public MenuInstance(string _title, MenuType _forcetype = MenuType.Default)
        {
            Title = _title;
            forcetype = _forcetype;
            MenuOptions = new List<ChatMenuOption>();
        }

        public string Title {get;set;}
                
        public List<ChatMenuOption> MenuOptions { get; }

        public bool ExitButton { get; set; }

        public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
        {
            var option = new ChatMenuOption(display, disabled, (player, opt) => { onSelect(player, opt); });
            MenuOptions.Add(option);
            return option;            
        }

        public void Open(CCSPlayerController player)
        {
            if (forcetype == MenuType.Default)
                forcetype = Misc.GetCurrentPlayerMenu(player);

            IMenu menu = null;
            switch(forcetype)
            {
                case MenuType.ChatMenu: menu = new ChatMenu(Title); break;
                case MenuType.ConsoleMenu: menu = new ConsoleMenu(Title);  break;
                case MenuType.CenterMenu: menu = new CenterHtmlMenu(Title, Control.GetPlugin()); break;
                case MenuType.ButtonMenu: menu = new ButtonMenu(Title);  break;
            }

            foreach(var option in MenuOptions)
                menu.AddMenuOption(option.Text, option.OnSelect, option.Disabled);
            //Control.AddMenu(player, this, forcetype);
            menu.Open(player);

        }

        public void OpenToAll()
        {
            foreach (var player in Misc.GetValidPlayers())
                Open(player);
        }

        public MenuType Type()
        {
            return forcetype;
        }

    }
}
