using System.Reflection;
using Core40k;
using HarmonyLib;
using Verse;

namespace KhRanks40k;

public class KhRanks40kMod : Mod
{
    public static Harmony harmony;

    public KhRanks40kMod(ModContentPack content) : base(content)
    {
        Log.Message("[KhRanks40k] Mod constructor called");
        harmony = new Harmony("khranks40k.mod");
        harmony.PatchAll();
        Log.Message("[KhRanks40k] Harmony patches applied");

        // Vérifier spécifiquement si ta méthode est patchée
        var targetMethod = typeof(ITab_RankSystem).GetMethod("RequirementMetAndText",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (targetMethod != null)
        {
            Log.Message($"[KhRanks40k] Target method found: {targetMethod.Name}");
            var patches = Harmony.GetPatchInfo(targetMethod);
            if (patches != null) Log.Message($"[KhRanks40k] Method has {patches.Postfixes.Count} postfixes");
        }
        else
        {
            Log.Message("[KhRanks40k] Target method not found");
        }
    }
}