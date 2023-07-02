using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using HarmonyLib.Tools;

namespace ConfigurableParrying
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ConfigurableParrying : BaseUnityPlugin
    {
        public static ConfigEntry<float> ParryingWindow;

        private static Harmony _harmony;
    
        private void Awake()
        {
            ParryingWindow = Config.Bind<float>("ConfigurableParrying", "Parrying Window", 0.25f,
                "Max time to parry after blocking");
            
            HarmonyFileLog.Enabled = true;
            _harmony = Harmony.CreateAndPatchAll(typeof(BlockAttackPatch));
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }

    [HarmonyPatch]
    public static class BlockAttackPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.BlockAttack))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true,
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_R4),
                new CodeMatch(OpCodes.Beq),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_R4))
                .Set(OpCodes.Ldc_R4, ConfigurableParrying.ParryingWindow.Value)
                .InstructionEnumeration();
        }
    }
}
