using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Translations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager
{
    internal class PlayerInfo
    {
        CCSPlayerController player;
        public ButtonMenu menu;

        private int offset;
        private int selected;
        private float prev_mod;
        private string prev_buttons;
        private bool closed;

        public PlayerInfo(CCSPlayerController _player, ButtonMenu _menu)
        {
            player = _player;
            menu = _menu;
            prev_mod = player.PlayerPawn.Value.VelocityModifier;

            closed = false;
            offset = 0;
            selected = 0;  
            prev_buttons = player.Buttons.ToString();
        }

        public CCSPlayerController GetPlayer()
        {
            return player;
        }           

        public float GetMod()
        {
            return prev_mod;
        }

        public string GetText()
        {
            string text = $"<font class='mono-spaced-font'>{menu.Title}</font><font class='fontSize-sm stratum-font'>";
            for (int i = offset; i < Math.Min(offset + 7, menu.MenuOptions.Count); i++)
            {
                var line = menu.MenuOptions[i].Text;
                if (menu.MenuOptions[i].Disabled) line = $"<font color='#aaaaaa'>{line}</font>";
                if (i == selected) line = $"► {line} ◄";

                text = text + "<br>" + line;
            }

            //text = text + "<br>" + "W - вверх D - вниз<br>E - выбор R - выход";

            return text + $"</font><br><font class='fontSize-s'>{Control.GetPlugin().Localizer["menumanager.footer"]}</font>";
        }

        public bool MoveDown(int lines = 1)
        {
            if (selected == menu.MenuOptions.Count - 1) return false;

            selected = Math.Min(selected + lines, menu.MenuOptions.Count-1);
            
            if (selected - offset > 6) offset = selected - 6;

            

            return true;
        }

        public bool MoveUp(int lines = 1)
        {
            if (selected == 0) return false;

            selected = Math.Max(selected - lines, 0);
            if (selected < offset) offset = selected;

            return true;
        }

        public bool IsEqualButtons(string buttons)
        {
            var flag = prev_buttons.Equals(buttons);
            prev_buttons = buttons;
            return flag;
        }

        public int Selected()
        {
            return selected;
        }

        public void OnSelect()
        {
            if (!menu.MenuOptions[selected].Disabled)
            {
                menu.MenuOptions[selected].OnSelect(player, menu.MenuOptions[selected]);
                if (menu.PostSelectAction == PostSelectAction.Close)
                    Close();
            }
        }

        public void Close()
        {            
            closed = true;
        }

        public bool Closed()
        {
            return closed;
        }

    }
}
