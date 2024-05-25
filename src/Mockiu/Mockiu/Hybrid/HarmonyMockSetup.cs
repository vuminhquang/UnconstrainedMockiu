using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Mockiu.Hybrid
{
    public class HarmonyMockSetup<T> : IMockSetup<T>, IDisposable where T : class
    {
        private readonly Harmony _harmonyInstance;
        private readonly Type _type;
        private T? _mockedInstance;
        private readonly List<MethodBase> _patchedMethods = new List<MethodBase>();

        public HarmonyMockSetup(Harmony harmonyInstance, Type type)
        {
            _harmonyInstance = harmonyInstance;
            _type = type;
        }

        public IMockSetup<T> Setup(Expression<Action<T>> setup, Action action)
        {
            var method = ((MethodCallExpression)setup.Body).Method;
            PatchMethod(method, action, nameof(MethodReplacements.MethodReplacementVoid));
            return this;
        }

        public IMockSetup<T> Setup<TResult>(Expression<Func<T, TResult>> setup, Func<TResult> func)
        {
            var method = ((MethodCallExpression)setup.Body).Method;
            PatchMethod(method, func, nameof(MethodReplacements.MethodReplacement));
            return this;
        }

        public IMockSetup<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            var propInfo = ((MemberExpression)expression.Body).Member as PropertyInfo;
            if (propInfo == null)
                throw new NotSupportedException("Property setup using Harmony is not supported for this property.");
            var originalSetter = propInfo.GetSetMethod();
            if (originalSetter == null)
                throw new NotSupportedException("Property setup using Harmony is not supported for this property.");
            PatchMethod(originalSetter, new Action(() => { }), nameof(MethodReplacements.MethodReplacementVoid));
            return this;
        }

        private void PatchMethod(MethodBase method, Delegate implementation, string replacementMethodName, BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public)
        {
            var patchProcessor = _harmonyInstance.CreateProcessor(method);
            patchProcessor.AddPrefix(new HarmonyMethod(typeof(MethodReplacements).GetMethod(replacementMethodName, bindingFlags)));

            var harmonyDict = MethodReplacements.MockedMethods.GetOrAdd(method, _ => new ConcurrentDictionary<string, Delegate>());
            harmonyDict[_harmonyInstance.Id] = implementation;

            patchProcessor.Patch();
            _patchedMethods.Add(method);
        }
        
        public T GetObject()
        {
            return _mockedInstance ??= (T)Activator.CreateInstance(_type);
        }

        public void Dispose()
        {
            foreach (var method in _patchedMethods)
            {
                _harmonyInstance.Unpatch(method, HarmonyPatchType.All, _harmonyInstance.Id);

                if (!MethodReplacements.MockedMethods.TryGetValue(method, out var harmonyDict)) continue;
                harmonyDict.TryRemove(_harmonyInstance.Id, out _);
                if (harmonyDict.IsEmpty)
                {
                    MethodReplacements.MockedMethods.TryRemove(method, out _);
                }
            }
            _patchedMethods.Clear();
        }
    }
}