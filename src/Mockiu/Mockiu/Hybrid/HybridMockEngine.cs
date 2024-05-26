using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Moq;

namespace Mockiu.Hybrid
{
    public class HybridMockEngine : IDisposable
    {
        private readonly Harmony _harmony;
        private readonly string _instanceId;
        private readonly ConcurrentBag<IDisposable> _mockSetups = new ConcurrentBag<IDisposable>();
        private readonly Dictionary<Type, HarmonyMockSetup<object>> _harmonySetups = new Dictionary<Type, HarmonyMockSetup<object>>();

        public HybridMockEngine(string instanceId)
        {
            _instanceId = instanceId;
            _harmony = new Harmony(instanceId);
        }

        public IMockSetup<T> Mock<T>(IMockSetup<T>? preferredMockSetup = null) where T : class
        {
            if (preferredMockSetup != null)
            {
                if (!(preferredMockSetup is MoqMockSetup<T>))
                {
                    _mockSetups.Add((preferredMockSetup as IDisposable)!);
                }
                return preferredMockSetup;
            }

            if (!typeof(T).IsInterface && !typeof(T).IsAbstract)
            {
                var setup = new HarmonyMockSetup<T>(_harmony, typeof(T));
                _mockSetups.Add(setup);
                return setup;
            }

            var mock = new Mock<T>();
            return new MoqMockSetup<T>(mock);
        }

        public void SetupStaticMethod(Type type, string methodName, Delegate implementation)
        {
            var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
                throw new ArgumentException($"The method '{methodName}' could not be found in type '{type.FullName}'.");

            if (!_harmonySetups.TryGetValue(type, out var setup))
            {
                setup = new HarmonyMockSetup<object>(_harmony, type);
                _harmonySetups[type] = setup;
                _mockSetups.Add(setup);
            }

            setup.SetupStaticMethod(methodInfo, implementation);
        }

        public void Dispose()
        {
            foreach (var mockSetup in _mockSetups)
            {
                mockSetup.Dispose();
            }
            _harmony.UnpatchAll(_instanceId);
        }
    }
}