using System;
using System.Linq.Expressions;

namespace Mockiu.Hybrid
{
    public interface IMockSetup<T> where T : class
    {
        IMockSetup<T> Setup(Expression<Action<T>> setup, Action action);
        IMockSetup<T> Setup<TResult>(Expression<Func<T, TResult>> setup, Func<TResult> func);
        IMockSetup<T> Setup<T1, TResult>(Expression<Func<T, TResult>> setup, Func<T1, TResult> func);
        IMockSetup<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value);
        T GetObject();
    }
}