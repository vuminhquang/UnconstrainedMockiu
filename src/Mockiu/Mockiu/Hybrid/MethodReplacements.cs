using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mockiu.Hybrid
{
    public static class MethodReplacements
    {
        public static readonly Dictionary<MethodBase, Delegate> MockedMethods = new Dictionary<MethodBase, Delegate>();

        public static bool MethodReplacementVoid(MethodBase __originalMethod, object[] __args)
        {
            if (!MockedMethods.TryGetValue(__originalMethod, out var implementation))
                return true; // Proceed to original method
            
            // var parameters = __originalMethod.IsStatic ? __args : new[] { __instance }.Concat(__args).ToArray();
            var parameters = __args;
            implementation.DynamicInvoke(parameters);
            
            return false;
        }

        public static bool MethodReplacement(MethodBase __originalMethod, ref object __result, object[] __args)
        {
            if (!MockedMethods.TryGetValue(__originalMethod, out var implementation)) return true;
            
            var parameters =  __args;
            __result = implementation.DynamicInvoke(parameters);
            
            return false;
        }
    }
}