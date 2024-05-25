using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;

namespace Mockiu.Hybrid
{
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class HarmonyMockSetup<T> : IMockSetup<T> where T : class
    {
        private readonly Harmony _harmonyInstance;
        private readonly Type _type;
        private T? _mockedInstance;

        public HarmonyMockSetup(Harmony harmonyInstance, Type type)
        {
            _harmonyInstance = harmonyInstance;
            _type = type;
        }

        public IMockSetup<T> Setup(Expression<Action<T>> setup, Action action)
        {
            var method = ((MethodCallExpression)setup.Body).Method;
            var patchProcessor = _harmonyInstance.CreateProcessor(method);
            patchProcessor.AddPrefix(
                new HarmonyMethod(
                    typeof(MethodReplacements).GetMethod(nameof(MethodReplacements.MethodReplacementVoid))));
            MethodReplacements.MockedMethods[method] = action;
            patchProcessor.Patch();
            return this;
        }

        public IMockSetup<T> Setup<TResult>(Expression<Func<T, TResult>> setup, Func<TResult> func)
        {
            var method = ((MethodCallExpression)setup.Body).Method;
            var patchProcessor = _harmonyInstance.CreateProcessor(method);
            patchProcessor.AddPrefix(
                new HarmonyMethod(typeof(MethodReplacements).GetMethod(nameof(MethodReplacements.MethodReplacement))));
            MethodReplacements.MockedMethods[method] = func;
            patchProcessor.Patch();
            return this;
        }

        public IMockSetup<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            var propInfo = ((MemberExpression)expression.Body).Member as PropertyInfo;
            if (propInfo != null)
            {
                var originalSetter = propInfo.GetSetMethod();
                if (originalSetter != null)
                {
                    var patchProcessor = _harmonyInstance.CreateProcessor(originalSetter);
                    patchProcessor.AddPrefix(new HarmonyMethod(
                        typeof(MethodReplacements).GetMethod(nameof(MethodReplacements.MethodReplacementVoid))));
                    MethodReplacements.MockedMethods[originalSetter] = new Action(() => { });
                    patchProcessor.Patch();
                    return this;
                }
            }

            throw new NotSupportedException("Property setup using Harmony is not supported for this property.");
        }

        public IMockSetup<T> SetupConstructor(Func<T> implementation)
        {
            var constructor = _type.GetConstructors().First();
            var patchProcessor = _harmonyInstance.CreateProcessor(constructor);
            patchProcessor.AddPrefix(new HarmonyMethod(
                typeof(HarmonyMockSetup<T>).GetMethod(nameof(ConstructorReplacement),
                    BindingFlags.Static | BindingFlags.NonPublic)));
            MethodReplacements.MockedMethods[constructor] = implementation;
            patchProcessor.Patch();
            _mockedInstance = implementation();
            return this;
        }

        public static bool ConstructorReplacement(MethodBase __originalMethod, object[] __args, ref object __instance)
        {
            if (!MethodReplacements.MockedMethods.TryGetValue(__originalMethod, out var implementation))
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
                    var fieldInfo = instanceType.GetField(field.Key,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(__instance, field.Value);
                    }
                    else
                    {
                        // Try to find an auto-generated backing field for properties
                        var backingFieldInfo = instanceType.GetField($"<{field.Key}>k__BackingField",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        if (backingFieldInfo != null)
                        {
                            backingFieldInfo.SetValue(__instance, field.Value);
                        }
                        else
                        {
                            // Try to find a property with the given name
                            var propertyInfo = instanceType.GetProperty(field.Key,
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            if (propertyInfo != null && propertyInfo.CanWrite)
                            {
                                propertyInfo.SetValue(__instance, field.Value);
                            }
                            else
                            {
                                throw new ArgumentException(
                                    $"Field or property {field.Key} not found on type {instanceType.FullName}");
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

        public T GetObject()
        {
            return _mockedInstance ??= (T)Activator.CreateInstance(_type);
        }
    }
}