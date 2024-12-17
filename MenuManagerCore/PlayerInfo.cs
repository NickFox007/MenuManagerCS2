using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Core.Translations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using Serilog.Core;
using Microsoft.Extensions.Logging;

namespace MenuManager
{
    internal class PlayerInfo
    {
        CCSPlayerController player;
        public ButtonMenu menu;
        //Action<CCSPlayerController> backaction;

        private int offset;
        private int selected;
        private float prev_mod;
        private string prev_buttons;
        private bool closed;

        public PlayerInfo(CCSPlayerController _player, ButtonMenu _menu, float new_mod = 0.0f, int new_selected = 0, string old_title = "")
        {
            player = _player;
            menu = _menu;
            
            if(new_mod == 0.0f)
                prev_mod = player.PlayerPawn.Value.VelocityModifier;
            else
                prev_mod = new_mod;

            closed = false;
            offset = 0;
            if (_menu.Title == old_title)
            {
                if (_menu.MenuOptions.Count > new_selected)
                    selected = new_selected;
                else
                    selected = Math.Max(_menu.MenuOptions.Count - 1, 0);
            }
            else
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
            
            if(menu.MenuOptions.Count > 0)            
                for (int i = offset; i < Math.Min(offset + 7, menu.MenuOptions.Count); i++)
                {
                    var line = menu.MenuOptions[i].Text;
                    if (menu.MenuOptions[i].Disabled) line = $"<font color='#aaaaaa'>{line}</font>";
                    if (i == selected) line = $"► {line} ◄";

                    text = text + "<br>" + line;
                }
            else
                text = $"{text}<br><font color='#aaaaaa'>{Misc.ColorText(Control.GetPlugin().Localizer["menumanager.empty"])}</font>";

            return text + $"</font><br><font class='fontSize-s'>{Misc.ColorText(Control.GetPlugin().Localizer["menumanager.footer"])}</font>";
        }

        public bool MoveDown(int lines = 1)
        {
            Control.PlaySound(player, Control.GetPlugin().Config.SoundScroll);

            if (selected == menu.MenuOptions.Count - 1) return false;

            selected = Math.Min(selected + lines, menu.MenuOptions.Count-1);
            
            if (selected - offset > 6) offset = selected - 6;

            

            return true;
        }

        public bool MoveUp(int lines = 1)
        {
            Control.PlaySound(player, Control.GetPlugin().Config.SoundScroll);
            if (selected < 1)
            {
                selected = 0;
                return false;                
            }

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
            if (selected > -1 && selected < menu.MenuOptions.Count)
            {
                if (menu.MenuOptions[selected].Disabled)
                {
                    Control.PlaySound(player, Control.GetPlugin().Config.SoundDisabled);
                }
                else
                {                    
                    Control.PlaySound(player, Control.GetPlugin().Config.SoundClick);
                    try
                    {
                        menu.MenuOptions[selected].OnSelect(player, menu.MenuOptions[selected]);
                    }
                    catch (Exception e)
                    {
                        Control.GetPlugin().Logger.LogInformation($"Error was caused in calling plugin: {e.Message}\n{e.StackTrace}");
                    }
                    if (menu.PostSelectAction == PostSelectAction.Close)
                        Close();
                }

            }
        }

        public void Close(bool withsound = false)
        {
            if (!closed)
            {
                if (withsound)
                    Control.PlaySound(player, Control.GetPlugin().Config.SoundExit);
                closed = true;
            }
        }

        public bool Closed()
        {
            return closed;
        }

    }
}
