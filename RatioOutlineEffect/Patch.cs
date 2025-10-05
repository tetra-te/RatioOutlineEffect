using System.Reflection.Emit;
using HarmonyLib;
using YukkuriMovieMaker.Plugin;

namespace RatioOutlineEffect
{
    internal class Patch : IPlugin
    {
        public string Name => "比率縁取り（パッチ）";

        public Patch()
        {
            var harmony = new Harmony("tetra_te.RatioOutlineEffect");

            var transpiler = typeof(Patch).GetMethod(nameof(Transpiler));

            var target = AccessTools.Method("YukkuriMovieMaker.Player.Video.TimelineSource:Update");

            harmony.Patch(target, transpiler: transpiler);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var field = AccessTools.Field("YukkuriMovieMaker.Player.Video.TimelineSource:scene");
            var save = typeof(DataStore).GetMethod(nameof(DataStore.SaveScene));

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, field);
            yield return new CodeInstruction(OpCodes.Call, save);

            foreach (var instruction in instructions)
            {                
                yield return instruction;
            }
        }
    }
}
