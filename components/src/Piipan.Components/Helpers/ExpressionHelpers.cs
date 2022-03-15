using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Piipan.Components.Helpers
{
    public static class ExpressionHelpers
    {
        private static T GetAttribute<T>(this ICustomAttributeProvider provider)
            where T : Attribute
        {
            var attributes = provider.GetCustomAttributes(typeof(T), true);
            return attributes.Length > 0 ? attributes[0] as T : null;
        }

        public static A GetAttribute<T, A>(this Expression<Func<T>> expression) where A : Attribute
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new InvalidOperationException("Expression must be a member expression");

            return memberExpression.Member.GetAttribute<A>();
        }

        public static bool HasAttribute<T, A>(this Expression<Func<T>> expression) where A : Attribute
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new InvalidOperationException("Expression must be a member expression");

            return memberExpression.Member.GetAttribute<A>() != null;
        }
    }
}
