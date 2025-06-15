using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace RatioOutlineEffect
{
    [VideoEffect("縁取り（比率）", ["装飾"], [], IsEffectItemSupported = false, IsAviUtlSupported = false)]
    internal class RatioOutlineEffect : VideoEffectBase
    {        
        public override string Label => "縁取り（比率）";

        [Display(GroupName = "縁取り（比率）", Name = "太さ", Description = "線の太さ")]
        [AnimationSlider("F1", "%", 0, 10)]
        public Animation StrokeThicknessRate { get; } = new Animation(7, 0, 1000);

        [Display(GroupName = "縁取り（比率）", Name = "ぼかし", Description = "線をぼかす")]
        [AnimationSlider("F1", "px", 0, 5)]
        public Animation Blur { get; } = new Animation(0, 0, 1000);

        [Display(GroupName = "縁取り（比率）", Name = "縁のみ", Description = "縁のみ")]
        [ToggleSlider]
        public bool IsOutlineOnly { get => isOutlineOnly; set => Set(ref isOutlineOnly, value); }
        bool isOutlineOnly = false;

        [Display(GroupName = "縁取り（比率）", Name = "角縁取り", Description = "角ばった縁取りにします")]
        [ToggleSlider]
        public bool IsAngular { get => isAngular; set => Set(ref isAngular, value); }
        bool isAngular = false;

        [Display(GroupName = "縁取り（比率） / 描画", Name = "X", Description = "X")]
        [AnimationSlider("F1", "px", -50, 50)]
        public Animation X { get; } = new Animation(0, -100000, 100000);

        [Display(GroupName = "縁取り（比率） / 描画", Name = "Y", Description = "Y")]
        [AnimationSlider("F1", "px", -50, 50)]
        public Animation Y { get; } = new Animation(0, -100000, 100000);

        [Display(GroupName = "縁取り（比率） / 描画", Name = "不透明度", Description = "不透明度")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation Opacity { get; } = new Animation(100, 0, 100);

        [Display(GroupName = "縁取り（比率） / 描画", Name = "拡大率", Description = "拡大率")]
        [AnimationSlider("F1", "%", 0, 400)]
        public Animation Zoom { get; } = new Animation(100, 0, 100000);

        [Display(GroupName = "縁取り（比率） / 描画", Name = "回転角", Description = "回転角")]
        [AnimationSlider("F1", "°", -360.0, 360.0)]
        public Animation Rotation { get; } = new Animation(0.0, -100000.0, 100000.0);

        [Display(GroupName = "縁の模様", AutoGenerateField = true)]
        public YukkuriMovieMaker.Plugin.Brush.Brush StrokeBruch { get; } = new YukkuriMovieMaker.Plugin.Brush.Brush();

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new RatioOutlineEffectProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [StrokeThicknessRate, Blur, X, Y, Opacity, Zoom, Rotation, StrokeBruch];
    }
}
