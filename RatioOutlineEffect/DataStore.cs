using System.Collections.Concurrent;
using YukkuriMovieMaker.Project;

namespace RatioOutlineEffect
{
    internal static class DataStore
    {
        private static ConcurrentDictionary<Guid, Scene> scenes = new();

        public static void SaveScene(Scene scene)
        {
            scenes[scene.ID] = scene;
        }

        public static Scene? GetScene(Guid sceneId)
        {
            Scene? scene;
            scenes.TryGetValue(sceneId, out scene);
            return scene;
        }
    }
}
