using System;
using HarmonyLib;
using Moq;

namespace Mockiu.Hybrid
{
    public class HybridMockEngine : IDisposable
    {
        private readonly Harmony _harmony;
        private readonly string _instanceId;

        public HybridMockEngine(string instanceId)
        {
            _instanceId = instanceId;
            _harmony = new Harmony(instanceId);
        }

        public IMockSetup<T> Mock<T>() where T : class
        {
            if (typeof(T).IsInterface || typeof(T).IsAbstract)
            {
                var mock = new Mock<T>();
                return new MoqMockSetup<T>(mock);
            }
            else
            {
                return new HarmonyMockSetup<T>(_harmony, typeof(T));
            }
        }

        public void Dispose()
        {
            _harmony.UnpatchAll(_instanceId);
        }
    }
}