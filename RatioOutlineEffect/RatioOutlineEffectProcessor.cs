using System.Collections.Immutable;
using System.Reflection;
using System.Windows;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Effects;
using YukkuriMovieMaker.Project.Items;

namespace RatioOutlineEffect
{
    internal class RatioOutlineEffectProcessor : IVideoEffectProcessor
    {
        readonly RatioOutlineEffect param;
        
        readonly OutlineEffect outlineItem;
        readonly IVideoEffectProcessor outlineProcessor;

        ID2D1Image? input;
        readonly IGraphicsDevicesAndContext devices;

        public ID2D1Image Output { get; }

        public RatioOutlineEffectProcessor(IGraphicsDevicesAndContext devices, RatioOutlineEffect item)
        {
            param= item;
            this.devices= devices;
            outlineItem = new OutlineEffect();
            outlineProcessor = outlineItem.CreateVideoEffect(devices);
            Output = outlineProcessor.Output;
        }      

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var scene = DataStore.GetScene(effectDescription.SceneId);

            if (scene is null)
                return effectDescription.DrawDescription;

            var timeline = scene.Timeline;
            var items = timeline.Items;

            var layer = effectDescription.Layer;
            var itemPosition = effectDescription.TimelinePosition.Frame - frame;

            var item = items.Where(i => i.Layer == layer && i.Frame == itemPosition)
                            .FirstOrDefault();

            var size = 0d;

            if (item is TextItem textItem)
            {
                size = textItem.FontSize.GetValue(frame, length, fps);
            }
            else if (item is VoiceItem voiceItem)
            {
                if (voiceItem.JimakuVisibility == JimakuVisibility.Custom)
                {
                    size = voiceItem.FontSize.GetValue(frame, length, fps);
                }
                else
                {
                    size = voiceItem.Character.FontSize.GetValue(frame, length, fps);
                }
            }
            else if (item is ShapeItem shapeItem)
            {
                if (shapeItem.ShapeParameter is SizeAndAspectShapeParameterBase sa)
                {
                    if (sa.SizeMode == SizeMode.SizeAspect)
                        size = sa.Size.GetValue(frame, length, fps);
                    else
                        size = Math.Sqrt(sa.Width.GetValue(frame, length, fps) * sa.Height.GetValue(frame, length, fps));
                }
                else if (shapeItem.ShapeParameter is TimerShapeParameter t)
                {
                    size = t.FontSize.GetValue(frame, length, fps);
                }
                else if (shapeItem.ShapeParameter.GetType().FullName == "YukkuriMovieMaker.Shape.LineShapeParameter" &&
                    shapeItem.ShapeParameter.GetType().GetProperty("Thickness", BindingFlags.Public | BindingFlags.Instance)?.GetValue(shapeItem.ShapeParameter) is Animation thickness)
                {
                    size = thickness.GetValue(frame, length, fps);
                }
                else if (shapeItem.ShapeParameter.GetType().FullName == "YukkuriMovieMaker.Plugin.Community.Shape.NumberText.NumberTextParameter" &&
                    shapeItem.ShapeParameter.GetType().GetProperty("FontSize", BindingFlags.Public | BindingFlags.Instance)?.GetValue(shapeItem.ShapeParameter) is Animation fontSize)
                {
                    size = fontSize.GetValue(frame, length, fps);
                }
                else
                {
                    var bounds = devices.DeviceContext.GetImageLocalBounds(input);
                    var width = bounds.Right - bounds.Left;
                    var height = bounds.Bottom - bounds.Top;
                    size = Math.Sqrt(width * height);
                }
            }
            else
            {
                var bounds = devices.DeviceContext.GetImageLocalBounds(input);
                var width = bounds.Right - bounds.Left;
                var height = bounds.Bottom - bounds.Top;
                size = Math.Sqrt(width * height);
            }

            var strokeThickness = size * param.StrokeThicknessRate.GetValue(frame, length, fps) / 100;
            strokeThickness = Math.Min(strokeThickness, 500);
            SetAnimationValue(outlineItem.StrokeThickness, strokeThickness);

            var blur = size * param.BlurRate.GetValue(frame, length, fps) / 100;
            SetAnimationValue(outlineItem.Blur, blur);

            outlineItem.IsOutlineOnly = param.IsOutlineOnly;

            outlineItem.IsAngular = param.IsAngular;

            var x = param.X.GetValue(frame, length, fps);
            SetAnimationValue(outlineItem.X, x);

            var y = param.Y.GetValue(frame, length, fps);
            SetAnimationValue(outlineItem.Y, y);

            var opacity = param.Opacity.GetValue(frame, length, fps);
            SetAnimationValue(outlineItem.Opacity, opacity);

            var zoom = param.Zoom.GetValue(frame, length, fps);
            SetAnimationValue(outlineItem.Zoom, zoom);

            var rotation = param.Rotation.GetValue(frame, length, fps);
            SetAnimationValue(outlineItem.Rotation, rotation);

            outlineItem.StrokeBrush.Parameter = param.StrokeBruch.Parameter;

            return outlineProcessor.Update(effectDescription);
        }

        static void SetAnimationValue(Animation animation, double value)
        {
            var current = animation.GetValue(0, 1, 30);
            animation.AddToEachValues(value - current);
        }

        public void ClearInput()
        {
            input = null;
            outlineProcessor.ClearInput();
        }

        public void Dispose()
        {
            outlineProcessor.Dispose();
            Output.Dispose();
        }

        public void SetInput(ID2D1Image? input)
        {
            this.input = input;
            outlineProcessor.SetInput(input);
        }
    }
}
