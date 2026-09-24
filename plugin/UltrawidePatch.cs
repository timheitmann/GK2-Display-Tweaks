using HarmonyLib;

namespace GK2DisplayTweaks
{

[HarmonyPatch(
    typeof(ResolutionConfig),
    "IsUltraWide")]
internal static class UltrawidePatch
{

[HarmonyPrefix]
private static bool Prefix(
    ref bool __result)
{
    __result = false;
    return false;
}

}

}