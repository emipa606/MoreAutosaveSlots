using HarmonyLib;
using RimWorld;
using Verse;

namespace Revolus.MoreAutosaveSlots.HarmonyPatches;

[HarmonyPatch(typeof(Autosaver), nameof(Autosaver.AutosaverTick))]
public static class Autosaver_AutosaverTick
{
    public static bool Prefix(ref Autosaver __instance, ref int ___ticksSinceSave)
    {
        var hours = MoreAutosaveSlotsMod.Settings.hours;
        if (hours <= 0)
        {
            return true; // use default implementation
        }

        var instance = __instance;

        var doAutosaveThen = hours * GenDate.TicksPerHour;

        if (++___ticksSinceSave < doAutosaveThen)
        {
            return false; // don't call default implementation
        }

        LongEventHandler.QueueLongEvent(instance.DoAutosave, "Autosaving", false, null);
        ___ticksSinceSave = 0;

        return false; // don't call default implementation
    }
}