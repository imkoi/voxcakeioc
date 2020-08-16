using System;
using System.Collections.Generic;

namespace VoxCake.IoC
{
    internal static class GlobalContainer
    {
        internal static Dictionary<Type, object> dependencies = new Dictionary<Type, object>();
        internal static HashSet<Type> resolvedContainers = new HashSet<Type>();
    }
}