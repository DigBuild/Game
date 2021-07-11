using System.Text.Json;
using DigBuild.Engine.Registries;
using DigBuild.Platform.Resource;

namespace DigBuild.Render.Models.Geometry
{
    public interface IGeometryProvider
    {
        IPartialGeometry Provide(JsonElement json, JsonSerializerOptions jsonOptions, ResourceManager resourceManager, ResourceName currentFile);
    }

    public static class GeometryProviderRegistryBuilderExtensions
    {
        public static T Add<T>(this IRegistryBuilder<IGeometryProvider> registry, string domain, string path, T provider)
            where T : IGeometryProvider
        {
            return Register(registry, domain, path, provider);
        }

        public static T Register<T>(this IRegistryBuilder<IGeometryProvider> registry, string domain, string path, T provider)
            where T : IGeometryProvider
        {
            return registry.Add(new ResourceName(domain, path), provider);
        }
    }
}