using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Menu;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager;

internal static class Control
{
    public static List<PlayerInfo> menus = new List<PlayerInfo>();
    private static MenuManagerCore hPlugin;

    public static void AddMenu(CCSPlayerController player, ButtonMenu inst)
    {
        int old_selected = 0;
        string old_title = "";
        int old_offset = 0;
        for(int i = 0; i < menus.Count; i++)
            if (menus[i].GetPlayer() == player)
            {
                menus[i].RestoreMovement();
                if (menus[i].IsCurrentPlayer())
                {
                    old_selected = menus[i].Selected();
                    old_title = menus[i].menu.Title;
                    old_offset = menus[i].Offset();
                }
                menus.Remove(menus[i]);
                i--;
            }

        var menu = new PlayerInfo(player, inst, null, old_selected, old_offset, old_title);
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
        foreach (var menu in menus) menu.RestoreMovement();
        menus.RemoveAll(x => true);
    }

    public static void OnPluginTick()
    {

        if(menus.Count > 0)
        {
            for(int i = 0; i < menus.Count; i++)
            {
                var menu = menus[i];
                if(menu == null)
                {
                    menus.RemoveAt(i);
                    i--;
                    continue;
                }
                var player = menu.GetPlayer();
                if(!menu.IsCurrentPlayer() || !Misc.IsValidPlayer(player))
                {
                    menu.RestoreMovement();
                    menus.RemoveAt(i);
                    i--;
                    continue;
                }
                if (menu.Closed())
                {
                    menu.RestoreMovement();
                    menus.RemoveAt(i--);
                    continue;
                }
                PlayerButtons buttons = 0;

                buttons = player.Buttons;
                if(!hPlugin.Config.MoveWhileOpenMenu)
                    menu.ApplyMovementLock();
                else
                    menu.RestoreMovement();
                
                if (!menu.IsEqualButtons(buttons))
                {

                    if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.UpButton))
                        menu.MoveUp();
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.DownButton))
                        menu.MoveDown();
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.LeftButton))
                        menu.MoveUp(Control.GetPlugin().Config.MenuLinesCount);
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.RightButton))
                        menu.MoveDown(Control.GetPlugin().Config.MenuLinesCount);
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.SelectButton))
                        menu.OnSelect();
                    else if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.BackButton) && menu.menu.BackAction != null)
                        menu.menu.BackAction(player);

                    // Selection/back callbacks may replace or remove this menu.
                    if (i >= menus.Count || !ReferenceEquals(menus[i], menu))
                    {
                        i--;
                        continue;
                    }
                    if (!menu.IsCurrentPlayer() || !Misc.IsValidPlayer(player))
                    {
                        menu.RestoreMovement();
                        menus.RemoveAt(i--);
                        continue;
                    }
                    if (buttons.HasFlag(hPlugin.Config.ButtonsConfig.ExitButton) || menu.Closed())
                    {                            
                        menu.Close(true);
                        menu.RestoreMovement();
                        menus.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                if (i >= menus.Count || !ReferenceEquals(menus[i], menu))
                {
                    i--;
                    continue;
                }
                                
                player.PrintToCenterHtml(menu.GetText(), 1);
            }
        }
    }

    public static void PlaySound(CCSPlayerController player, string sound)
    {
        if(sound != "")
            player.ExecuteClientCommand("play " + sound);
    }

    public static void CloseMenu(CCSPlayerController player)
    {
        CounterStrikeSharp.API.Modules.Menu.MenuManager.CloseActiveMenu(player);
        for (int i = 0; i < menus.Count; i++)
        {
            if (menus[i].GetPlayer() == player)
            {
                menus[i].Close();
            }
        }
        MenusMM.ClosePlayerMenu(player.Slot);
    }
 
    internal static bool HasOpenedMenu(CCSPlayerController player, PlayerInfo info = null)
    {     
        
        foreach (var menu in menus)
            if (menu.GetPlayer() == player && !menu.Closed() && menu != info)
            {                    
                return true;
            }            
        
        return info == null && MenusMM.IsMenuOpen(player.Slot);        
    }

    internal static void Init(MenuManagerCore _hPlugin)
    {
        hPlugin = _hPlugin;
    }

    internal static MenuManagerCore GetPlugin()
    {
        return hPlugin;
    }
}
