using System;
using System.Text;
using Core40k;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace KhRanks40k;

[HarmonyPatch(typeof(ITab_RankSystem), "RequirementMetAndText")]
public static class PatchRequirementMetAndText
{
    private static void Postfix(RankDef rankDef, ref (bool requirementMet, string requirementText) __result,
        ITab_RankSystem __instance)
    {
        if (rankDef is ExpandedRankDef expanded)
            if (!expanded.requiredAgeRange.Equals(new Vector2(-1, -1)))
            {
                var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                var compRankInfo = Traverse.Create(__instance).Field("compRankInfo").GetValue<CompRankInfo>();
                var noneTxt = "    " + "BEWH.Framework.CommonKeyword.None".Translate();
                var stringBuilder =
                    new StringBuilder(__result.requirementText == noneTxt ? null : __result.requirementText);

                var ageRequirementsMet = true;

                if (expanded.requiredAgeRange.x >= 0)
                {
                    var minAge = expanded.requiredAgeRange.x;
                    var hasMinAge = pawn.ageTracker.AgeBiologicalYears >= minAge;
                    var requirementMetColour = Color.white;
                    var requirementNotMetColour = new Color(1f, 0.0f, 0.0f, 0.8f);
                    var color = hasMinAge ? requirementMetColour : requirementNotMetColour;

                    if (!hasMinAge)
                    {
                        stringBuilder.AppendLine(
                            ("    " + "KHWH.RankSystem.MinimumBiologicalAge".Translate(minAge)).Colorize(color));
                        ageRequirementsMet = false;
                    }
                }

                if (expanded.requiredAgeRange.y >= 0)
                {
                    var maxAge = expanded.requiredAgeRange.y;
                    var hasMaxAge = pawn.ageTracker.AgeBiologicalYears <= maxAge;
                    var requirementMetColour = Color.white;
                    var requirementNotMetColour = new Color(1f, 0.0f, 0.0f, 0.8f);
                    var color = hasMaxAge ? requirementMetColour : requirementNotMetColour;

                    if (!hasMaxAge)
                    {
                        stringBuilder.AppendLine(
                            ("    " + "KHWH.RankSystem.MaximumBiologicalAge".Translate(maxAge)).Colorize(color));
                        ageRequirementsMet = false;
                    }
                }


                __result = (ageRequirementsMet && __result.requirementMet, stringBuilder.ToString());
                Log.Warning("[KhRanks40k] PatchRequirementMetAndText called for rankDef: " + rankDef.defName + " " +
                            __result);
            }
    }
}

[HarmonyPatch(typeof(ITab_RankSystem), "FillRankTree")]
public static class PatchReplaceOriginalFillRankTree
{
    private static bool Prefix(ITab_RankSystem __instance, Rect rectRankTree)
    {
        try
        {
            ExpandedRankTreeRenderer.FillRankTree(__instance, rectRankTree);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"Erreur dans le patch FillRankTree: {ex}");
            return true; // Fallback to original
        }
    }
}