using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Mockiu
{
    public class MockEngine : IDisposable
    {
        private readonly Harmony _harmony;
        private bool _disposed = false;

        public MockEngine(string id)
        {
            _harmony = new Harmony(id);
        }

        public void Mock<T>(string methodName, Delegate implementation)
        {
            var method = typeof(T).GetMethod(methodName);
            if (method == null) throw new ArgumentException($"Method {methodName} not found on type {typeof(T).FullName}");

            MockMethod(method, implementation);
        }

        public void Mock<T>(string propertyName, Func<T,object>? getterImplementation = null, Action<T,object>? setterImplementation = null)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null) throw new ArgumentException($"Property {propertyName} not found on type {typeof(T).FullName}");

            if (getterImplementation != null)
            {
                var getter = property.GetGetMethod();
                if (getter != null)
                {
                    MockMethod(getter, getterImplementation);
                }
            }

            if (setterImplementation != null)
            {
                var setter = property.GetSetMethod();
                if (setter != null)
                {
                    MockMethod(setter, setterImplementation);
                }
            }
        }

        public void MockConstructor<T>(Delegate implementation)
        {
            var constructors = typeof(T).GetConstructors();
            foreach (var constructor in constructors)
            {
                MockMethod(constructor, implementation);
            }
        }
        
        public void MockGeneric<T>(string methodName, Type[] typeArguments, Delegate implementation)
        {
            var methods = typeof(T).GetMethods().Where(m => m.Name == methodName && m.IsGenericMethodDefinition);
            var method = methods.FirstOrDefault(m => m.GetGenericArguments().Length == typeArguments.Length);

            if (method == null) throw new ArgumentException($"Generic method {methodName} not found on type {typeof(T).FullName}");

            var constructedMethod = method.MakeGenericMethod(typeArguments);
            MockMethod(constructedMethod, implementation);
        }
        
        private void MockMethod(MethodBase method, Delegate implementation)
        {
            MethodReplacements.MockedMethods[method] = implementation;

            HarmonyMethod patchPrefix;
            if (method is ConstructorInfo)
            {
                patchPrefix = new HarmonyMethod(typeof(MethodReplacements).GetMethod(
                    nameof(MethodReplacements.ConstructorReplacement),
                    BindingFlags.Public | BindingFlags.Static));
            }
            else
            {
                patchPrefix = new HarmonyMethod(typeof(MethodReplacements).GetMethod(
                    method is MethodInfo mi && mi.ReturnType == typeof(void) ? nameof(MethodReplacements.MethodReplacementVoid) : nameof(MethodReplacements.MethodReplacement),
                    BindingFlags.Public | BindingFlags.Static));
            }

            _harmony.Patch(method, prefix: patchPrefix);
        }
        
        
        public void ClearAll()
        {
            _harmony.UnpatchAll(_harmony.Id);
            MethodReplacements.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            ClearAll();
            _disposed = true;
        }

        private static class MethodReplacements
        {
            public static readonly Dictionary<MethodBase, Delegate> MockedMethods = new Dictionary<MethodBase, Delegate>();

            public static void Clear()
            {
                MockedMethods.Clear();
            }

            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public static bool MethodReplacement(MethodBase __originalMethod, object __instance, object[] __args, ref object __result)
            {
                if (!MockedMethods.TryGetValue(__originalMethod, out var implementation))
                    return true; // Proceed to original method

                var parameters = __originalMethod.IsStatic ? __args : new[] { __instance }.Concat(__args).ToArray();
                __result = implementation.DynamicInvoke(parameters);
                return false; // Skip original method
            }
            
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public static bool MethodReplacementVoid(MethodBase __originalMethod, object __instance, object[] __args)
            {
                if (!MockedMethods.TryGetValue(__originalMethod, out var implementation))
                    return true; // Proceed to original method

                try
                {
                    var parameters = __originalMethod.IsStatic ? __args : new[] { __instance }.Concat(__args).ToArray();
                    implementation.DynamicInvoke(parameters);
                    return false; // Skip original method
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking delegate: {ex}");
                    throw;
                }
            }
            
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public static bool ConstructorReplacement(MethodBase __originalMethod, object[] __args, ref object __instance)
            {
                if (!MockedMethods.TryGetValue(__originalMethod, out var implementation))
                    return true; // Proceed to original constructor

                try
                {
                    var fieldValues = new Dictionary<string, object>();
                    implementation.DynamicInvoke(__instance, fieldValues, __args);

                    // Use reflection to set the fields and properties with the provided values
                    var instanceType = __instance.GetType();
                    foreach (var field in fieldValues)
                    {
                        // Try to find a field with the exact name
                        var fieldInfo = instanceType.GetField(field.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(__instance, field.Value);
                        }
                        else
                        {
                            // Try to find an auto-generated backing field for properties
                            var backingFieldInfo = instanceType.GetField($"<{field.Key}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (backingFieldInfo != null)
                            {
                                backingFieldInfo.SetValue(__instance, field.Value);
                            }
                            else
                            {
                                // Try to find a property with the given name
                                var propertyInfo = instanceType.GetProperty(field.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                if (propertyInfo != null && propertyInfo.CanWrite)
                                {
                                    propertyInfo.SetValue(__instance, field.Value);
                                }
                                else
                                {
                                    throw new ArgumentException($"Field or property {field.Key} not found on type {instanceType.FullName}");
                                }
                            }
                        }
                    }

                    return false; // Skip original constructor
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error invoking delegate: {ex}");
                    throw;
                }
            }
            
            // public static bool ConstructorReplacement(MethodBase __originalMethod, object[] __args, ref object __instance)
            // {
            //     if (!MockedMethods.TryGetValue(__originalMethod, out var implementation))
            //         return true; // Proceed to original constructor
            //
            //     var parameters = new[]{__instance}.Concat(__args).ToArray();
            //     
            //     var tempFile = "abc.txt";
            //     
            //     // create the string builder to store the text before writing it to the file
            //     var sb = new System.Text.StringBuilder();
            //     sb.AppendLine($"Mocked method: {__originalMethod.DeclaringType.FullName}.{__originalMethod.Name}");
            //     // the original method parameters count
            //     sb.AppendLine($"Original method parameters count: {parameters.Length}");
            //     // add the parameters to the string builder
            //     foreach (var parameter in parameters)
            //     {
            //         sb.AppendLine(parameter.ToString());
            //     }
            //     
            //     // add $"Expected delegate parameters count: {implementation.Method.GetParameters().Length}"
            //     sb.AppendLine($"Implementation delegate parameters count: {implementation.Method.GetParameters().Length}");
            //     // list the parameters of the delegate
            //     foreach (var parameter in implementation.Method.GetParameters())
            //     {
            //         // add a line with name and value
            //         var value = parameters[parameter.Position];
            //         var printLine = $"{parameter.Name}: {value}";
            //         sb.AppendLine(printLine);
            //     }
            //     
            //     // write the string builder to the file
            //     System.IO.File.WriteAllText(tempFile, sb.ToString());
            //     
            //     try
            //     {
            //         implementation.DynamicInvoke(parameters);
            //         return false; // Skip original constructor, the instance is already created
            //     }
            //     catch (Exception ex)
            //     {
            //         Console.WriteLine($"Error invoking delegate: {ex}");
            //         throw;
            //     }
            // }
        }
    }
}