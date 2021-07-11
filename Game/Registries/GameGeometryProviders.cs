using DigBuild.Engine.Registries;
using DigBuild.Render.Models.Geometry;

namespace DigBuild.Registries
{
    public static class GameGeometryProviders
    {
        internal static void Register(RegistryBuilder<IGeometryProvider> registry)
        {
            registry.Add(DigBuildGame.Domain, "cuboid", new CuboidGeometryProvider());
            registry.Add(DigBuildGame.Domain, "include", new IncludeGeometryProvider());
            registry.Add(DigBuildGame.Domain, "include_obj", new IncludeObjGeometryProvider());
        }
    }
}