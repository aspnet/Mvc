// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ObjectMethodExecutor
    {
        private TaskOfTActionExecutorAsync _taskOfTexecutorAsync;
        private ActionExecutor _executor;
        private Type _taskInnerTypeForMethodReturnType;

        private static readonly MethodInfo _convertOfTMethod =
            typeof(ObjectMethodExecutor).GetRuntimeMethods().Single(methodInfo => methodInfo.Name == nameof(ObjectMethodExecutor.Convert));

        private ObjectMethodExecutor(MethodInfo methodInfo, TypeInfo targetTypeInfo)
        {            
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (targetTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(targetTypeInfo));
            }

            MethodInfo = methodInfo;
            TargetTypeInfo = targetTypeInfo;
        }

        private delegate Task<object> TaskOfTActionExecutorAsync(object target, object[] parameters);

        private delegate object ActionExecutor(object target, object[] parameters);

        private delegate void VoidActionExecutor(object target, object[] parameters);

        public MethodInfo MethodInfo { get; }

        public TypeInfo TargetTypeInfo { get; set; }

        public bool CanExecuteInSync { get; set; }

        public Type TaskInnerType
        {
            get
            {
                if (_taskInnerTypeForMethodReturnType == null)
                {
                    var returnType = MethodInfo.ReturnType;
                    _taskInnerTypeForMethodReturnType = GetTaskInnerTypeOrNull(returnType);
                }

                return _taskInnerTypeForMethodReturnType;
            }
        }

        private TaskOfTActionExecutorAsync TaskOfTActionExecutoAsync
        {
            get
            {
                if (_taskOfTexecutorAsync == null)
                {
                    _taskOfTexecutorAsync = GetTaskOfTExecutorAsync(TaskInnerType, MethodInfo, TargetTypeInfo);
                }

                return _taskOfTexecutorAsync;
            }
        }

        public static ObjectMethodExecutor Create(MethodInfo methodInfo, TypeInfo targetTypeInfo)
        {
            var executor = new ObjectMethodExecutor(methodInfo, targetTypeInfo);
            executor.CanExecuteInSync = typeof(Task) == methodInfo.ReturnType || 
                !typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
            executor._executor = GetExecutor(methodInfo, targetTypeInfo);
            return executor;
        }

        public Task<object> ExecuteAsync(object target, object[] parameters)
        {
            return TaskOfTActionExecutoAsync(target, parameters);
        }

        public object Execute(object target, object[] parameters)
        {
            return _executor(target, parameters);
        }

        private static ActionExecutor GetExecutor(MethodInfo methodInfo, TypeInfo targetTypeInfo)
        {
            // Parameters to executor
            var targetParameter = Expression.Parameter(typeof(object), "target");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // Build parameter list
            var parameters = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i];
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                // valueCast is "(Ti) parameters[i]"
                parameters.Add(valueCast);
            }

            // Call method
            var instanceCast = Expression.Convert(targetParameter, targetTypeInfo.AsType());
            var methodCall = Expression.Call(instanceCast, methodInfo, parameters);

            // methodCall is "((Ttarget) target) method((T0) parameters[0], (T1) parameters[1], ...)"
            // Create function
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<VoidActionExecutor>(methodCall, targetParameter, parametersParameter);
                var voidExecutor = lambda.Compile();
                return WrapVoidAction(voidExecutor);
            }
            else
            {
                // must coerce methodCall to match ActionExecutor signature
                var castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<ActionExecutor>(castMethodCall, targetParameter, parametersParameter);
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

        private static TaskOfTActionExecutorAsync GetTaskOfTExecutorAsync(Type taskInnerType, MethodInfo methodInfo, TypeInfo targetTypeInfo)
        {
            if (taskInnerType == null)
            {
                // This will be the case for types which have derived from Task and Task<T>
                throw new InvalidOperationException(Resources.FormatActionExecutor_UnexpectedTaskInstance(
                    methodInfo.Name,
                    methodInfo.DeclaringType));
            }

            // Parameters to executor
            var targetParameter = Expression.Parameter(typeof(object), "target");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // Build parameter list
            var parameters = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i];
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                // valueCast is "(Ti) parameters[i]"
                parameters.Add(valueCast);
            }

            // Call method
            var instanceCast = Expression.Convert(targetParameter, targetTypeInfo.AsType());
            var methodCall = Expression.Call(instanceCast, methodInfo, parameters);

            var coerceMethodCall = GetCoerceMethodCallExpression(taskInnerType, methodCall, methodInfo);
            var lambda = Expression.Lambda<TaskOfTActionExecutorAsync>(coerceMethodCall, targetParameter, parametersParameter);
            return lambda.Compile();
        }

        // We need to CoerceResult as the object value returned from methodInfo.Invoke has to be cast to a Task<T>.
        // This is necessary to enable calling await on the returned task.
        // i.e we need to write the following var result = await (Task<ActualType>)mInfo.Invoke.
        // Returning Task<object> enables us to await on the result.
        private static Expression GetCoerceMethodCallExpression(
            Type taskValueType, 
            MethodCallExpression methodCall, 
            MethodInfo methodInfo)
        {
            var castMethodCall = Expression.Convert(methodCall, typeof(object));
            // for: public Task<T> Action()
            // constructs: return (Task<object>)Convert<T>((Task<T>)result)
            var genericMethodInfo = _convertOfTMethod.MakeGenericMethod(taskValueType);
            var genericMethodCall = Expression.Call(null, genericMethodInfo, castMethodCall);
            var convertedResult = Expression.Convert(genericMethodCall, typeof(Task<object>));
            return convertedResult;
        }

        /// <summary>
        /// Cast Task of T to Task of object
        /// </summary>
        private static async Task<object> CastToObject<T>(Task<T> task)
        {
            return (object)await task;
        }

        private static Type GetTaskInnerTypeOrNull(Type type)
        {
            var genericType = ClosedGenericMatcher.ExtractGenericInterface(type, typeof(Task<>));

            return genericType?.GenericTypeArguments[0];
        }

        private static Task<object> Convert<T>(object taskAsObject)
        {
            var task = (Task<T>)taskAsObject;
            return CastToObject<T>(task);
        }
    }
}
