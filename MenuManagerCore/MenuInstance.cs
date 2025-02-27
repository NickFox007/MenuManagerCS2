using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    public class MenuInstance : IMenu
    {
        public Action<CCSPlayerController> BackAction;
        public Action<CCSPlayerController> ResetAction;

        MenuType forcetype;

        public MenuInstance(string _title, Action<CCSPlayerController> _back_action = null, Action<CCSPlayerController> _reset_action = null, MenuType _forcetype = MenuType.Default)
        {
            Title = _title;
            ExitButton = true;
            MenuOptions = new List<ChatMenuOption>();
            BackAction = _back_action;
            forcetype = _forcetype;
            ResetAction = _reset_action;
        }

        public string Title {get;set;}
                
        public List<ChatMenuOption> MenuOptions { get; }

        public bool ExitButton { get; set; }

        public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

        public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
        {
            var option = new ChatMenuOption(display, disabled, (p, opt) => onSelect(p, opt));
            MenuOptions.Add(option);
            return option;            
        }

        public void OnBackAction(CCSPlayerController player)
        {
            if (BackAction != null)
            {
                Control.PlaySound(player, Control.GetPlugin().Config.SoundBack);
                BackAction(player);
            }
        }

        public void OnResetAction(CCSPlayerController player)
        {
            if (ResetAction != null)
                ResetAction(player);
            else
                Control.GetPlugin().Logger.LogWarning($"Reset action is not passed to func! TITLE: {Title}");
        }

        public void Open(CCSPlayerController player)
        {
            IMenu menu = null;

            if (forcetype == MenuType.Default)
                forcetype = Misc.GetCurrentPlayerMenu(player);

            if (forcetype == MenuType.MetamodMenu && !MenusMM.Hooked())
                forcetype = MenuType.ButtonMenu;

            switch (forcetype)
            {
                case MenuType.ChatMenu: menu = new ChatMenu(Title); break;
                case MenuType.ConsoleMenu: menu = new ConsoleMenu(Title);  break;
                case MenuType.CenterMenu: menu = new CenterHtmlMenu(Title, Control.GetPlugin()); break;
                case MenuType.ButtonMenu: menu = new ButtonMenu(Title, false);  break;
                case MenuType.MetamodMenu: menu = new ButtonMenu(Title, true);  break;
            }

            menu.ExitButton = ExitButton;
            menu.PostSelectAction = PostSelectAction;

            if (BackAction != null)
            {
                menu.AddMenuOption(Control.GetPlugin().Localizer["menumanager.back"], (p,d) => OnBackAction(p));                

            }

            if (forcetype == MenuType.ButtonMenu)
            {
                ((ButtonMenu)menu).BackAction = OnBackAction;
                ((ButtonMenu)menu).ResetAction = OnResetAction;
            }
            else
            {
                var flag = forcetype == MenuType.CenterMenu;
                menu.Title = Misc.ColorText(menu.Title, flag);
                for (int i = 0; i < MenuOptions.Count; i++)
                    MenuOptions[i].Text = Misc.ColorText(MenuOptions[i].Text, flag);
            }


                foreach (var option in MenuOptions)
                    menu.AddMenuOption(option.Text, option.OnSelect, option.Disabled);
            
            if (Control.GetPlugin().Config.UseMetamodMenu && ((Control.GetPlugin().Config.UseMetamodMenuReplace && forcetype == MenuType.ButtonMenu) || (forcetype == MenuType.MetamodMenu)))
                MenusMM.PassMenuToMM(player, this);
            else
            {                
                MenusMM.ClosePlayerMenu(player.Slot);
                menu.Open(player);                
            }
        }

        public void OpenToAll()
        {
            foreach (var player in Misc.GetValidPlayers())
                Open(player);
        }
    }
}
