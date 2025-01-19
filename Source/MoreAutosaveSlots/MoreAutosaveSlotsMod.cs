using System.Reflection;
using HarmonyLib;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Revolus.MoreAutosaveSlots;

public class MoreAutosaveSlotsMod : Mod
{
    public static MoreAutosaveSlotsSettings Settings;
    private static string currentVersion;

    public MoreAutosaveSlotsMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<MoreAutosaveSlotsSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        new Harmony(typeof(MoreAutosaveSlotsMod).AssemblyQualifiedName).PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void DoSettingsWindowContents(Rect rect)
    {
        bool changed;

        var listing = new Listing_Standard();
        listing.Begin(rect);
        try
        {
            var oldAnchorValue = Text.Anchor;
            try
            {
                changed = Settings.ShowAndChangeSettings(listing);
            }
            finally
            {
                Text.Anchor = oldAnchorValue;
            }
        }
        finally
        {
            if (currentVersion != null)
            {
                listing.Gap();
                GUI.contentColor = Color.gray;
                listing.Label("MoreAutosaveSlots.CurrentVersion".Translate(currentVersion));
                GUI.contentColor = Color.white;
            }

            listing.End();
        }

        if (changed)
        {
            SoundDefOf.DragSlider.PlayOneShotOnCamera();
        }

        base.DoSettingsWindowContents(rect);
    }

    public override string SettingsCategory()
    {
        return "MoreAutosaveSlots.SettingsName".Translate();
    }
}