using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;

namespace GK2DisplayTweaks
{

[HarmonyPatch(
    typeof(ResolutionConfig),
    nameof(ResolutionConfig.InitAvailableResolutions))]
internal static class ResolutionLimitPatch
{

private static readonly ManualLogSource s_logger =
    BepInEx.Logging.Logger.CreateLogSource(
        "GK2 Resolution Limit");

[HarmonyTranspiler]
private static IEnumerable<CodeInstruction> Transpiler(
    IEnumerable<CodeInstruction> instructions)
{
    List<CodeInstruction> result =
        new List<CodeInstruction>(instructions);

    int replacementCount = 0;

    foreach (CodeInstruction instruction in result)
    {
        if (instruction.opcode == OpCodes.Ldc_I4 &&
            instruction.operand is int value &&
            value == 5120)
        {
            instruction.operand = int.MaxValue;
            replacementCount++;
        }
    }

    if (replacementCount != 1)
    {
        throw new InvalidOperationException(
            $"Expected exactly one 5120 resolution limit, " +
            $"but found {replacementCount}.");
    }

    s_logger.LogInfo(
        "Maximum resolution width limit removed.");

    return result;
}

}

}