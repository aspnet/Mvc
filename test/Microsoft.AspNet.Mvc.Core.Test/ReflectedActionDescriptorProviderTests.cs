// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ReflectedActionDescriptorProviderTests
    {
        private DefaultActionDiscoveryConventions _actionDiscoveryConventions =
            new DefaultActionDiscoveryConventions();
        private IControllerDescriptorFactory _controllerDescriptorFactory = new DefaultControllerDescriptorFactory();
        private IParameterDescriptorFactory _parameterDescriptorFactory = new DefaultParameterDescriptorFactory();
        private readonly IEnumerable<string> _validActionNamesFromDerivedController;

        public ReflectedActionDescriptorProviderTests()
        {
            _validActionNamesFromDerivedController =
                GetDescriptors(typeof(DerivedController).GetTypeInfo()).Select(a => a.Name);
        }

        public static IEnumerable<string[]> MethodsFromObjectClass
        {
            get
            {
                return typeof(object).GetMethods().Select(m => new string[] { m.Name });
            }
        }

        [Fact]
        public void GetDescriptors_GetsDescriptorsForActionsInBaseAndDerivedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.Equal(3, actionNames.Count());
            Assert.True(actionNames.Contains("GetFromDerived"));
            Assert.True(actionNames.Contains("GetFromBase"));
            Assert.True(actionNames.Contains("NewMethod")); // Public method declared with keyword "new".
        }

        [Fact]
        public void GetDescriptors_Ignores_OverridenRedirect_FromControllerClass()
        {
            // Arrange & Act
            var actionNames = GetDescriptors(typeof(BaseController).GetTypeInfo()).Select(a => a.Name);

            // Assert
            Assert.False(actionNames.Contains("Redirect"));
        }

        [Fact]
        public void GetDescriptors_Ignores_PrivateMethod_FromUserDefinedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains("PrivateMethod"));
        }

        [Fact]
        public void GetDescriptors_Ignores_Constructor_FromUserDefinedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains("DerivedController"));
        }

        [Fact]
        public void GetDescriptors_Ignores_OperatorOverloadingMethod_FromUserDefinedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains("op_Addition"));
        }

        [Fact]
        public void GetDescriptors_Ignores_GenericMethod_FromUserDefinedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains("GenericMethod"));
        }

        [Fact]
        public void GetDescriptors_Ignores_StaticMethod_FromUserDefinedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains("StaticMethod"));
        }

        [Fact]
        public void GetDescriptors_Ignores_OverridenNonActionMethod_FromDerivedController()
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains("OverridenNonActionMethod"));
        }

        [Theory]
        [MemberData("MethodsFromObjectClass")]
        public void GetDescriptors_Ignores_MethodsFromObjectClass_FromUserDefinedController(string methodName)
        {
            // Arrange & Act
            var actionNames = _validActionNamesFromDerivedController;

            // Assert
            Assert.False(actionNames.Contains(methodName));
        }

        private IEnumerable<ReflectedActionDescriptor> GetDescriptors(TypeInfo controllerTypeInfo)
        {
            var provider = new ReflectedActionDescriptorProvider(null,
                _actionDiscoveryConventions,
                _controllerDescriptorFactory,
                _parameterDescriptorFactory,
                null);
            var testControllers = new TypeInfo[]
            {
                controllerTypeInfo,
            };
            var controllerDescriptors = testControllers
                .Select(t => _controllerDescriptorFactory.CreateControllerDescriptor(t));
            return provider.GetDescriptors(controllerDescriptors).Cast<ReflectedActionDescriptor>();
        }

        #region Controller Classes

        private class DerivedController : BaseController
        {
            public void GetFromDerived() // Valid action method.
            { }

            [HttpGet]
            public override void OverridenNonActionMethod()
            { }

            public new void NewMethod() // Valid action method.
            { }

            public void GenericMethod<T>()
            { }

            public static void StaticMethod()
            { }

            public static DerivedController operator +(DerivedController c1, DerivedController c2)
            {
                return new DerivedController();
            }

            private void PrivateMethod()
            { }
        }

        private class BaseController : Controller
        {
            public void GetFromBase() // Valid action method.
            { }

            [NonAction]
            public virtual void OverridenNonActionMethod()
            { }

            [NonAction]
            public virtual void NewMethod()
            { }

            public override RedirectResult Redirect(string url)
            {
                return base.Redirect(url + "#RedirectOverride");
            }
        }

        #endregion Controller Classes
    }
}
