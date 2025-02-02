using CounterStrikeSharp.API;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MenuManager;

internal static class MenusMM
{
    private static unsafe delegate* unmanaged[Cdecl]<int, bool> Native_IsMenuOpen;
    private static unsafe delegate* unmanaged[Cdecl]<int, void> Native_ClosePlayerMenu;

    static bool hooked = false;
    internal static void Init()
    {
        if (hooked) return;

        string libPath = string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            libPath = $"{Server.GameDirectory}/csgo/addons/MenusExport/bin/MenusExport.so";
        //else
            //return;

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
            return false;
    }

    internal static void ClosePlayerMenu(int iSlot)
    {
        if(hooked)
            unsafe
            {
                if(Native_IsMenuOpen(iSlot)) Native_ClosePlayerMenu(iSlot);
            }
    }

    internal static void NotHooked(int i)
    {
        hooked = false;
        Control.GetPlugin().Logger.LogInformation($"Metamod MenusApi found but couldnt hook it! [{i}]");
    }
}
