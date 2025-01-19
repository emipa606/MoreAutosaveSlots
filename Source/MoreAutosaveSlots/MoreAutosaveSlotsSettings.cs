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
    private static readonly int countDefault = 5;
    private static readonly int countMin = 1;
    private static readonly int countMax = 60;

    private static readonly string formatDefault = "{faction} ({index})";

    private static readonly int hoursDefault = 0;
    private static readonly int hoursMin = 0;
    private static readonly int hoursMax = 15 * 24;

    private static readonly bool useNextSaveNameDefault = true;
    public int count = countDefault;
    public string format = formatDefault;
    public int hours = hoursDefault;
    public bool useNextSaveName = useNextSaveNameDefault;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref count, "count", countDefault);
        Scribe_Values.Look(ref format, "format", formatDefault);
        Scribe_Values.Look(ref hours, "hours", hoursDefault);
        Scribe_Values.Look(ref useNextSaveName, "useNextSaveName", useNextSaveNameDefault);
        base.ExposeData();
    }

    public static string[] AutoSaveNames(bool onlyOne = false)
    {
        var settings = MoreAutosaveSlotsMod.Settings;
        var count = Math.Max(Math.Min(settings.count, countMax), countMin);
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
            return [DoFormat(almostFormatted, count)];
        }

        var result = new string[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = DoFormat(almostFormatted, i + 1);
        }

        return result;

        string DoFormat(string s, int index)
        {
            s = GenFile.SanitizedFileName(s.Replace("{index}", index.ToString()));
            if (s.Length > 60)
            {
                s = s.Substring(0, 60).Trim();
            }

            return s;
        }
    }

    public static string NextName()
    {
        var texts = AutoSaveNames();

        var text = (from name in texts where !SaveGameFilesUtility.SavedGameNamedExists(name) select name)
            .FirstOrDefault();
        return text ?? texts.MinBy(name => new FileInfo(GenFilePaths.FilePathForSavedGame(name)).LastWriteTime);
    }

    internal bool ShowAndChangeSettings(Listing_Standard listing)
    {
        int countNew, countOld = count;
        string formatNew, formatOld = format;
        int hoursNew, hoursOld = hours;
        bool useNextSaveNameNew, useNextSaveNameOld = useNextSaveName;

        var reset = Widgets.ButtonText(listing.GetRect(Text.LineHeight).RightHalf().LeftHalf().Rounded(),
            "MoreAutosaveSlots.ButtonReset".Translate());
        listing.Gap(listing.verticalSpacing + Text.LineHeight);

        Line("MoreAutosaveSlots.LabelExample".Translate(), AutoSaveNames(true)[0]);
        listing.Gap(Text.LineHeight);

        {
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
            countNew = Mathf.RoundToInt(Widgets.HorizontalSlider(dataSliderRect, countOld, countMin, countMax));

            listing.Gap(listing.verticalSpacing + Text.LineHeight);
        }
        {
            var wholeRect = listing.GetRect(Text.LineHeight);
            var labelRect = wholeRect.LeftHalf().Rounded();
            var dataRect = wholeRect.RightHalf().Rounded();

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(labelRect, "MoreAutosaveSlots.LabelFormat".Translate());

            Text.Anchor = TextAnchor.MiddleCenter;
            formatNew = Widgets.TextArea(dataRect.Rounded(), formatOld);
            listing.Gap(listing.verticalSpacing);

            foreach (Match m in Regex.Matches("MoreAutosaveSlots.FormatDescription".Translate(),
                         @"\|([^|]*)\|([^|]*)\|\|"))
            {
                var groups = m.Groups;
                Line(groups[1].Value, groups[2].Value);
            }

            listing.Gap(Text.LineHeight);
        }
        {
            var wholeRect = listing.GetRect(2 * Text.LineHeight);
            var labelRect = wholeRect.LeftHalf().Rounded();
            var dataRect = wholeRect.RightHalf().Rounded();
            var dataDescRect = dataRect.TopHalf().Rounded();
            var dataSliderRect = dataRect.BottomHalf().Rounded();

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
            hoursNew = (int)Widgets.HorizontalSlider(dataSliderRect, hoursOld, hoursMin, hoursMax, roundTo: 1f);

            listing.Gap(listing.verticalSpacing + Text.LineHeight);
        }
        {
            var wholeRect = listing.GetRect(Text.LineHeight);
            var labelRect = wholeRect.LeftHalf().Rounded();
            var dataRect = wholeRect.RightHalf().Rounded();

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(labelRect, "MoreAutosaveSlots.LabelUseNextSaveName".Translate());

            Text.Anchor = TextAnchor.MiddleLeft;
            useNextSaveNameNew = useNextSaveNameOld;
            Widgets.CheckboxLabeled(dataRect, "", ref useNextSaveNameNew, placeCheckboxNearText: true);

            listing.Gap(listing.verticalSpacing);
        }

        if (reset)
        {
            countNew = countDefault;
            formatNew = formatDefault;
            hoursNew = hoursDefault;
            useNextSaveNameNew = useNextSaveNameDefault;
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
            hours = hoursNew;
            changed = true;
        }

        if (useNextSaveNameNew == useNextSaveNameOld)
        {
            return changed;
        }

        useNextSaveName = useNextSaveNameNew;

        return true;

        void Line(string left, string right)
        {
            var wholeRect = listing.GetRect(Text.LineHeight);

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(wholeRect.LeftHalf().Rounded(), left);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(wholeRect.RightHalf().Rounded(), right);

            listing.Gap(listing.verticalSpacing);
        }
    }
}