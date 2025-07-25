using System.Collections.Concurrent;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Project.Items;

namespace RatioOutlineEffect
{
    internal static class DataStore
    {
        private static ConcurrentDictionary<(Guid, int), IItem> items = new();

        public static void SaveItem(TimelineItemSourceDescription desc, IItem item)
        {
            items[(desc.SceneId, desc.Layer)] = item;
        }

        public static IItem? GetItem(EffectDescription desc)
        {
            IItem? item;
            items.TryGetValue((desc.SceneId, desc.Layer), out item);
            return item;
        }
    }
}
