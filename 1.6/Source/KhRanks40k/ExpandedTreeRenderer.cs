using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core40k;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace KhRanks40k;

public static class ExpandedRankTreeRenderer
{
    private const float rankIconRectSize = 40f;
    private const float rankIconGapSize = 40f;
    private const float rankPlacementMult = rankIconGapSize + rankIconRectSize;
    private const float rankIconMargin = 20f;

    private static readonly CachedTexture LockedIcon = new("UI/Misc/LockedIcon");

    public static void FillRankTree(ITab_RankSystem instance, Rect rectRankTree)
    {
        var currentlySelectedRankCategory = GetField<RankCategoryDef>(instance, "currentlySelectedRankCategory");
        var scrollPosition = GetField<Vector2>(instance, "scrollPosition");
        var rankPos = GetField<Dictionary<RankDef, Vector2>>(instance, "rankPos");
        var availableRanksForCategory = GetField<IList>(instance, "availableRanksForCategory");
        var currentlySelectedRank = GetField<object>(instance, "currentlySelectedRank");
        var compRankInfo = GetField<CompRankInfo>(instance, "compRankInfo");

        if (currentlySelectedRankCategory == null) return;

        var viewRect = new Rect(rectRankTree);

        viewRect.ContractedBy(20f);

        var yStart = viewRect.height / 2 + rankIconRectSize;
        var xStart = rectRankTree.xMin + rankIconMargin;

        var (yMax, yMin, xMax) = GetYAndX(instance);

        //Needs correction on larger uiscale
        var newYMin = (Math.Abs(yMin * rankIconRectSize) + Math.Abs(yMin * rankIconGapSize) + rankIconMargin) *
                      Prefs.UIScale;
        var newYMax = (Math.Abs(yMax * rankIconRectSize) + Math.Abs(yMax * rankIconGapSize) + rankIconMargin) *
                      Prefs.UIScale;
        var newXMax = (Math.Abs(xMax * rankPlacementMult) + rankIconMargin * 2) * Prefs.UIScale;

        if (newYMin - yStart > 0) viewRect.yMin -= Math.Abs(newYMin);
        if (newYMax - yStart > 0) viewRect.yMax += Math.Abs(newYMax);
        if (newXMax > viewRect.width) viewRect.width = newXMax;

        Widgets.BeginScrollView(rectRankTree.ContractedBy(10f), ref scrollPosition, viewRect);

        SetField(instance, "scrollPosition", scrollPosition);

        Widgets.DrawRectFast(viewRect, new Color(0f, 0f, 0f, 0.3f));

        //Find positions for ranks if they're not presents. Will only happen when category is switched
        if (rankPos.NullOrEmpty())
            foreach (var rank in availableRanksForCategory)
            {
                var rankDef = GetRankDefFromRankInfo(rank); // Utiliser la méthode utilitaire

                var rankRect = new Rect
                {
                    width = rankIconRectSize,
                    height = rankIconRectSize,
                    x = xStart + currentlySelectedRankCategory.rankDict[rankDef].displayPosition.x * rankPlacementMult,
                    y = yStart + currentlySelectedRankCategory.rankDict[rankDef].displayPosition.y * rankPlacementMult
                };

                if (currentlySelectedRankCategory.rankDict[rankDef].displayPosition.x < 0)
                    Log.Error(rankDef.defName + " has display position with x < 0. Should be 0 or above");

                if (!rankPos.ContainsKey(rankDef)) rankPos.Add(rankDef, rankRect.position);
            }


        //Draws requirement lines
        foreach (var rank in availableRanksForCategory)
        {
            var rankDef = GetRankDefFromRankInfo(rank);

            if (currentlySelectedRankCategory.rankDict[rankDef].rankRequirements == null &&
                currentlySelectedRankCategory.rankDict[rankDef].rankRequirementsOneAmong == null) continue;

            // Default lines
            var normalRequirements = currentlySelectedRankCategory.rankDict[rankDef].rankRequirements;
            if (normalRequirements != null)
                foreach (var rankReq in normalRequirements)
                {
                    var startPos = new Vector2(rankPos[rankDef].x + rankIconRectSize / 2,
                        rankPos[rankDef].y + rankIconRectSize / 2);
                    var endPos = new Vector2(rankPos[rankReq.rankDef].x + rankIconRectSize / 2,
                        rankPos[rankReq.rankDef].y + rankIconRectSize / 2);

                    var rankUnlocked = compRankInfo.HasRank(rankReq.rankDef) ? Color.white : Color.grey;

                    if (currentlySelectedRank != null)
                    {
                        var selectedRankDef = GetRankDefFromRankInfo(currentlySelectedRank);
                        if (selectedRankDef == rankReq.rankDef)
                            rankUnlocked = new Color(0.0f, 0.5f, 1f, 0.9f);
                    }

                    Widgets.DrawLine(startPos, endPos, rankUnlocked, 2f);
                }

            // One-among lines
            var oneAmongRequirements = currentlySelectedRankCategory.rankDict[rankDef].rankRequirementsOneAmong;
            if (oneAmongRequirements != null)
                foreach (var rankReq in oneAmongRequirements)
                {
                    var startPos = new Vector2(rankPos[rankDef].x + rankIconRectSize / 2,
                        rankPos[rankDef].y + rankIconRectSize / 2);
                    var endPos = new Vector2(rankPos[rankReq.rankDef].x + rankIconRectSize / 2,
                        rankPos[rankReq.rankDef].y + rankIconRectSize / 2);

                    var rankUnlocked = compRankInfo.HasRank(rankReq.rankDef) ? Color.white : Color.grey;

                    if (currentlySelectedRank != null)
                    {
                        var selectedRankDef = GetRankDefFromRankInfo(currentlySelectedRank);
                        if (selectedRankDef == rankReq.rankDef)
                            rankUnlocked = new Color(0.0f, 0.5f, 1f, 0.9f);
                    }

                    DrawDashedLine(startPos, endPos, rankUnlocked, 2f, 6f, 4f);
                }
        }

//Draws icons
        foreach (var rank in availableRanksForCategory)
        {
            var rankDef = GetRankDefFromRankInfo(rank); // Utiliser la méthode utilitaire
            var requirementsMet = GetRequirementsMetFromRankInfo(rank); // Utiliser la méthode utilitaire

            var rankRect = new Rect
            {
                width = rankIconRectSize,
                height = rankIconRectSize,
                x = xStart + currentlySelectedRankCategory.rankDict[rankDef].displayPosition.x * rankPlacementMult,
                y = yStart + currentlySelectedRankCategory.rankDict[rankDef].displayPosition.y * rankPlacementMult
            };

            if (rank == currentlySelectedRank) Widgets.DrawStrongHighlight(rankRect.ExpandedBy(4f));

            DrawIcon(rankRect, rankDef.RankIcon, true);

            if (!AlreadyUnlocked(instance, rankDef))
            {
                var colour = requirementsMet ? new Color(0f, 0f, 0f, 0.55f) : new Color(0f, 0f, 0f, 0.9f);
                Widgets.DrawRectFast(rankRect, colour);
            }

            if (rankDef.incompatibleRanks != null)
                if (Enumerable.Any(rankDef.incompatibleRanks,
                        incompatibleRankDef => compRankInfo.HasRank(incompatibleRankDef)))
                    DrawIcon(rankRect, LockedIcon.Texture, false);

            if (Widgets.ButtonInvisible(rankRect))
                SetField(instance, "currentlySelectedRank", rank);

            TooltipHandler.TipRegion(rankRect, rankDef.label.CapitalizeFirst());
        }

        Widgets.EndScrollView();
    }

