using BepInEx.Logging;
using HarmonyLib;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

namespace GK2DisplayTweaks
{

[HarmonyPatch(
    typeof(UIGameSettingsWindow),
    nameof(UIGameSettingsWindow.Init))]
internal static class UIGameSettingsWindowPatch
{

private static readonly ManualLogSource s_logger =
    BepInEx.Logging.Logger.CreateLogSource(
        "GK2 Settings");

private static TextMeshProUGUI s_uiScaleValueLabel;
private static TextMeshProUGUI s_cameraZoomValueLabel;

[HarmonyPostfix]
private static void Postfix(
    UIGameSettingsWindow __instance)
{
    Transform content =
        __instance.transform.Find(
            "GenericWIndowLayout/Content");

    if (content == null)
    {
        s_logger.LogError(
            "Settings Content object not found.");

        return;
    }

    Transform masterVolume =
        content.Find("MasterVolume");

    Transform speechVolume =
        content.Find("SpeechVolume");

    if (masterVolume == null ||
        speechVolume == null)
    {
        s_logger.LogError(
            "Could not find slider template objects.");

        return;
    }

    Transform uiScaleTransform =
        createUiScaleSlider(
            content,
            masterVolume,
            speechVolume);

    if (uiScaleTransform == null)
    {
        return;
    }

    createCameraZoomSlider(
        content,
        masterVolume,
        uiScaleTransform);
}

private static Transform createUiScaleSlider(
    Transform pContent,
    Transform pTemplate,
    Transform pPreviousItem)
{
    Transform existingSlider =
        pContent.Find("UIScale");

    if (existingSlider != null)
    {
        return existingSlider;
    }

    GameObject uiScaleObject =
        Object.Instantiate(
            pTemplate.gameObject,
            pContent);

    uiScaleObject.name = "UIScale";

    Transform uiScaleTransform =
        uiScaleObject.transform;

    uiScaleTransform.SetSiblingIndex(
        pPreviousItem.GetSiblingIndex() + 1);

    setCustomLabel(
        uiScaleTransform,
        "UI Scale");

    s_uiScaleValueLabel =
        getValueLabel(uiScaleTransform);

    UISlider slider =
        uiScaleObject.GetComponent<UISlider>();

    if (slider == null)
    {
        s_logger.LogError(
            "Cloned UI Scale object has no UISlider component.");

        Object.Destroy(uiScaleObject);
        return null;
    }

    float sliderValue =
        (Plugin.UiScale - 1.0f) *
        200.0f;

    slider.Initialize(
        onUiScaleChanged,
        sliderValue,
        10.0f);

    updateUiScaleLabel();

    s_logger.LogInfo(
        $"UI Scale slider created at " +
        $"{Plugin.UiScale * 100.0f:F0}%.");

    return uiScaleTransform;
}

private static void createCameraZoomSlider(
    Transform pContent,
    Transform pTemplate,
    Transform pPreviousItem)
{
    if (pContent.Find("CameraZoom") != null)
    {
        return;
    }

    GameObject cameraZoomObject =
        Object.Instantiate(
            pTemplate.gameObject,
            pContent);

    cameraZoomObject.name =
        "CameraZoom";

    Transform cameraZoomTransform =
        cameraZoomObject.transform;

    cameraZoomTransform.SetSiblingIndex(
        pPreviousItem.GetSiblingIndex() + 1);

    setCustomLabel(
        cameraZoomTransform,
        "Camera Zoom");

    s_cameraZoomValueLabel =
        getValueLabel(cameraZoomTransform);

    UISlider slider =
        cameraZoomObject.GetComponent<UISlider>();

    if (slider == null)
    {
        s_logger.LogError(
            "Cloned Camera Zoom object has no UISlider component.");

        Object.Destroy(cameraZoomObject);
        return;
    }

    float sliderValue =
        (Plugin.CameraZoom - 0.75f) /
        0.75f *
        100.0f;

    slider.Initialize(
        onCameraZoomChanged,
        sliderValue,
        100.0f / 15.0f);

    updateCameraZoomLabel();

    s_logger.LogInfo(
        $"Camera Zoom slider created at " +
        $"{Plugin.CameraZoom * 100.0f:F0}%.");
}

private static void setCustomLabel(
    Transform pSliderTransform,
    string text)
{
    Transform labelTransform =
        pSliderTransform.Find("LeftName");

    if (labelTransform == null)
    {
        s_logger.LogWarning(
            $"LeftName not found on " +
            $"{pSliderTransform.name}.");

        return;
    }

    LocalizedLabel localizedLabel =
        labelTransform.GetComponent<LocalizedLabel>();

    if (localizedLabel != null)
    {
        Object.DestroyImmediate(localizedLabel);
    }

    TextMeshProUGUI label =
        labelTransform.GetComponent<TextMeshProUGUI>();

    if (label == null)
    {
        s_logger.LogWarning(
            $"TextMeshProUGUI not found on " +
            $"{pSliderTransform.name}/LeftName.");

        return;
    }

    label.text = text;
}

private static TextMeshProUGUI getValueLabel(
    Transform pSliderTransform)
{
    Transform valueTransform =
        pSliderTransform.Find("Value");

    if (valueTransform == null)
    {
        return null;
    }

    return valueTransform.GetComponent<TextMeshProUGUI>();
}

private static void onUiScaleChanged(
    float value)
{
    float scale =
        1.0f +
        value / 200.0f;

    Plugin.SetUiScale(scale);

    updateUiScaleLabel();

    s_logger.LogInfo(
        $"UI Scale changed to " +
        $"{Plugin.UiScale * 100.0f:F0}%.");

    GUIElements guiElements =
        GUIElements.Instance;

    if (guiElements == null)
    {
        s_logger.LogWarning(
            "GUIElements.Instance is not available.");

        return;
    }

    guiElements.OnResolutionChanged(default);
}

private static void onCameraZoomChanged(
    float value)
{
    float zoom =
        0.75f +
        value / 100.0f *
        0.75f;

    zoom =
        Mathf.Round(zoom * 20.0f) /
        20.0f;

    Plugin.SetCameraZoom(zoom);

    updateCameraZoomLabel();

    s_logger.LogInfo(
        $"Camera Zoom changed to " +
        $"{Plugin.CameraZoom * 100.0f:F0}%.");

    CameraSystem cameraSystem =
        CameraSystem.Instance;

    if (cameraSystem == null)
    {
        s_logger.LogWarning(
            "CameraSystem.Instance is not available.");

        return;
    }

    if (GameSettings.Instance == null)
    {
        s_logger.LogWarning(
            "GameSettings.Instance is not available.");

        return;
    }

    IntVector2 resolution =
        GameSettings.Instance.GetResolutionIntVector2();

    cameraSystem.OnResolutionChanged(
        resolution);
}

private static void updateUiScaleLabel()
{
    if (s_uiScaleValueLabel == null)
    {
        return;
    }

    s_uiScaleValueLabel.text =
        $"{Plugin.UiScale * 100.0f:F0}%";
}

private static void updateCameraZoomLabel()
{
    if (s_cameraZoomValueLabel == null)
    {
        return;
    }

    s_cameraZoomValueLabel.text =
        $"{Plugin.CameraZoom * 100.0f:F0}%";
}

}

}