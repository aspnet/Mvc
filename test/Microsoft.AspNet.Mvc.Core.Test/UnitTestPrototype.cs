// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.Http.Core.Collections;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
	public class UnitTestabilityTestPrototype
	{
		[Fact]
		public void ControllerTestability_UsingContextRequestAbortedTest()
		{
			// Arrange
			var controller = new TestabilityController();

			var callback = new MvcControllerUnitTestHelperCallback()
			{
				OnRquestAborted = () => new CancellationToken(true)
			};

			// Act
			new MvcControllerUnitTestHelper().Initialize(controller, callback);
			var viewResult = controller.UsingContextRequestAborted();

			// Assert
			Assert.Equal("RequestAborted", viewResult.ViewName);

			// Arrange
			callback = new MvcControllerUnitTestHelperCallback()
			{
				OnRquestAborted = () => new CancellationToken(false)
			};

			// Act
			new MvcControllerUnitTestHelper().Initialize(controller, callback);
			viewResult = controller.UsingContextRequestAborted();

			// Assert
			Assert.Equal("Request", viewResult.ViewName);

		}

		[Fact]
		public void ControllerTestability_UsingModelStateTest()
		{
			// Arrange
			var controller = new TestabilityController();
			var model = new MyModel()
			{
				Property1 = "ValidModel",
				Property2 = "InvalidModel"
			};

			// Act
			new MvcControllerUnitTestHelper().Initialize(controller, null);
			controller.ModelState.AddModelError("", "error_1");
			var result = controller.UsngModelState(model);

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Equal(model.Property2, viewResult.ViewName);
			Assert.NotNull(viewResult.ViewData.Model);
			Assert.Equal(1, controller.ModelState.ErrorCount);
		}

		[Fact]
		public void ControllerTestability_UsingFormCollectionTest()
		{
			// Arrange
			var controller = new TestabilityController();
			var formData = new Dictionary<string, string[]>() { { "formKey1", new string[] { "formKey1Value" } } };
			var formCollection = new FormCollection(formData);

			var callback = new MvcControllerUnitTestHelperCallback()
			{
				OnRequestFormCollection = () => formCollection
			};

			// Act
			new MvcControllerUnitTestHelper().Initialize(controller, callback);
			var result = controller.UsingContextFormCollection("formKey1");

			// Assert
			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Equal(viewResult.ViewName, "formKey1Value");
		}

		[Fact]
		public void ControllerTestability_UsingActionTest()
		{
			// Arrange
			var controller = new TestabilityController();
			var testHelper = new MvcControllerUnitTestHelper();


			var callback = new MvcControllerUnitTestHelperCallback()
			{
				OnUrlAction = (action) =>
				{
					Assert.Equal("redirect", action);
					return action;
				}
			};

			// Act && Assert
			testHelper.Initialize(controller, callback);
			var result = controller.UsingUrlAction("redirect");

		}

		private class MyModel
		{
			[Required]
			public string Property1
			{
				get; set;
			}

			[StringLength(3)]
			public string Property2
			{
				get;
				set;
			}
		}

		private class TestabilityController : Controller
		{
			public ViewResult UsingViewBagAction()
			{
				ViewBag.StatusMessage = "Hello World";
				return View();
			}

			public ViewResult UsingContextRequestAborted()
			{
				if (Context.RequestAborted.IsCancellationRequested)
				{
					return View("RequestAborted");
				}
				return View("Request");
			}

			public IActionResult UsngModelState(MyModel model)
			{
				if (ModelState.IsValid)
				{
					return View(model.Property1);
				}

				return View(model.Property2, model);
			}

			public IActionResult UsingContextFormCollection(string formKey)
			{
				var formCollection = Context.Request.ReadFormAsync();
				var viewName = formCollection.Result.GetValues(formKey).FirstOrDefault();

				return View(viewName);
			}

			public IActionResult UsingUrlAction(string action)
			{
				return View(Url.Action(action));
			}
		}
	}
}