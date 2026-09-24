using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace GK2DisplayTweaks
{

[BepInPlugin(
    "de.timheitmann.gk2displaytweaks",
    "GK2 Display Tweaks",
    "1.1.0")]
public sealed class Plugin : BaseUnityPlugin
{

public static float UiScale { get; private set; } = 1.0f;

public static float CameraZoom { get; private set; } = 1.0f;

private static ConfigEntry<float> s_uiScaleConfig;

private static ConfigEntry<float> s_cameraZoomConfig;

private Harmony m_harmony;

private void Awake()
{
    Logger.LogInfo("GK2 Display Tweaks plugin loaded.");

    s_uiScaleConfig = Config.Bind(
        "UI",
        "Scale",
        1.0f,
        "UI scale factor. Valid range: 1.0 to 1.5.");

    SetUiScale(s_uiScaleConfig.Value);

    s_cameraZoomConfig = Config.Bind(
        "Camera",
        "Zoom",
        1.0f,
        "Camera zoom factor. Valid range: 0.75 to 1.5.");

    SetCameraZoom(s_cameraZoomConfig.Value);

    m_harmony =
        new Harmony("de.timheitmann.gk2displaytweaks");

    m_harmony.PatchAll();

    Logger.LogInfo(
        $"UI scale initialized to {UiScale:F2}.");

    Logger.LogInfo("Harmony patches applied.");
}

public static void SetUiScale(
    float scale)
{
    scale = UnityEngine.Mathf.Clamp(
        scale,
        1.0f,
        1.5f);

    UiScale = scale;

    if (s_uiScaleConfig != null)
    {
        s_uiScaleConfig.Value = scale;
    }
}

public static void SetCameraZoom(
    float zoom)
{
    zoom = UnityEngine.Mathf.Clamp(
        zoom,
        0.75f,
        1.5f);

    CameraZoom = zoom;

    if (s_cameraZoomConfig != null)
    {
        s_cameraZoomConfig.Value = zoom;
    }
}

private void OnDestroy()
{
    m_harmony?.UnpatchSelf();
}

}

}