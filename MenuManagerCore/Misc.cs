using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerSettings;


namespace MenuManager
{
    internal static class Misc
    {

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

        public static MenuType GetCurrentPlayerMenu(CCSPlayerController player)
        {         
            var res = settings.GetPlayerSettingsValue(player, "menutype", "ButtonMenu");
            return (MenuType)Enum.Parse(typeof(MenuType), res);
        }

        public static void SelectPlayerMenu(CCSPlayerController player, MenuType type)
        {
            settings.SetPlayerSettingsValue(player, "menutype", Enum.GetName(type.GetType(), type));
        }
    }

    
}
