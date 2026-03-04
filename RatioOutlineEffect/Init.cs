using System.IO;
using System.Reflection;
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

    internal static class Reflection
    {
        private static Assembly yukkuriMovieMaker = Assembly.Load("YukkuriMovieMaker");

        public static Type OutlineEffect = yukkuriMovieMaker.GetType("YukkuriMovieMaker.Project.Effects.OutlineLiteEffect") ?? yukkuriMovieMaker.GetType("YukkuriMovieMaker.Project.Effects.OutlineEffect")!;

        public static PropertyInfo? StrokeThickness = OutlineEffect.GetProperty("StrokeThickness", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Blur = OutlineEffect.GetProperty("Blur", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Quality = OutlineEffect.GetProperty("Quality", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Smoothness = OutlineEffect.GetProperty("Smoothness", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? IsOutlineOnly = OutlineEffect.GetProperty("IsOutlineOnly", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? IsAngular = OutlineEffect.GetProperty("IsAngular", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? X = OutlineEffect.GetProperty("X", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Y = OutlineEffect.GetProperty("Y", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Opacity = OutlineEffect.GetProperty("Opacity", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Zoom = OutlineEffect.GetProperty("Zoom", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? Rotation = OutlineEffect.GetProperty("Rotation", BindingFlags.Public | BindingFlags.Instance);
        public static PropertyInfo? StrokeBrush = OutlineEffect.GetProperty("StrokeBrush", BindingFlags.Public | BindingFlags.Instance);
    }
}
