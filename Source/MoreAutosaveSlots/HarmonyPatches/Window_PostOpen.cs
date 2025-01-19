using HarmonyLib;
using RimWorld;
using Verse;

namespace Revolus.MoreAutosaveSlots.HarmonyPatches;

[HarmonyPatch(typeof(Window), nameof(Window.PostOpen))]
public static class Window_PostOpen
{
    public static void Postfix(ref Window __instance)
    {
        if (!MoreAutosaveSlotsMod.Settings.useNextSaveName)
        {
            return;
        }

        if (__instance is not Dialog_SaveFileList_Save saveFileListDialog)
        {
            return;
        }

        AccessTools.Field(typeof(Dialog_SaveFileList_Save), "typingName")
            .SetValue(saveFileListDialog, MoreAutosaveSlotsSettings.NextName());
    }
}