    private static bool AlreadyUnlocked(object instance, RankDef rankDef)
    {
        return Traverse.Create(instance).Method("AlreadyUnlocked", rankDef).GetValue<bool>();
    }

    private static RankDef GetRankDefFromRankInfo(object rankInfo)
    {
        return Traverse.Create(rankInfo).Field("rankDef").GetValue<RankDef>();
    }

    private static bool GetRequirementsMetFromRankInfo(object rankInfo)
    {
        return Traverse.Create(rankInfo).Field("requirementsMet").GetValue<bool>();
    }


    private static T GetField<T>(object instance, string fieldName)
    {
        var field = Traverse.Create(instance).Field(fieldName);
        var value = field.GetValue();

        if (value is T result)
            return result;

        Log.Warning(
            $"Cannot cast field '{fieldName}' of type '{value?.GetType()?.Name ?? "null"}' to '{typeof(T).Name}'");
        return default;
    }

    private static void SetField<T>(object instance, string fieldName, T value)
    {
        Traverse.Create(instance).Field(fieldName).SetValue(value);
    }

    private static (float yMax, float yMin, float xMax) GetYAndX(ITab_RankSystem instance)
    {
        return Traverse.Create(instance).Method("GetYAndX").GetValue<(float, float, float)>();
    }

    private static void DrawIcon(Rect inRect, Texture2D icon, bool drawBg)
    {
        GUI.color = Mouse.IsOver(inRect) ? GenUI.MouseoverColor : Color.white;
        if (drawBg)
            GUI.DrawTexture(inRect, Command.BGTexShrunk);
        GUI.color = Color.white;
        GUI.DrawTexture(inRect, icon);
    }

    private static void DrawDashedLine(Vector2 start, Vector2 end, Color color, float width, float dashLength,
        float gapLength)
    {
        var direction = (end - start).normalized;
        var totalDistance = Vector2.Distance(start, end);
        var dashGapLength = dashLength + gapLength;

        var currentPos = start;
        var distanceCovered = 0f;

        while (distanceCovered < totalDistance)
        {
            var remainingDistance = totalDistance - distanceCovered;
            var currentDashLength = Mathf.Min(dashLength, remainingDistance);

            var dashEnd = currentPos + direction * currentDashLength;
            Widgets.DrawLine(currentPos, dashEnd, color, width);

            distanceCovered += dashGapLength;
            currentPos += direction * dashGapLength;
        }
    }
}