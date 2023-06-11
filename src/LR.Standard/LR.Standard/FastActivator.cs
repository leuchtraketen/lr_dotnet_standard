using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LR.Standard;

public static class FastActivator
{
#if XAMARIN_IOS
        public static T CreateInstance<T> () where T : new ()
        {
            return (T) Activator.CreateInstance (typeof (T));
        }

#else

    public static T CreateInstance<T>() where T : new()
    {
        return FastActivatorImpl<T>.NewFunction();
    }

    private static class FastActivatorImpl<T> where T : new()
    {
        // Compiler translates 'new T()' into Expression.New()
        private static readonly Expression<Func<T>> NewExpression = () => new T();

        // Compiling expression into the delegate
        public static readonly Func<T> NewFunction = NewExpression.Compile();
    }

#endif
}
