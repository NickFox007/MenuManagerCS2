using CounterStrikeSharp.API;
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
    internal static class Control
    {
        public static List<PlayerInfo> menus = new List<PlayerInfo>();
        private static BasePlugin hPlugin;

        public static void AddMenu(CCSPlayerController player, ButtonMenu inst)
        {
            for(int i = 0; i < menus.Count; i++)
                if (menus[i].GetPlayer() == player)
                {
                    menus.Remove(menus[i]);
                    i++;
                }

            var menu = new PlayerInfo(player, inst);
            menus.Add(menu);
        }

        public static void AddMenuAll(ButtonMenu inst)
        {
            var players = Utilities.GetPlayers();
            foreach (var player in players)
            {
                if(player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.Connected == PlayerConnectedState.PlayerConnected)
                    AddMenu(player, inst);
            }
        }

        public static void Clear()
        {            
            menus.RemoveAll(x => true);
        }

        public static void OnPluginTick()
        {
            if(menus.Count > 0)
            {
                //foreach(var menu in menus)
                for(int i = 0; i < menus.Count; i++)
                {
                    var menu = menus[i];
                    var player = menu.GetPlayer();
                    if(!Misc.IsValidPlayer(player))
                    {
                        menus.Remove(menu);
                        i--;
                        continue;
                    }
                    var buttons = player.Buttons;
                    player.PlayerPawn.Value.VelocityModifier = 0.0f;
                    // For ButtonMenu
                    //menu.GetPlayer().PrintToChat("Вот тебе меню .!.");

                    
                    if (!menu.IsEqualButtons(buttons.ToString()))
                    {

                        if (buttons.HasFlag(PlayerButtons.Forward))
                            menu.MoveUp();
                        else if (buttons.HasFlag(PlayerButtons.Back))
                            menu.MoveDown();
                        else if (buttons.HasFlag(PlayerButtons.Moveleft))
                            menu.MoveUp(7);
                        else if (buttons.HasFlag(PlayerButtons.Moveright))
                            menu.MoveDown(7);
                        else if (buttons.HasFlag(PlayerButtons.Use))
                            menu.OnSelect();

                        if (buttons.HasFlag(PlayerButtons.Reload) || menu.Closed())
                        {
                            menus.Remove(menu);
                            i--;
                        }
                    }

                    menu.GetPlayer().PrintToCenterHtml(menu.GetText());
                }
            }
        }    
        
        internal static void SetPlugin(BasePlugin _hPlugin)
        {
            hPlugin = _hPlugin;
        }

        internal static BasePlugin GetPlugin()
        {
            return hPlugin;
        }
    }
}
