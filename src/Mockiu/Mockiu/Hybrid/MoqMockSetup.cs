using Moq;
using System;
using System.Linq.Expressions;

namespace Mockiu.Hybrid
{
    public class MoqMockSetup<T> : IMockSetup<T> where T : class
    {
        private readonly Mock<T> _mock;

        public MoqMockSetup(Mock<T> mock)
        {
            _mock = mock;
        }

        public IMockSetup<T> Setup(Expression<Action<T>> setup, Action action)
        {
            _mock.Setup(setup).Callback(action);
            return this;
        }

        public IMockSetup<T> Setup<TResult>(Expression<Func<T, TResult>> setup, Func<TResult> func)
        {
            _mock.Setup(setup).Returns(func);
            return this;
        }

        public IMockSetup<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            _mock.SetupProperty(expression, value);
            return this;
        }

        public IMockSetup<T> Setup<T1, TResult>(Expression<Func<T, TResult>> setup, Func<T1, TResult> func)
        {
            _mock.Setup(setup).Returns((T1 arg1) => func(arg1));
            return this;
        }

        public T GetObject()
        {
            return _mock.Object;
        }
    }
}