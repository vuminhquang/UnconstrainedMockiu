using System;
using System.Collections.Concurrent;
using HarmonyLib;
using Moq;

namespace Mockiu.Hybrid
{
    public class HybridMockEngine : IDisposable
    {
        private readonly Harmony _harmony;
        private readonly string _instanceId;
        private readonly ConcurrentBag<IDisposable> _mockSetups = new ConcurrentBag<IDisposable>();

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