using System.Reflection;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Brush;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;

namespace RatioOutlineEffect
{
    internal class RatioOutlineEffectProcessor : IVideoEffectProcessor
    {
        readonly RatioOutlineEffect param;
        
        readonly IVideoEffect outlineItem;
        readonly IVideoEffectProcessor outlineProcessor;

        ID2D1Image? input;
        readonly IGraphicsDevicesAndContext devices;

        public ID2D1Image Output { get; }

        public RatioOutlineEffectProcessor(IGraphicsDevicesAndContext devices, RatioOutlineEffect item)
        {
            param = item;
            this.devices = devices;

            outlineItem = (IVideoEffect)Activator.CreateInstance(Reflection.OutlineEffect);
            outlineProcessor = outlineItem.CreateVideoEffect(devices);
            Output = outlineProcessor.Output;
        }      

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var size = 0d;

            var a = true;
            while (a)
            {
                a = false;

                var sceneInfo = effectDescription.Scenes.FirstOrDefault(s => s.ID == effectDescription.SceneId);

                var scene = sceneInfo is Scene s ? s : null;

                if (scene is null)
                {
                    break;
                }

                var items = scene.Timeline.Items;

                var layer = effectDescription.Layer;
                var itemPosition = effectDescription.TimelinePosition.Frame - frame;

                var item = items.FirstOrDefault(i => i.Layer == layer && i.Frame == itemPosition);

                if (item is TextItem textItem)
                {
                    size = textItem.FontSize.GetValue(frame, length, fps);
                    break;
                }
                else if (item is VoiceItem voiceItem)
                {
                    if (voiceItem.JimakuVisibility == JimakuVisibility.Custom)
                    {
                        size = voiceItem.FontSize.GetValue(frame, length, fps);
                        break;
                    }
                    else
                    {
                        size = voiceItem.Character.FontSize.GetValue(frame, length, fps);
                        break;
                    }
                }
                else if (item is ShapeItem shapeItem)
                {
                    if (shapeItem.ShapeParameter is SizeAndAspectShapeParameterBase sa)
                    {
                        if (sa.SizeMode == SizeMode.SizeAspect)
                        {
                            size = sa.Size.GetValue(frame, length, fps);
                            break;
                        }
                        else
                        {
                            size = Math.Sqrt(sa.Width.GetValue(frame, length, fps) * sa.Height.GetValue(frame, length, fps));
                            break;
                        }
                    }
                    else if (shapeItem.ShapeParameter is TimerShapeParameter t)
                    {
                        size = t.FontSize.GetValue(frame, length, fps);
                        break;
                    }
                    else if (shapeItem.ShapeParameter.GetType().FullName == "YukkuriMovieMaker.Shape.LineShapeParameter" &&
                        shapeItem.ShapeParameter.GetType().GetProperty("Thickness", BindingFlags.Public | BindingFlags.Instance)?.GetValue(shapeItem.ShapeParameter) is Animation thickness)
                    {
                        size = thickness.GetValue(frame, length, fps);
                        break;
                    }
                    else if (shapeItem.ShapeParameter.GetType().FullName == "YukkuriMovieMaker.Plugin.Community.Shape.NumberText.NumberTextParameter" &&
                        shapeItem.ShapeParameter.GetType().GetProperty("FontSize", BindingFlags.Public | BindingFlags.Instance)?.GetValue(shapeItem.ShapeParameter) is Animation fontSize)
                    {
                        size = fontSize.GetValue(frame, length, fps);
                        break;
                    }
                    else
                    {
                        var bounds = devices.DeviceContext.GetImageLocalBounds(input);
                        var width = bounds.Right - bounds.Left;
                        var height = bounds.Bottom - bounds.Top;
                        size = Math.Sqrt(width * height);
                        break;
                    }
                }
                else
                {
                    var bounds = devices.DeviceContext.GetImageLocalBounds(input);
                    var width = bounds.Right - bounds.Left;
                    var height = bounds.Bottom - bounds.Top;
                    size = Math.Sqrt(width * height);
                    break;
                }
            }


            if (Reflection.StrokeThickness is not null)
            {
                var strokeThickness = size * param.StrokeThicknessRate.GetValue(frame, length, fps) / 100;
                strokeThickness = Math.Min(strokeThickness, 500);
                var anm = (Animation?)Reflection.StrokeThickness.GetValue(outlineItem);
                SetAnimationValue(anm, strokeThickness);
            }
            if (Reflection.Blur is not null)
            {
                var blur = size * param.BlurRate.GetValue(frame, length, fps) / 100;
                var anm = (Animation?)Reflection.Blur.GetValue(outlineItem);
                SetAnimationValue(anm, blur);
            }
            if (Reflection.Quality is not null)
            {
                var quality = param.Quality.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.Quality.GetValue(outlineItem);
                SetAnimationValue(anm, quality);
            }
            if (Reflection.Smoothness is not null)
            {
                var smoothness = param.Smoothness.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.Smoothness.GetValue(outlineItem);
                SetAnimationValue(anm, smoothness);
            }
            if (Reflection.IsOutlineOnly is not null)
            {
                Reflection.IsOutlineOnly.SetValue(outlineItem, param.IsOutlineOnly);
            }
            if (Reflection.IsAngular is not null)
            {
                Reflection.IsAngular.SetValue(outlineItem, param.IsAngular);
            }
            if (Reflection.X is not null)
            {
                var x = param.X.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.X.GetValue(outlineItem);
                SetAnimationValue(anm, x);
            }
            if (Reflection.Y is not null)
            {
                var y = param.Y.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.Y.GetValue(outlineItem);
                SetAnimationValue(anm, y);
            }
            if (Reflection.Opacity is not null)
            {
                var opacity = param.Opacity.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.Opacity.GetValue(outlineItem);
                SetAnimationValue(anm, opacity);
            }
            if (Reflection.Zoom is not null)
            {
                var zoom = param.Zoom.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.Zoom.GetValue(outlineItem);
                SetAnimationValue(anm, zoom);
            }
            if (Reflection.Rotation is not null)
            {
                var rotation = param.Rotation.GetValue(frame, length, fps);
                var anm = (Animation?)Reflection.Rotation.GetValue(outlineItem);
                SetAnimationValue(anm, rotation);
            }
            if (Reflection.StrokeBrush is not null)
            {
                var brush = (Brush?)Reflection.StrokeBrush.GetValue(outlineItem);
                brush?.Parameter = param.StrokeBruch.Parameter;
            }

            return outlineProcessor.Update(effectDescription);
        }

        static void SetAnimationValue(Animation? animation, double value)
        {
            if (animation is null) return;
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
