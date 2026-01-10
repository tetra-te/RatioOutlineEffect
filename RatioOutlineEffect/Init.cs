using System.IO;
using System.Windows;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;

namespace RatioOutlineEffect
{
    internal class Init : IPlugin
    {
        public string Name => "比率縁取り（互換性処理）";

        public Init()
        {
            var ratioOutlineEffectDir = Path.Combine(AppDirectories.PluginDirectory, "RatioOutlineEffect");

            var harmonyPath = Path.Combine(ratioOutlineEffectDir, "0Harmony.dll");

            if (File.Exists(harmonyPath))
            {
                MessageBox.Show($"以下のファイルはこのバージョンでは不要ですので削除をおすすめします。\r\n\r\n{harmonyPath.Replace('\\', '/')}", "比率縁取りプラグイン");
            }
        }
    }
}
