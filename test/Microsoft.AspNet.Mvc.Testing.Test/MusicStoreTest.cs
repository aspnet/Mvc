using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core.Security;
using Microsoft.AspNet.Http.Interfaces.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;
using Moq;
using MusicStore.Controllers;
using MusicStore.Models;
using Xunit;

namespace Microsoft.AspNet.Mvc.Testing
{
    public class MusicStoreTest
    {
        [Fact]
        public void AccountController_VerifyCodeTest()
        {
            // Arrange
            var helperContext = new ControllerTestHelperContext();

            var userManager = CreateMockUserManager<ApplicationUser>();
            var signInManager = CreateMockSignInManager<ApplicationUser>
                (userManager.Object, helperContext.HttpContext);

            var controller = new AccountController(userManager.Object, signInManager.Object);
            controller.ModelState.AddModelError("", "validation error");

            ControllerTestHelper.Initialize(controller, helperContext);

            var model = new VerifyCodeViewModel()
            {
                Provider = "provider",
                Code = "Undefined",
            };

            // Act
            var result = controller.VerifyCode(model).Result;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.ViewData.Model);


            // Arrange
            model.Code = "Success";
            model.ReturnUrl = "/ReturnToMe";
            controller = new AccountController(userManager.Object, signInManager.Object);
            ControllerTestHelper.Initialize(controller, helperContext);

            // Act
            result = controller.VerifyCode(model).Result;

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(model.ReturnUrl, redirectResult.Url);

            // Arrange
            helperContext = new ControllerTestHelperContext();
            helperContext.RequestAborted = true;

            signInManager = CreateMockSignInManager<ApplicationUser>
                (userManager.Object, helperContext.HttpContext);

            controller = new AccountController(userManager.Object, signInManager.Object);
            ControllerTestHelper.Initialize(controller, helperContext);

            // Act
            Assert.ThrowsAsync<OperationCanceledException>(() => controller.VerifyCode(model));
        }

        [Fact]
        public void AccountController_LogOffTest()
        {
            // Arrange
            var helperContext = new ControllerTestHelperContext();

            var userManager = CreateMockUserManager<ApplicationUser>();
            var signInManager = CreateMockSignInManager<ApplicationUser>
                (userManager.Object, helperContext.HttpContext);

            var authHandler = new Mock<IAuthenticationHandler>();
            authHandler.Setup(p => p.SignOut(It.IsAny<ISignOutContext>()))
                .Callback<ISignOutContext>((signoutContext) =>
                    {
                        Assert.Contains("OpenIdConnect", signoutContext.AuthenticationTypes);
                        var acceptedList = (List<string>)((SignOutContext)signoutContext).Accepted;
                        acceptedList.AddRange(signoutContext.AuthenticationTypes);
                    }).Verifiable();


            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.SetupGet(n => n.EnvironmentName).Returns("OpenIdConnect");
            helperContext.RequestServices.AddSingleton<IHostingEnvironment>(n => hostingEnvironment.Object);

            var controller = new AccountController(userManager.Object, signInManager.Object);
            helperContext.ResponseAuthenticationHandler = authHandler.Object;
            ControllerTestHelper.Initialize(controller, helperContext);

            //Act
            controller.LogOff();

            // Assert
            authHandler.Verify();
        }

        [Fact]
        public void AccountController_Register()
        {
            // Arrange
            var helperContext = new ControllerTestHelperContext();
            var userManager = CreateMockUserManager<ApplicationUser>();
            var signInManager = CreateMockSignInManager<ApplicationUser>
                (userManager.Object, helperContext.HttpContext);
            var controller = new AccountController(userManager.Object, signInManager.Object);
            controller.ModelState.AddModelError("", "modelerror");

            // Act
            var result = controller.Register(new RegisterViewModel()).Result;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);

            // Arrange
            var model = new RegisterViewModel() { Password = "Fail" };
            controller = new AccountController(userManager.Object, signInManager.Object);
            helperContext.RequestAborted = false;
            ControllerTestHelper.Initialize(controller, helperContext);

