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
            var menu_types = new List<string>(["ButtonMenu", "CenterMenu", "ConsoleMenu", "ChatMenu"]);
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
            return (MenuType)Enum.Parse(typeof(MenuType), res);
        }

        public static void SelectPlayerMenu(CCSPlayerController player, MenuType type)
        {
            settings.SetPlayerSettingsValue(player, "menutype", Enum.GetName(type.GetType(), type));

            player.PrintToChat($"{Misc.ColorText(Control.GetPlugin().Localizer["menumanager.selected_type"])} {Misc.GetMenuTypeName(type)}");
        }

        public static string GetMenuTypeName(MenuType type)
        {
            switch(type)
            {
                case MenuType.ChatMenu: return Misc.ColorText(Control.GetPlugin().Localizer["menumanager.chat"]);
                case MenuType.ConsoleMenu: return Misc.ColorText(Control.GetPlugin().Localizer["menumanager.console"]);
                case MenuType.CenterMenu: return Misc.ColorText(Control.GetPlugin().Localizer["menumanager.center"]);
                case MenuType.ButtonMenu: return Misc.ColorText(Control.GetPlugin().Localizer["menumanager.control"]);
                default: return "Undefined";
            }
        }

        public static bool IsValidPlayer(CCSPlayerController player)
        {
            if (player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected && !player.IsBot) return true;
            else return false;
        }

        internal static string ColorText(string text)
        {
            var new_text = text;

            var colors = new List<string>(["Default", "White", "Darkred", "Green", "Lightyellow", "Lightblue", "Olive", "Lime", "Red", "Lightpurple", "Purple", "Grey", "Yellow", "Gold", "Silver", "Blue", "Darkblue", "Bluegrey", "Magenta", "Lightred", "Orange"]);

            foreach (var color in colors)
            {
                var lower = color.ToLower();
                var rep = $"<font color='{lower}'>";
                new_text = new_text.Replace(color, rep);
                new_text = new_text.Replace(color.ToUpper(), rep);
                new_text = new_text.Replace(lower, rep);
            }

            return new_text;
        }
    }

    
}
