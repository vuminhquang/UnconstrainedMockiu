using System;
using System.Linq.Expressions;

namespace Mockiu.Hybrid
{
    using System;
    using System.Linq.Expressions;

    public interface IMockSetup<T> where T : class
    {
        IMockSetup<T> Setup(Expression<Action<T>> setup, Action action);
        IMockSetup<T> Setup<TResult>(Expression<Func<T, TResult>> setup, Func<TResult> func);
        IMockSetup<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value);
        T GetObject();
    }
}