            // Act
            result = controller.Register(model).Result;

            // Assert
            viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);


            // Arrange
            model = new RegisterViewModel() { Password = "Success" };
            controller = new AccountController(userManager.Object, signInManager.Object);
            helperContext.RequestAborted = false;
            ControllerTestHelper.Initialize(controller, helperContext);

            // Act
            result = controller.Register(model).Result;

            // Assert
            userManager.Verify();
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public void AccountController_ExternalLoginConfirmationTest()
        {
            // Arrange
            var helperContext = new ControllerTestHelperContext();
            var userManager = CreateMockUserManager<ApplicationUser>();
            var signInManager = CreateMockSignInManager<ApplicationUser>
                (userManager.Object, helperContext.HttpContext);

            var controller = new AccountController(userManager.Object, signInManager.Object);
            helperContext.User.IsAuthenticated = true;
            ControllerTestHelper.Initialize(controller, helperContext);

            // Act
            var result = controller.ExternalLoginConfirmation(new ExternalLoginConfirmationViewModel()).Result;

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Manage", redirectResult.ControllerName);

            // Arrange
            helperContext.RequestAborted = false;
            helperContext.User.IsAuthenticated = false;
            controller = new AccountController(userManager.Object, signInManager.Object);
            ControllerTestHelper.Initialize(controller, helperContext);

            // Act
            result = controller.ExternalLoginConfirmation(new ExternalLoginConfirmationViewModel()).Result;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ExternalLoginFailure", viewResult.ViewName);

        }

        private Mock<SignInManager<TUser>> CreateMockSignInManager<TUser>(UserManager<TUser> userManager, HttpContext httpContext)
            where TUser : class
        {
            var claimsManager = new Mock<IClaimsIdentityFactory<ApplicationUser>>();

            var identityOptions = new IdentityOptions { SecurityStampValidationInterval = TimeSpan.Zero };
            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Options).Returns(identityOptions);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.Value).Returns(httpContext);

            var signInManager = new Mock<SignInManager<TUser>>
                (userManager, contextAccessor.Object, claimsManager.Object, options.Object, null);

            signInManager.Setup(n =>
                n.TwoFactorSignInAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        new CancellationToken(true))).ThrowsAsync(new OperationCanceledException());

            signInManager.Setup(n =>
                n.TwoFactorSignInAsync(
                    It.IsAny<string>(),
                    "LockedOut",
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    new CancellationToken(false))).ReturnsAsync(SignInResult.LockedOut);

            signInManager.Setup(n =>
                n.TwoFactorSignInAsync(
                    It.IsAny<string>(),
                    "Success",
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    new CancellationToken(false))).ReturnsAsync(SignInResult.Success);

            signInManager.Setup(n =>
                n.TwoFactorSignInAsync(
                    It.IsAny<string>(),
                    "Fail",
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    new CancellationToken(false))).ReturnsAsync(new SignInResult());

            return signInManager;
        }

        private Mock<UserManager<TUser>> CreateMockUserManager<TUser>()
            where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var manager = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null, null);
            manager.Object.UserValidators.Add(new UserValidator<TUser>());
            manager.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            manager.Setup(n => n.CreateAsync(It.IsAny<TUser>(), "Success", new CancellationToken(false)))
                .ReturnsAsync(IdentityResult.Success);

            manager.Setup(n => n.CreateAsync(It.IsAny<TUser>(), "Fail", new CancellationToken(false)))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError() { Code = "AAA", Description = "IDENTITYERROR" }));

            manager.Setup(n => n.GenerateEmailConfirmationTokenAsync(It.IsAny<TUser>(), new CancellationToken(false)))
                .ReturnsAsync("BBB");

            manager.Setup(n => n.SendMessageAsync(
                It.IsAny<string>(), It.IsAny<IdentityMessage>(), new CancellationToken(false)))
                .ReturnsAsync(new IdentityResult()).Verifiable();

            return manager;
        }
    }
}