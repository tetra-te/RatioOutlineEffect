using System.Reflection;
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

            var jimaku = AccessTools.Method("YukkuriMovieMaker.Player.Video.Items.JimakuSource:Update");
            var shape = AccessTools.Method("YukkuriMovieMaker.Player.Video.Items.ShapeSource:Update");
            var text = AccessTools.Method("YukkuriMovieMaker.Player.Video.Items.TextSource:Update");

            harmony.Patch(jimaku, transpiler: transpiler);
            harmony.Patch(shape, transpiler: transpiler);
            harmony.Patch(text, transpiler: transpiler);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var field = AccessTools.Field(method.DeclaringType, "item");
            var save = typeof(DataStore).GetMethod(nameof(DataStore.SaveItem));
            
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field);
                    yield return new CodeInstruction(OpCodes.Call, save);
                }
                
                yield return instruction;
            }
        }
    }
}
