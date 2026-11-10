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
        private PlayerButtons prev_buttons;
        private bool closed;
        private nint pawnHandle;
        private uint pawnIdentity;
        private readonly uint playerIdentity;
        private bool movementLocked;
        private string? cachedText;
        private string? cachedTitle, cachedSelected, cachedEmpty, cachedFooter;
        private int cachedOffset, cachedSelection, cachedLines, cachedCount;
        private ChatMenuOption[] cachedOptions = Array.Empty<ChatMenuOption>();
        private string?[] cachedLabels = Array.Empty<string?>();
        private bool[] cachedDisabled = Array.Empty<bool>();

        public PlayerInfo(CCSPlayerController _player, ButtonMenu _menu, float? new_mod = null, int new_selected = 0, int new_offset = 0, string old_title = "")
        {
            player = _player;
            menu = _menu;
            playerIdentity = player.EntityHandle.Raw;
            
            var pawn = player.PlayerPawn.Value;
            pawnHandle = pawn is { IsValid: true } ? pawn.Handle : 0;
            pawnIdentity = player.PlayerPawn.Raw;
            prev_mod = new_mod ?? (pawnHandle != 0 ? pawn!.VelocityModifier : 1.0f);

            closed = false;
            offset = 0;
            if (_menu.Title == old_title)
            {
                if (_menu.MenuOptions.Count > new_selected)
                    selected = new_selected;
                else
                    selected = Math.Max(_menu.MenuOptions.Count - 1, 0);
                offset = new_offset;
            }
            else
                selected = 0;
            prev_buttons = player.Buttons;
        }

        public int Offset()
        {
            return offset;
        }

        public CCSPlayerController GetPlayer()
        {
            return player;
        }           

        public float GetMod()
        {
            return prev_mod;
        }

        public nint PawnHandle => pawnHandle;

        public bool IsCurrentPlayer() => player.IsValid && player.EntityHandle.Raw == playerIdentity;

        public string GetText()
        {
            var config = Control.GetPlugin().Config;
            var selectedText = Control.GetPlugin().Localizer["menumanager.selected_item"].Value;
            var emptyText = Control.GetPlugin().Localizer["menumanager.empty"].Value;
            var footerText = Control.GetPlugin().Localizer["menumanager.footer"].Value;
            var count = menu.MenuOptions.Count;
            if (cachedCount != count || cachedLines != config.MenuLinesCount)
            {
                // Direct option mutations or a smaller page must not leave the
                // selection outside the list or the visible page.
                selected = Math.Clamp(selected, 0, Math.Max(0, count - 1));
                offset = Math.Clamp(offset, Math.Max(0, selected - config.MenuLinesCount + 1), selected);
            }
            var end = Math.Min(offset + config.MenuLinesCount, count);
            var visible = Math.Max(0, end - offset);
            var dirty = cachedText == null || cachedTitle != menu.Title || cachedSelected != selectedText ||
                cachedEmpty != emptyText || cachedFooter != footerText || cachedOffset != offset ||
                cachedSelection != selected || cachedLines != config.MenuLinesCount || cachedCount != count ||
                cachedOptions.Length != visible;
            if (!dirty)
                for (int j = 0; j < visible; j++)
                {
                    var option = menu.MenuOptions[offset + j];
                    if (!ReferenceEquals(cachedOptions[j], option) || cachedLabels[j] != option.Text || cachedDisabled[j] != option.Disabled)
                    {
                        dirty = true;
                        break;
                    }
                }
            if (!dirty) return cachedText!;

            string text = $"<font class='mono-spaced-font'>{menu.Title}</font><font class='fontSize-sm stratum-font'>";
            
            if(menu.MenuOptions.Count > 0)            
                for (int i = offset; i < end; i++)
                {
                    var line = menu.MenuOptions[i].Text;
                    if (menu.MenuOptions[i].Disabled) line = $"<font color='#aaaaaa'>{Misc.ColorText(line, false)}</font>";
                    if (i == selected) line = selectedText.Replace("[MENUITEM]", line);


                    text = text + "<br>" + line;
                }
            else
                text = $"{text}<br><font color='#aaaaaa'>{emptyText}</font>";

            text = Misc.ColorText(text + $"</font><br><font class='fontSize-s'>{footerText}</font>");

            if (cachedOptions.Length != visible)
            {
                cachedOptions = new ChatMenuOption[visible];
                cachedLabels = new string?[visible];
                cachedDisabled = new bool[visible];
            }
            for (int j = 0; j < visible; j++)
            {
                var option = menu.MenuOptions[offset + j];
                cachedOptions[j] = option;
                cachedLabels[j] = option.Text;
                cachedDisabled[j] = option.Disabled;
            }
            cachedTitle = menu.Title; cachedSelected = selectedText; cachedEmpty = emptyText; cachedFooter = footerText;
            cachedOffset = offset; cachedSelection = selected; cachedLines = config.MenuLinesCount; cachedCount = count;
            cachedText = text;

            return text;
        }

        public bool MoveDown(int lines = 1)
        {
            Control.PlaySound(player, Control.GetPlugin().Config.SoundScroll);

            if (selected == menu.MenuOptions.Count - 1) return false;

            selected = Math.Min(selected + lines, menu.MenuOptions.Count-1);
            
            if (selected - offset > Control.GetPlugin().Config.MenuLinesCount - 1) offset = selected - Control.GetPlugin().Config.MenuLinesCount + 1;

            

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

        public bool IsEqualButtons(PlayerButtons buttons)
        {
            var flag = prev_buttons == buttons;
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

                    if (Control.GetPlugin().Config.IgnoreErrors)
                    {

                        try
                        {
                            menu.MenuOptions[selected].OnSelect(player, menu.MenuOptions[selected]);
                        }
                        catch (Exception e)
                        {
                            Control.GetPlugin().Logger.LogInformation($"Error was caused in calling plugin: {e.Message}\n=============== STACKTRACE ===============\n{e.StackTrace}\n==========================================");
                        }
                    }
                    else
                        menu.MenuOptions[selected].OnSelect(player, menu.MenuOptions[selected]);

                    switch (menu.PostSelectAction)
                    {
                        case PostSelectAction.Close: Close(); break;
                        //case PostSelectAction.Reset: Close(false); Control.GetPlugin().AddTimer(0.2f, () => { if (menu.ResetAction != null && !Control.HasOpenedMenu(player, this)) menu.ResetAction(player); }); break;
                        case PostSelectAction.Reset: if (menu.ResetAction != null && !Control.HasOpenedMenu(player, this)) menu.ResetAction(player); break;
                    }
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

        public void ApplyMovementLock()
        {
            if (!IsCurrentPlayer()) return;
            var pawn = player.PlayerPawn.Value;
            if (pawn is not { IsValid: true }) return;
            // A respawn may reuse the native address with a different serial.
            // Capture the new pawn's own modifier before taking its lock.
            if (!movementLocked || pawn.Handle != pawnHandle || player.PlayerPawn.Raw != pawnIdentity)
            {
                prev_mod = pawn.VelocityModifier;
                pawnHandle = pawn.Handle;
                pawnIdentity = player.PlayerPawn.Raw;
                movementLocked = true;
            }
            pawn.VelocityModifier = 0.0f;
        }

        public void RestoreMovement()
        {
            if (!movementLocked) return;
            var pawn = IsCurrentPlayer() ? player.PlayerPawn.Value : null;
            if (pawn is { IsValid: true } && pawn.Handle == pawnHandle && player.PlayerPawn.Raw == pawnIdentity)
                pawn.VelocityModifier = prev_mod;
            movementLocked = false;
        }

    }
}
