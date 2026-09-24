using HarmonyLib;

namespace GK2DisplayTweaks
{

[HarmonyPatch(
    typeof(CameraSystem),
    nameof(CameraSystem.CalculateOrthographicSize))]
internal static class CameraSystemPatch
{

[HarmonyPostfix]
private static void Postfix(
    ref float __result)
{
    __result /= Plugin.CameraZoom;
}

}

}