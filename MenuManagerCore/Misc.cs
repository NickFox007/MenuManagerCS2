using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerSettings;
using Microsoft.Extensions.Logging;


namespace MenuManager
{
    internal static class Misc
    {
        private static string DefaultMenu = "ButtonMenu";
        private static ISettingsApi? settings;
        public static void SetSettingApi(ISettingsApi _settings)
        {
            settings = _settings;
        }
        public static List<CCSPlayerController> GetValidPlayers()
        {
            var players = new List<CCSPlayerController>();
            foreach (var player in Utilities.GetPlayers())
            {
                if (player != null && player.IsValid && !player.IsBot && !player.IsHLTV && player.Connected == PlayerConnectedState.PlayerConnected)
                    players.Add(player);
            }

            return players;
        }

        public static void SetDefaultMenu(string DefaultMenu)
        {
            var menu_types = new List<string>(["ButtonMenu", "CenterMenu", "ConsoleMenu", "ChatMenu", "MetamodMenu"]);
            if (menu_types.Contains(DefaultMenu))
                Misc.DefaultMenu = DefaultMenu;
            else
            {
                Control.GetPlugin().Logger.LogInformation($"Invalid menu type: {DefaultMenu}. Using default menu {Misc.DefaultMenu}");
            }            
        }

        public static MenuType GetCurrentPlayerMenu(CCSPlayerController player)
        {         
            var res = settings.GetPlayerSettingsValue(player, "menutype", DefaultMenu);
            try
            {
                return (MenuType)Enum.Parse(typeof(MenuType), res);
            }
            catch(Exception _)
            {
                Control.GetPlugin().Logger.LogWarning($"Cannot cast MenuType for player {player.PlayerName} [{player.Slot}] (got value \"{res}\"). Using default {DefaultMenu}...");
                return (MenuType)Enum.Parse(typeof(MenuType), DefaultMenu);
            }
        }

        public static void SelectPlayerMenu(CCSPlayerController player, MenuType type)
        {
            var name = Enum.GetName(type.GetType(), type);
            settings.SetPlayerSettingsValue(player, "menutype", name);

            player.PrintToChat($"{Control.GetPlugin().Localizer["menumanager.selected_type"]} {Misc.GetMenuTypeName(type)}");
        }

        public static string GetMenuTypeName(MenuType type)
        {
            switch(type)
            {
                case MenuType.ChatMenu: return Control.GetPlugin().Localizer["menumanager.chat"];
                case MenuType.ConsoleMenu: return Control.GetPlugin().Localizer["menumanager.console"];
                case MenuType.CenterMenu: return Control.GetPlugin().Localizer["menumanager.center"];
                case MenuType.ButtonMenu: return Control.GetPlugin().Localizer["menumanager.control"];
                case MenuType.MetamodMenu: return Control.GetPlugin().Localizer["menumanager.metamod"];
                default: return "Undefined";
            }
        }

        public static bool IsValidPlayer(CCSPlayerController player)
        {
            if (player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected && !player.IsBot) return true;
            else return false;
        }

        internal static string ColorText(string text, bool need_colors = true)
        {
            var new_text = text;

            var colors = new List<string>(["Default", "White", "Darkred", "Green", "Lightyellow", "Lightblue", "Olive", "Lime", "Red", "Lightpurple", "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue", "Darkblue", "Bluegrey", "Magenta", "Lightred", "Orange"]);

            if (need_colors)
                foreach (var color0 in colors)
                {
                    var color = "[color:" + color0 + "]";
                    var color_old = "{" + color0 + "}";
                    var rep = $"<font color='{color0.ToLower()}'>";
                    new_text = new_text.Replace(color, rep, StringComparison.CurrentCultureIgnoreCase);
                    new_text = new_text.Replace(color_old, rep, StringComparison.CurrentCultureIgnoreCase); // For some backward compatibility..?                    
                }
            else
            {
                foreach (var color0 in colors)
                {
                    var color = "[color:" + color0 + "]";
                    var color_old = "{" + color0 + "}";
                    new_text = new_text.Replace(color, "", StringComparison.CurrentCultureIgnoreCase);
                    new_text = new_text.Replace(color_old, "", StringComparison.CurrentCultureIgnoreCase);
                }
                new_text = new_text.Replace("</font>","");
            }
            return new_text;
        }
                
    }

    
}
