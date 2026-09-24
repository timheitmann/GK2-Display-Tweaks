using HarmonyLib;

namespace GK2DisplayTweaks
{

[HarmonyPatch(
    typeof(ResolutionConfig),
    nameof(ResolutionConfig.GetUiScaleFactor))]
internal static class ResolutionConfigPatch
{

[HarmonyPostfix]
private static void Postfix(
    ref float __result)
{
    __result *= Plugin.UiScale;
}

}

}