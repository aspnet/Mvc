// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.AspNet.Mvc.Rendering.Expressions
{
    public static class ExpressionCompiler
    {
        // This is the entry point to the expression compilation system. The system previously tried to turn the
        // expression into an actual delegate as quickly as possible, relying on cache lookups and other techniques to
        // save time if appropriate. For now, provide a few shortcuts but normally compile the expression as-is.
        public static Func<TModel, TValue> Process<TModel, TValue>([NotNull] Expression<Func<TModel, TValue>> expr)
        {
            return Compiler<TModel, TValue>.Compile(expr);
        }

        private static class Compiler<TIn, TOut>
        {
            private static Func<TIn, TOut> _identityFunc;

            public static Func<TIn, TOut> Compile([NotNull] Expression<Func<TIn, TOut>> expr)
            {
                return CompileFromIdentityFunc(expr)
                       ?? CompileFromConstLookup(expr)
                       ?? CompileFromMemberAccess(expr)
                       ?? CompileSlow(expr);
            }

            private static Func<TIn, TOut> CompileFromConstLookup([NotNull] Expression<Func<TIn, TOut>> expr)
            {
                var constExpr = expr.Body as ConstantExpression;
                if (constExpr != null)
                {
                    // model => {const}

                    var constantValue = (TOut)constExpr.Value;
                    return _ => constantValue;
                }

                return null;
            }

            private static Func<TIn, TOut> CompileFromIdentityFunc([NotNull] Expression<Func<TIn, TOut>> expr)
            {
                if (expr.Body == expr.Parameters[0])
                {
                    // model => model

                    // Don't need to lock, as all identity funcs are identical.
                    if (_identityFunc == null)
                    {
                        _identityFunc = expr.Compile();
                    }

                    return _identityFunc;
                }

                return null;
            }

            private static Func<TIn, TOut> CompileFromMemberAccess([NotNull] Expression<Func<TIn, TOut>> expr)
            {
                // Performance tests show that on the x64 platform, special-casing static member and
                // captured local variable accesses is faster than letting the fingerprinting system
                // handle them. On the x86 platform, the fingerprinting system is faster, but only
                // by around one microsecond, so it's not worth it to complicate the logic here with
                // an architecture check.

                var memberExpr = expr.Body as MemberExpression;
                if (memberExpr != null)
                {
                    if (memberExpr.Expression == expr.Parameters[0] || memberExpr.Expression == null)
                    {
                        // model => model.Member or model => StaticMember
                        return expr.Compile();
                    }

                    var constExpr = memberExpr.Expression as ConstantExpression;
                    if (constExpr != null)
                    {
                        // model => {const}.Member (captured local variable)
                        // rewrite as capturedLocal => ((TDeclaringType)capturedLocal).Member
                        var constParamExpr = Expression.Parameter(typeof(object), "capturedLocal");
                        var constCastExpr = Expression.Convert(constParamExpr, memberExpr.Member.DeclaringType);
                        var newMemberAccessExpr = memberExpr.Update(constCastExpr);
                        var newLambdaExpr = Expression.Lambda<Func<object, TOut>>(newMemberAccessExpr, constParamExpr);
                        var del = newLambdaExpr.Compile();

                        var capturedLocal = constExpr.Value;
                        return _ => del(capturedLocal);
                    }
                }

                return null;
            }

            private static Func<TIn, TOut> CompileSlow([NotNull] Expression<Func<TIn, TOut>> expr)
            {
                // fallback compilation system - just compile the expression directly
                return expr.Compile();
            }
        }
    }
}
