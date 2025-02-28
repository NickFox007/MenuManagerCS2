using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MenuManager.MenusMM;

namespace MenuManager;


// typedef std::function<void(const char* szBack, const char* szFront, int iItem, int iSlot)> MenuCallbackFunc;
//private static unsafe delegate* unmanaged[Cdecl]<string, string, int, int, void> MM_MenuCallbackFunc;



internal static class MenusMM
{
    internal struct Callback_Info
    {
        int slot;
        public MM_MenuCallbackFunc func;

        internal Callback_Info(int slot, MM_MenuCallbackFunc func)
        {
            this.slot = slot;
            this.func = func;
        }

        internal int Slot() { return slot; }
    }


    private static List<Callback_Info> callbacks_infos = new List<Callback_Info>();

    private static unsafe delegate* unmanaged[Cdecl]<int, bool> Native_IsMenuOpen;
    private static unsafe delegate* unmanaged[Cdecl]<int, void> Native_ClosePlayerMenu;
    private static unsafe delegate* unmanaged[Cdecl]<string, string, int, void> Native_AddItemMenu; // MenusApi_SetExitMenu(Menu& hMenu, bool bExit)
    private static unsafe delegate* unmanaged[Cdecl]<bool, void> Native_SetExitMenu; // MenusApi_SetExitMenu(Menu& hMenu, bool bExit)
    private static unsafe delegate* unmanaged[Cdecl]<bool, void> Native_SetBackMenu; // MenusApi_SetBackMenu(Menu& hMenu, bool bBack)
    private static unsafe delegate* unmanaged[Cdecl]<string, void> Native_SetTitleMenu; // MenusApi_SetTitleMenu(Menu& hMenu, const char* szTitle)
    private static unsafe delegate* unmanaged[Cdecl]<MM_MenuCallbackFunc, void> Native_SetCallback; // MenusApi_SetCallback(Menu& hMenu, MenuCallbackFunc func)
    private static unsafe delegate* unmanaged[Cdecl]<int, bool, bool, void> Native_DisplayPlayerMenu; // MenusApi_DisplayPlayerMenu(Menu& hMenu, int iSlot, bool bClose = true, bool bReset = true)
    private static unsafe delegate* unmanaged[Cdecl]<int, void> Native_NewMenuInstance; // MenusApi_DisplayPlayerMenu(Menu& hMenu, int iSlot, bool bClose = true, bool bReset = true)
    private static unsafe delegate* unmanaged[Cdecl]<int, void> Native_Clear; // MenusApi_DisplayPlayerMenu(Menu& hMenu, int iSlot, bool bClose = true, bool bReset = true)

    private static bool hooked = false;

    public delegate void MM_MenuCallbackFunc(string szBack, string szFront, int iItem, int iSlot);
    
