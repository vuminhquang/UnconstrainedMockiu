using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Mockiu.Hybrid
{
    public static class MethodReplacements
    {
        public static readonly ConcurrentDictionary<MethodBase, ConcurrentDictionary<string, Delegate>> MockedMethods 
            = new ConcurrentDictionary<MethodBase, ConcurrentDictionary<string, Delegate>>();

        public static bool MethodReplacementVoid(MethodBase __originalMethod, object[] __args)
        {
            if (!MockedMethods.TryGetValue(__originalMethod, out var harmonyDict))
                return true; // Proceed to original method

            foreach (var implementation in harmonyDict.Values)
            {
                var parameters = __args;
                implementation.DynamicInvoke(parameters);
            }

            return false;
        }

        public static bool MethodReplacement(MethodBase __originalMethod, ref object __result, object[] __args)
        {
            if (!MockedMethods.TryGetValue(__originalMethod, out var harmonyDict))
                return true; // Proceed to original method

            foreach (var implementation in harmonyDict.Values)
            {
                var parameters = __args;
                __result = implementation.DynamicInvoke(parameters);
            }

            return false;
        }
    }
}