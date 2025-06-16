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

        ImmutableList<Timeline>? timelines;
        ImmutableList<IItem>? items;
        IItem? item;
        bool isFirst = true;
        Guid sceneId;
        int itemPosition, layer;

        public ID2D1Image Output { get; }

        public RatioOutlineEffectProcessor(IGraphicsDevicesAndContext devices, RatioOutlineEffect item)
        {
            param= item;
            this.devices= devices;
            outlineItem = new OutlineEffect();
            outlineProcessor = outlineItem.CreateVideoEffect(devices);
            Output = outlineProcessor.Output;


            object viewModel = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = Application.Current.Windows.OfType<Window>().First(w => w.GetType().FullName == "YukkuriMovieMaker.Views.MainView");
                viewModel = window.DataContext;
            });

            var viewModelType = viewModel.GetType();
            var modelInfo = viewModelType.GetField("model", BindingFlags.Instance | BindingFlags.NonPublic);
            var model = modelInfo.GetValue(viewModel);

            var modelType = model.GetType();
            var scenesInfo = modelType.GetProperty("Scenes");
            var scenes = (Scenes)scenesInfo.GetValue(model);

            var timelineInfo = typeof(Scenes).GetField("timelines", BindingFlags.Instance | BindingFlags.NonPublic);
            timelines = (ImmutableList<Timeline>)timelineInfo.GetValue(scenes);
        }      

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var itemPosition = effectDescription.TimelinePosition.Frame - frame;
            var layer = effectDescription.Layer;

            if (isFirst || sceneId != effectDescription.SceneId)
            {
                // シーンIDが変わったらtimelineを再度探す
                foreach (var timeline in timelines)
                {
                    if (timeline.ID == effectDescription.SceneId)
                    {
                        var itemsInfo = typeof(Timeline).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
                        items = (ImmutableList<IItem>)itemsInfo.GetValue(timeline);
                        break;
                    }
                }
                sceneId = effectDescription.SceneId;
            }
          
            if (isFirst || this.itemPosition != itemPosition || this.layer != layer || true)
            {
                // アイテムの位置かレイヤーが変わったらitemを再度探す
                foreach (var item in items)
                {
                    if (item.Frame == itemPosition && item.Layer == layer)
                    {
                        this.item = item;
                        break;
                    }
                }
                this.itemPosition = itemPosition;
                this.layer = layer;
            }

            isFirst = false;

            var size = 0d;

            if (item is TextItem)
            {
                var textItem = (TextItem)item;
                size = textItem.FontSize.GetValue(frame, length, fps);
            }          
            else if (item is VoiceItem)
            {
                var voiceItem = (VoiceItem)item;

                if (voiceItem.JimakuVisibility == JimakuVisibility.Custom)
                {
                    size = voiceItem.FontSize.GetValue(frame, length, fps);
                }
                else
                {
                    size = voiceItem.Character.FontSize.GetValue(frame, length, fps);
                }
            }
            else if (item is ShapeItem)
            {
                var shapeItem = (ShapeItem)item;
                var sizeAndAspectShapeParameter = (SizeAndAspectShapeParameterBase)shapeItem.ShapeParameter;
                size = sizeAndAspectShapeParameter.Size.GetValue(frame, length, fps);
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
