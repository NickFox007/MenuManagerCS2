using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace MenuManager;

public static class EmitSoundExtension
{
    // TODO: these are for libserver.so, haven't found these on windows yet
    private static MemoryFunctionVoid<CBaseEntity, string, int, float, float> CBaseEntity_EmitSoundParamsFunc = new("\\x48\\xB8\\x2A\\x2A\\x2A\\x2A\\x2A\\x2A\\x2A\\x2A\\x55\\x48\\x89\\xE5\\x41\\x55\\x41\\x54\\x49\\x89\\xFC\\x53\\x48\\x89\\xF3");
    private static MemoryFunctionWithReturn<nint, nint, uint, uint, short, ulong, ulong> CSoundOpGameSystem_StartSoundEventFunc = new("\\x48\\xb8\\x00\\x00\\x00\\x00\\x08\\x00\\x00\\xc0\\x55\\x48\\x89\\xe5\\x41\\x57\\x45\\x89\\xc7\\x41\\x56\\x41\\x55\\x4c\\x8d\\x6d\\xc0\\x41\\x54\\x41\\x89\\xcc\\x53\\x48\\x89\\xfb\\x48\\x8d\\x3d");
    private static MemoryFunctionVoid<nint, nint, ulong, nint, nint, short, byte> CSoundOpGameSystem_SetSoundEventParamFunc = new("\\x55\\x48\\x89\\xe5\\x41\\x57\\x41\\x56\\x49\\x89\\xd6\\x48\\x89\\xca\\x41\\x55\\x49\\x89\\xf5\\x41\\x54\\x49\\x89\\xfc\\x53\\x48\\x89\\xcb\\x48\\x83\\xec\\x18\\x48\\x8d\\x05");

    internal static void Init()
    {
        CSoundOpGameSystem_StartSoundEventFunc.Hook(CSoundOpGameSystem_StartSoundEventFunc_PostHook, HookMode.Post);
    }

    internal static void CleanUp()
    {
        CSoundOpGameSystem_StartSoundEventFunc.Unhook(CSoundOpGameSystem_StartSoundEventFunc_PostHook, HookMode.Post);
    }

    [ThreadStatic]
    private static IReadOnlyDictionary<string, float>? CurrentParameters;

    /// <summary>
    /// Emit a sound event by name (e.g., "Weapon_AK47.Single").
    /// TODO: parameters passed in here only seem to work for sound events shipped with the game, not workshop ones.
    /// </summary>
    public static void EmitSound(this CBaseEntity entity, string soundName, IReadOnlyDictionary<string, float>? parameters = null)
    {
        if (!entity.IsValid)
        {
            throw new ArgumentException("Entity is not valid.");
        }

        try
        {
            // We call CBaseEntity::EmitSoundParams,
            // which calls a method that returns an ID that we can use
            // to modify the playing sound.

            CurrentParameters = parameters;

            // Pitch, volume etc aren't actually used here
            CBaseEntity_EmitSoundParamsFunc.Invoke(entity, soundName, 100, 1f, 0f);
        }
        finally
        {
            CurrentParameters = null;
        }
    }

    private static HookResult CSoundOpGameSystem_StartSoundEventFunc_PostHook(DynamicHook hook)
    {
        if (CurrentParameters is not { Count: > 0 })
        {
            return HookResult.Continue;
        }

        var pSoundOpGameSystem = hook.GetParam<nint>(0);
        var pFilter = hook.GetParam<nint>(1);
        var soundEventId = hook.GetReturn<ulong>();

        foreach (var parameter in CurrentParameters)
        {
            CSoundOpGameSystem_SetSoundEventParam(pSoundOpGameSystem, pFilter,
                soundEventId, parameter.Key, parameter.Value);
        }

        return HookResult.Continue;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct FloatParamData
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly uint _type1;
        private readonly uint _type2;

        private readonly uint _size1;
        private readonly uint _size2;

        private readonly float _value;
        private readonly uint _padding;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public FloatParamData(float value)
        {
            _type1 = 1;
            _type2 = 8;

            _size1 = 4;
            _size2 = 4;

            _value = value;
            _padding = 0;
        }
    }

    private static unsafe void CSoundOpGameSystem_SetSoundEventParam(nint pSoundOpGameSystem, nint pFilter,
        ulong soundEventId, string paramName, float value)
    {
        var data = new FloatParamData(value);
        var nameByteCount = Encoding.UTF8.GetByteCount(paramName);

        var pData = Unsafe.AsPointer(ref data);
        var pName = stackalloc byte[nameByteCount + 1];

        Encoding.UTF8.GetBytes(paramName, new Span<byte>(pName, nameByteCount));

        CSoundOpGameSystem_SetSoundEventParamFunc.Invoke(pSoundOpGameSystem, pFilter, soundEventId, (nint)pName, (nint)pData, 0, 0);
    }
}