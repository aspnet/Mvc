// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// This class represents a general purpose executor for a method(static/instance) on an object using MethodInfo.
    /// This executor is an alternative approach for MethodInfo.Invoke(). 
    /// Given the MethodInfo and the type of the target object, it creates a delegate and calls the method using it.
    /// The delegate is created dynamically using the Linq expressions. 
    /// The executor should be cached to get the maximum performance benefit or else compiling Linq expression adds overhead.
    /// </summary>
    public class ObjectMethodExecutor
    {
        private ActionExecutor _executor;

        public ObjectMethodExecutor(MethodInfo methodInfo, TypeInfo targetTypeInfo)
        {            
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (targetTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(targetTypeInfo));
            }

            _executor = GetExecutor(methodInfo, targetTypeInfo);
            MethodInfo = methodInfo;
        }

        private delegate object ActionExecutor(object target, object[] parameters);

        private delegate void VoidActionExecutor(object target, object[] parameters);

        public MethodInfo MethodInfo { get; private set; }

        /// <inheritdoc />
        public object Execute(object target, object[] parameters)
        {
            return _executor(target, parameters);
        }

        private static ActionExecutor GetExecutor(MethodInfo methodInfo, TypeInfo targetTypeInfo)
        {
            // Parameters to executor
            ParameterExpression targetParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // Build parameter list
            List<Expression> parameters = new List<Expression>();
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                ParameterInfo paramInfo = paramInfos[i];
                BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                // valueCast is "(Ti) parameters[i]"
                parameters.Add(valueCast);
            }

            // Call method
            UnaryExpression instanceCast = (!methodInfo.IsStatic) ? Expression.Convert(targetParameter, targetTypeInfo.AsType()) : null;
            MethodCallExpression methodCall = methodCall = Expression.Call(instanceCast, methodInfo, parameters);

            // methodCall is "((Ttarget) target) method((T0) parameters[0], (T1) parameters[1], ...)"
            // Create function
            if (methodCall.Type == typeof(void))
            {
                Expression<VoidActionExecutor> lambda = Expression.Lambda<VoidActionExecutor>(methodCall, targetParameter, parametersParameter);
                VoidActionExecutor voidExecutor = lambda.Compile();
                return WrapVoidAction(voidExecutor);
            }
            else
            {
                // must coerce methodCall to match ActionExecutor signature
                UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
                Expression<ActionExecutor> lambda = Expression.Lambda<ActionExecutor>(castMethodCall, targetParameter, parametersParameter);
                return lambda.Compile();
            }
        }

        private static ActionExecutor WrapVoidAction(VoidActionExecutor executor)
        {
            return delegate (object target, object[] parameters)
            {
                executor(target, parameters);
                return null;
            };
        }

    }
}
