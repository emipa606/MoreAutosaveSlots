using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using Verse;

namespace Revolus.MoreAutosaveSlots.HarmonyPatches;

[HarmonyPatch(typeof(GenText), nameof(GenText.IsValidFilename))]
public static class GenText_IsValidFilename
{
    public static bool Prefix(ref bool __result, string str)
    {
        __result = str.Length <= 60 &&
                   !new Regex($"[{Regex.Escape(GenText.GetInvalidFilenameCharacters())}]", RegexOptions.None,
                       TimeSpan.FromSeconds(1)).IsMatch(str);
        return false; // don't call default implementation
    }
}