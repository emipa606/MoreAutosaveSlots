using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Revolus.MoreAutosaveSlots;

public class MoreAutosaveSlotsSettings : ModSettings
{
    private const int CountDefault = 5;
    private const int CountMin = 1;
    private const int CountMax = 60;

    private const string FormatDefault = "{faction} ({index})";

    private const int HoursDefault = 0;
    private const int HoursMin = 0;
    private const int HoursMax = 15 * 24;

    private const bool UseNextSaveNameDefault = true;
    public int Hours = HoursDefault;
    public bool UseNextSaveName = UseNextSaveNameDefault;
    private int count = CountDefault;
    private string format = FormatDefault;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref count, "count", CountDefault);
        Scribe_Values.Look(ref format, "format", FormatDefault);
        Scribe_Values.Look(ref Hours, "hours");
        Scribe_Values.Look(ref UseNextSaveName, "useNextSaveName", UseNextSaveNameDefault);
        base.ExposeData();
    }

    private static string[] autoSaveNames(bool onlyOne = false)
    {
        var settings = MoreAutosaveSlotsMod.Settings;
        var count = Math.Max(Math.Min(settings.count, CountMax), CountMin);
        var now = DateTime.Now;

        string faction = null;
        try
        {
            faction = Faction.OfPlayerSilentFail?.Name;
        }
        catch
        {
            // ignored
        }

        if (string.IsNullOrEmpty(faction))
        {
            faction = "MoreAutosaveSlots.FactionNameDefault".Translate();
        }

        string settlement = null;
        try
        {
            settlement = ((Settlement)Find.CurrentMap.info.parent).Name;
        }
        catch
        {
            // ignored
        }

        if (string.IsNullOrEmpty(settlement))
        {
            settlement = "MoreAutosaveSlots.SettlementNameDefault".Translate();
        }

        string seed = null;
        try
        {
            seed = Find.World.info.seedString;
        }
        catch
        {
            // ignored
        }

        if (string.IsNullOrEmpty(seed))
        {
            seed = "MoreAutosaveSlots.SeedDefault".Translate();
        }

        var almostFormatted = settings.format.Replace("{faction}", faction).Replace("{settlement}", settlement)
            .Replace("{seed}", seed)
            .Replace("{year}", now.Year.ToString("D4")).Replace("{month}", now.Month.ToString("D2"))
            .Replace("{day}", now.Day.ToString("D2")).Replace("{hour}", now.Hour.ToString("D2"))
            .Replace("{minute}", now.Minute.ToString("D2")).Replace("{second}", now.Second.ToString("D2")).Trim();

        if (onlyOne)
        {
            return [doFormat(almostFormatted, count)];
        }

        var result = new string[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = doFormat(almostFormatted, i + 1);
        }

        return result;

        static string doFormat(string s, int index)
        {
            s = GenFile.SanitizedFileName(s.Replace("{index}", index.ToString()));
            if (s.Length > 60)
            {
                s = s[..60].Trim();
            }

            return s;
        }
    }

    public static string NextName()
    {
        var texts = autoSaveNames();

        var text = (from name in texts where !SaveGameFilesUtility.SavedGameNamedExists(name) select name)
            .FirstOrDefault();
        return text ?? texts.MinBy(name => new FileInfo(GenFilePaths.FilePathForSavedGame(name)).LastWriteTime);
    }

    internal bool ShowAndChangeSettings(Listing_Standard listing)
    {
        var countOld = count;
        var formatOld = format;
        var hoursOld = Hours;
        var useNextSaveNameOld = UseNextSaveName;

        var reset = Widgets.ButtonText(listing.GetRect(Text.LineHeight).RightHalf().LeftHalf().Rounded(),
            "MoreAutosaveSlots.ButtonReset".Translate());
        listing.Gap(listing.verticalSpacing + Text.LineHeight);

        line("MoreAutosaveSlots.LabelExample".Translate(), autoSaveNames(true)[0]);
        listing.Gap(Text.LineHeight);

        var wholeRect = listing.GetRect(2 * Text.LineHeight);
        var labelRect = wholeRect.LeftHalf().Rounded();
        var dataRect = wholeRect.RightHalf().Rounded();
        var dataDescRect = dataRect.TopHalf().Rounded();
        var dataSliderRect = dataRect.BottomHalf().Rounded();

        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(labelRect, "MoreAutosaveSlots.LabelCount".Translate());

        Text.Anchor = TextAnchor.LowerCenter;
        Widgets.Label(dataDescRect,
            countOld == 1
                ? "MoreAutosaveSlots.CountOne".Translate()
                : "MoreAutosaveSlots.CountMultiple".Translate(countOld));
        Text.Anchor = TextAnchor.UpperCenter;
        var countNew = Mathf.RoundToInt(Widgets.HorizontalSlider(dataSliderRect, countOld, CountMin, CountMax));

        listing.Gap(listing.verticalSpacing + Text.LineHeight);
        wholeRect = listing.GetRect(Text.LineHeight);
        labelRect = wholeRect.LeftHalf().Rounded();
        dataRect = wholeRect.RightHalf().Rounded();

        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(labelRect, "MoreAutosaveSlots.LabelFormat".Translate());

        Text.Anchor = TextAnchor.MiddleCenter;
        var formatNew = Widgets.TextArea(dataRect.Rounded(), formatOld);
        listing.Gap(listing.verticalSpacing);

        foreach (var groups in Regex.Matches("MoreAutosaveSlots.FormatDescription".Translate(),
                     @"\|([^|]*)\|([^|]*)\|\|", RegexOptions.None, TimeSpan.FromSeconds(1)).Select(x => x.Groups))
        {
            line(groups[1].Value, groups[2].Value);
        }

        listing.Gap(Text.LineHeight);
        wholeRect = listing.GetRect(2 * Text.LineHeight);
        labelRect = wholeRect.LeftHalf().Rounded();
        dataRect = wholeRect.RightHalf().Rounded();
        dataDescRect = dataRect.TopHalf().Rounded();
        dataSliderRect = dataRect.BottomHalf().Rounded();

        string intervalDescription;
        if (hoursOld <= 0)
        {
            intervalDescription = "MoreAutosaveSlots.HoursDisabled".Translate();
        }
        else
        {
            intervalDescription = (hoursOld * GenDate.TicksPerHour).ToStringTicksToPeriod();
        }

        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(labelRect, "MoreAutosaveSlots.LabelHours".Translate());

        Text.Anchor = TextAnchor.LowerCenter;
        Widgets.Label(dataDescRect, intervalDescription);
        Text.Anchor = TextAnchor.UpperCenter;
        var hoursNew = (int)Widgets.HorizontalSlider(dataSliderRect, hoursOld, HoursMin, HoursMax, roundTo: 1f);

        listing.Gap(listing.verticalSpacing + Text.LineHeight);
        wholeRect = listing.GetRect(Text.LineHeight);
        labelRect = wholeRect.LeftHalf().Rounded();
        dataRect = wholeRect.RightHalf().Rounded();

        Text.Anchor = TextAnchor.MiddleRight;
        Widgets.Label(labelRect, "MoreAutosaveSlots.LabelUseNextSaveName".Translate());

        Text.Anchor = TextAnchor.MiddleLeft;
        var useNextSaveNameNew = useNextSaveNameOld;
        Widgets.CheckboxLabeled(dataRect, "", ref useNextSaveNameNew, placeCheckboxNearText: true);

        listing.Gap(listing.verticalSpacing);

        if (reset)
        {
            countNew = CountDefault;
            formatNew = FormatDefault;
            hoursNew = HoursDefault;
            useNextSaveNameNew = UseNextSaveNameDefault;
        }

        var changed = false;
        if (countNew != countOld)
        {
            count = countNew;
            changed = true;
        }

        if (formatNew != formatOld)
        {
            format = formatNew;
            changed = true;
        }

        if (hoursNew != hoursOld)
        {
            Hours = hoursNew;
            changed = true;
        }

        if (useNextSaveNameNew == useNextSaveNameOld)
        {
            return changed;
        }

        UseNextSaveName = useNextSaveNameNew;

        return true;

        void line(string left, string right)
        {
            var rect = listing.GetRect(Text.LineHeight);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect.LeftHalf().Rounded(), left);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect.RightHalf().Rounded(), right);

            listing.Gap(listing.verticalSpacing);
        }
    }
}