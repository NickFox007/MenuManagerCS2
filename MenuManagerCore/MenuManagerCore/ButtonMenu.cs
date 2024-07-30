using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    public class ButtonMenu : IMenu
    {        
        public ButtonMenu(string _title)
        {
            MenuOptions = new List<ChatMenuOption>();
            Title = _title;
        }
        public string Title { get; set; }

        public List<ChatMenuOption> MenuOptions { get; }

        public bool ExitButton { get; set; }

        public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

        public ChatMenuOption AddMenuOption(string display, Action<CCSPlayerController, ChatMenuOption> onSelect, bool disabled = false)
        {
            var option = new ChatMenuOption(display, disabled, onSelect);
            MenuOptions.Add(option);            
            return option;            
        }

        public void Open(CCSPlayerController player)
        {
            Control.AddMenu(player, this);
        }

        public void OpenToAll()
        {
            Control.AddMenuAll(this);
        }
    }
}
