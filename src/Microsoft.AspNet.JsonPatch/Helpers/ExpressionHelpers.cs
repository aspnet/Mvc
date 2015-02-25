using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
	internal static class ExpressionHelpers
	{

		public static string GetPath<T, TProp>(Expression<Func<T, TProp>> expr) where T : class
		{
			return "/" + GetPath(expr.Body, true);
		}


		// from Ramone helpers.
		private static string GetPath(Expression expr, bool firstTime)
		{
			if (expr.NodeType == ExpressionType.MemberAccess)
			{
				MemberExpression m = expr as MemberExpression;
				string left = GetPath(m.Expression, false);
				if (left != null)
					return left + "/" + m.Member.Name;
				else
					return m.Member.Name;
			}
			else if (expr.NodeType == ExpressionType.Call)
			{
				MethodCallExpression m = (MethodCallExpression)expr;
				string left = GetPath(m.Object, false);
				if (left != null)
					return left + "/" + GetIndexerInvocation(m.Arguments[0]);
				else
					return GetIndexerInvocation(m.Arguments[0]);
			}
			else if (expr.NodeType == ExpressionType.ArrayIndex)
			{
				BinaryExpression b = (BinaryExpression)expr;
				string left = GetPath(b.Left, false);
				if (left != null)
					return left + "/" + b.Right.ToString();
				else
					return b.Right.ToString();
			}
			else if (expr.NodeType == ExpressionType.Parameter)
			{
				// Fits "x => x" (the whole document which is "" as JSON pointer)
				return firstTime ? "" : null;
			}
			else if (expr.NodeType == ExpressionType.Convert)
			{
				// Ignore conversions
				return GetPath(((UnaryExpression)expr).Operand, false);
			}
			else
				return null;
		}


		private static string GetIndexerInvocation(Expression expression)
		{
			Expression converted = Expression.Convert(expression, typeof(object));
			ParameterExpression fakeParameter = Expression.Parameter(typeof(object), null);
			Expression<Func<object, object>> lambda = Expression.Lambda<Func<object, object>>(converted, fakeParameter);
			Func<object, object> func;

			func = lambda.Compile();

			return Convert.ToString(func(null), CultureInfo.InvariantCulture);
		}

	}
}