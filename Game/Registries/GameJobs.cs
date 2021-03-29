using System;
using DigBuild.Engine.Registries;
using DigBuild.Engine.Ticking;
using DigBuild.Platform.Resource;

namespace DigBuild.Registries
{
    public static class GameJobs
    {
        public static JobHandle<string> DelayedPrinter { get; private set; } = null!;

        internal static void Register(RegistryBuilder<IJobHandle> registry)
        {
            DelayedPrinter = registry.CreateSequential<string>(new ResourceName(Game.Domain, "delayed_printer"),
                (_, s) => Console.WriteLine(s)
            );
        }
    }
}