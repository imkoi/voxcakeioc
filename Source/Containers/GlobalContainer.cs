using System;
using System.Collections.Generic;

namespace VoxCake.IoC
{
    internal static class GlobalContainer
    {
        internal static Dictionary<Type, Dependency> dependencies = new Dictionary<Type, Dependency>();
        internal static HashSet<Type> resolvedContainers = new HashSet<Type>();
    }
}