    private static string GetOSExt()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "so" : "dll";
    }

    internal static void Init()
    {
        if (hooked) return;

        if (callbacks_infos == null)
            callbacks_infos = new List<Callback_Info>();
        else
            callbacks_infos.Clear();

        string libPath = string.Empty;
        
        libPath = $"{Server.GameDirectory}/csgo/addons/MenusExport/bin/MenusExport.{GetOSExt()}";

        if (!File.Exists(libPath))
            return;

        var libHandle = NativeLibrary.Load(libPath);
        if (libHandle != IntPtr.Zero)
        {
            IntPtr funcPtr = IntPtr.Zero;

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_IsMenuOpen");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_IsMenuOpen = (delegate* unmanaged[Cdecl]<int, bool>)funcPtr;
                }
            }
            else
            {
                NotHooked(2);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_ClosePlayerMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_ClosePlayerMenu = (delegate* unmanaged[Cdecl]<int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(3);
                return;
            }



            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_AddItemMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_AddItemMenu = (delegate* unmanaged[Cdecl]<string, string, int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(4);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetExitMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_SetExitMenu = (delegate* unmanaged[Cdecl]<bool, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(4);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetBackMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_SetBackMenu = (delegate* unmanaged[Cdecl]<bool, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(5);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetTitleMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_SetTitleMenu = (delegate* unmanaged[Cdecl]<string, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(6);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_SetCallback");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_SetCallback = (delegate* unmanaged[Cdecl]<MM_MenuCallbackFunc, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(7);
                return;
            }

            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_DisplayPlayerMenu");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_DisplayPlayerMenu = (delegate* unmanaged[Cdecl]<int, bool, bool, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(8);
                return;
            }
            
            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_NewMenuInstance");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_NewMenuInstance = (delegate* unmanaged[Cdecl]<int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(9);
                return;
            }
            
            funcPtr = NativeLibrary.GetExport(libHandle, "MenusApi_Clear");
            if (funcPtr != IntPtr.Zero)
            {
                unsafe
                {
                    Native_Clear = (delegate* unmanaged[Cdecl]<int, void>)funcPtr;
                }
            }
            else
            {
                NotHooked(10);
                return;
            }
        }
        else
        {
            NotHooked(1);
            return;
        }

        Control.GetPlugin().Logger.LogInformation("====================================");
        Control.GetPlugin().Logger.LogInformation(" ");
        Control.GetPlugin().Logger.LogInformation("Metamod MenusApi found and hooked!");
        Control.GetPlugin().Logger.LogInformation(" ");
        Control.GetPlugin().Logger.LogInformation("====================================");

        NativeLibrary.Free(libHandle);
        hooked = true;
        
    }

    internal static bool IsMenuOpen(int iSlot)
    {
        if (hooked)
            unsafe
            {
                return Native_IsMenuOpen(iSlot);
            }
        else
        {
            return false;
        }
    }

    internal static void ClosePlayerMenu(int iSlot)
    {
        if(hooked)
            unsafe
            {
                if(Native_IsMenuOpen(iSlot)) Native_ClosePlayerMenu(iSlot);
            }
    }

    internal static void SetExitMenu(bool exit)
    {
        if (hooked)
            unsafe
            {
                Native_SetExitMenu(exit);
            }
    }

    internal static void SetBackMenu(bool back)
    {
        if (hooked)
            unsafe
            {
                Native_SetBackMenu(back);
            }
    }

    internal static void SetTitleMenu(string title)
    {
        if (hooked)
            unsafe
            {
                Native_SetTitleMenu(title);
            }
    }

    internal static void SetCallback(MM_MenuCallbackFunc func)
    {
        if (hooked)                    
            unsafe
            {
                Native_SetCallback(func);
            }
    }

    internal static void DisplayPlayerMenu(int slot, bool close = true, bool reset = true)
    {
        if (hooked)
            unsafe
            {
                Native_DisplayPlayerMenu(slot, close, reset);
            }
    }

    internal static void AddItemMenu(string back, string text, bool disabled = false)
    {
        if (hooked)
        {
            var itemtype = 1;
            if (disabled) itemtype = 2;
            unsafe
            {
                Native_AddItemMenu(back, text, itemtype);
            }
        }
    }

    internal static void NewMenuInstance(int slot)
    {
        if (hooked)
        {
            unsafe
            {
                Native_NewMenuInstance(slot);
            }
        }
    }

    internal static void Clear(int slot)
    {
        if (hooked)
        {
            unsafe
            {
                Native_Clear(slot);
            }
        }
    }

    internal static void NotHooked(int i)
    {
        hooked = false;
        Control.GetPlugin().Logger.LogInformation($"Metamod MenusApi found but couldnt hook it! [Code: {i}]");
        Control.GetPlugin().Config.UseMetamodMenu = false;
        Control.GetPlugin().Config.UseMetamodMenuReplace = false;
    }

    private static void AddCallbackInfo(int slot, MM_MenuCallbackFunc func)
    {
        callbacks_infos.Add(new Callback_Info(slot, func));
    }

    internal static void ClearCallbackInfo(int slot)
    {
        if (!hooked)
            return;
        for(int i = callbacks_infos.Count - 1; i >= 0; i--)
            if (callbacks_infos[i].Slot() == slot)
                callbacks_infos.RemoveAt(i);
        Clear(slot);

    }

    internal static bool Hooked()
    {
        return hooked;
    }

    internal static void PassMenuToMM(CCSPlayerController player, MenuInstance menu)
    {
        if (!hooked)
            return;
        var slot = player.Slot;
        NewMenuInstance(slot);
        SetTitleMenu(Misc.ColorText(menu.Title, false));
        if (menu.BackAction != null)
            SetBackMenu(true);
        
        SetExitMenu(menu.ExitButton);
        
        for (int i = 0; i < menu.MenuOptions.Count; i++)
            AddItemMenu(i.ToString(), Misc.ColorText(menu.MenuOptions[i].Text, false), menu.MenuOptions[i].Disabled);

        //Func<string, string, int, int, void> callback = delegate (string szBack, string szFront, int iItem, int iSlot)
        MM_MenuCallbackFunc callback = (string szBack, string szFront, int iItem, int iSlot) =>
        {
            var player = Utilities.GetPlayerFromSlot(iSlot);
            if (iItem < 7)
            {
                var index = int.Parse(szBack);
                if(menu.PostSelectAction != PostSelectAction.Nothing)
                    ClosePlayerMenu(iSlot);
                menu.MenuOptions[index].OnSelect(player, menu.MenuOptions[index]);

                
                switch (menu.PostSelectAction)
                {
                    case PostSelectAction.Close: ClosePlayerMenu(iSlot); Control.CloseMenu(Utilities.GetPlayerFromSlot(iSlot)); break;                    
                    case PostSelectAction.Reset: if (menu.ResetAction != null && !Control.HasOpenedMenu(player)) Server.NextFrameAsync(() => menu.ResetAction(player)); break;
                }

            }
            else if (iItem == 7 && menu.BackAction != null)
                menu.BackAction(player);
        };
        
        AddCallbackInfo(slot, callback);
        SetCallback(callbacks_infos.Last().func);
        DisplayPlayerMenu(slot);        
    }
}
