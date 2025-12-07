using System.IO;
using System.Reflection.Emit;
using System.Windows;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;

namespace RatioOutlineEffect
{
    internal class Patch : IPlugin
    {
        public string Name => "比率縁取り（パッチ）";

        public Patch()
        {
            var ymm4PluginDirectory = AppDirectories.PluginDirectory;

            var harmonyFolder = Path.Combine(ymm4PluginDirectory, "Harmony");
            var kerningFolder = Path.Combine(ymm4PluginDirectory, "SimpleKerningEffect");
            var ratioOutlineFolder = Path.Combine(ymm4PluginDirectory, "RatioOutlineEffect");
            var videoOutputMessageFolder = Path.Combine(ymm4PluginDirectory, "VideoOutputMessage");

            var harmonyYmmeDllName = "lib.har.ymmelib";

            // 0Harmony.ymmedllのパス候補
            var kerningHarmonyYmmeDll = Path.Combine(kerningFolder, harmonyYmmeDllName);
            var videoOutputMessageYmmeDll = Path.Combine(videoOutputMessageFolder, harmonyYmmeDllName);

            var harmonyDllName = "0Harmony.dll";

            // 0Harmony.dllのパス候補
            var harmonyDll = Path.Combine(harmonyFolder, harmonyDllName);
            var kerningHarmonyDll = Path.Combine(kerningFolder, harmonyDllName);
            var ratioOutlineHarmonyDll = Path.Combine(ratioOutlineFolder, harmonyDllName);
            var videoOutputMessageHarmonyDll = Path.Combine(videoOutputMessageFolder, harmonyDllName);

            List<string> pluginToUpdate = [];

            if (File.Exists(kerningHarmonyDll) && (!File.Exists(kerningHarmonyYmmeDll)))
                pluginToUpdate.Add("簡易カーニング");
            if (File.Exists(ratioOutlineHarmonyDll) && (!File.Exists(videoOutputMessageFolder)))
                pluginToUpdate.Add("動画出力メッセージ");

            List<string> fileToDelte = [];

            if (File.Exists(harmonyDll))
                fileToDelte.Add(harmonyDll);
            if (File.Exists(kerningHarmonyDll))
                fileToDelte.Add(kerningHarmonyDll);
            if (File.Exists(ratioOutlineHarmonyDll))
                fileToDelte.Add(ratioOutlineHarmonyDll);
            if (File.Exists(videoOutputMessageHarmonyDll))
                fileToDelte.Add(videoOutputMessageHarmonyDll);

            var message = "";

            for (int i = 0; i < pluginToUpdate.Count; i++)
            {
                if (i == 0)
                    message += "以下のプラグインはアップデートが必要です。\r\n最新版をダウンロードしてインストールしてください。\r\n";

                message += "・" + pluginToUpdate[i] + "\r\n";

                if (i == pluginToUpdate.Count - 1)
                    message += "\r\n";
            }

            for (int i = 0; i < fileToDelte.Count; i++)
            {
                if (i == 0)
                    message += "YMM4を終了して以下のファイルを削除してください。\r\n削除しないとYMM4が起動しなくなることがあります。\r\n";

                message += "・" + fileToDelte[i] + "\r\n";
            }

            if (message != "")
                MessageBox.Show(message, "比率縁取りプラグイン");

            var harmony = Activator.CreateInstance(HRef.Harmony, ["tetra_te.RatioOutlineEffect"]);

            var transpiler = typeof(Patch).GetMethod(nameof(Transpiler));
            var transpilerH = Activator.CreateInstance(HRef.HarmonyMethod, [transpiler]);

            var target = HRef.AccessToolsMethod.Invoke(null, ["YukkuriMovieMaker.Player.Video.TimelineSource:Update", null, null]);

            HRef.HarmonyPatch.Invoke(harmony, [target, null, null, transpilerH, null]);
        }

        public static IEnumerable<object> Transpiler(IEnumerable<object> instructions)
        {
            var field = HRef.AccessToolsField.Invoke(null, ["YukkuriMovieMaker.Player.Video.TimelineSource:scene"]);
            var save = typeof(DataStore).GetMethod(nameof(DataStore.SaveScene));

            yield return Activator.CreateInstance(HRef.CodeInstruction, [OpCodes.Ldarg_0, null])!;
            yield return Activator.CreateInstance(HRef.CodeInstruction, [OpCodes.Ldfld, field])!;
            yield return Activator.CreateInstance(HRef.CodeInstruction, [OpCodes.Call, save])!;

            foreach (var instruction in instructions)
            {                
                yield return instruction;
            }
        }
    }
}
