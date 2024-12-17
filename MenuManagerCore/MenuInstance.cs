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
        Action<CCSPlayerController> BackAction;

        MenuType forcetype;

        public MenuInstance(string _title, Action<CCSPlayerController> _back_action = null, MenuType _forcetype = MenuType.Default)
        {
            Title = _title;
            ExitButton = true;
            MenuOptions = new List<ChatMenuOption>();
            BackAction = _back_action;
            forcetype = _forcetype;
        }

        public string Title {get;set;}
                
        public List<ChatMenuOption> MenuOptions { get; }

        public bool ExitButton { get; set; }

        public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

        public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
        {
            var option = new ChatMenuOption(display, disabled, (player, opt) => { onSelect(player, opt); });
            MenuOptions.Add(option);
            return option;            
        }

        public void OnBackAction(CCSPlayerController player)
        {
            Control.PlaySound(player, Control.GetPlugin().Config.SoundBack);
            BackAction(player);
        }

        public void Open(CCSPlayerController player)
        {

            IMenu menu = null;

            if (forcetype == MenuType.Default)
                forcetype = Misc.GetCurrentPlayerMenu(player);

            switch (forcetype)
            {
                case MenuType.ChatMenu: menu = new ChatMenu(Title); break;
                case MenuType.ConsoleMenu: menu = new ConsoleMenu(Title);  break;
                case MenuType.CenterMenu: menu = new CenterHtmlMenu(Title, Control.GetPlugin()); break;
                case MenuType.ButtonMenu: menu = new ButtonMenu(Misc.ColorText(Title));  break;
            }

            menu.ExitButton = ExitButton;
            menu.PostSelectAction = PostSelectAction;

            if (BackAction != null)
            {
                menu.AddMenuOption(Misc.ColorText(Control.GetPlugin().Localizer["menumanager.back"]), (p,d) => OnBackAction(p));
                if (forcetype == MenuType.ButtonMenu)
                    ((ButtonMenu)menu).BackAction = OnBackAction;
            }

            foreach(var option in MenuOptions)
                menu.AddMenuOption(option.Text, option.OnSelect, option.Disabled);
            
            menu.Open(player);

        }

        public void OpenToAll()
        {
            foreach (var player in Misc.GetValidPlayers())
                Open(player);
        }
    }
}
