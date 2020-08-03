using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UnitTestsForCore2MVC.CustomLogger;
using NUnit.Framework;
using SampleUnitTestApplication2;
using SampleUnitTestApplication2.Controllers;
using SampleUnitTestApplication2.Data;
using SampleUnitTestApplication2.Models;
using SampleUnitTestApplication2.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestsForCore2MVC
{
    [TestFixture]
    public class UnitTest1
    {
        private HomeController _controller;
        private Dictionary<object, object> _httpContextItems = new Dictionary<object, object>();
        private List<TestsLoggerEvent> _testLogsStore = new List<TestsLoggerEvent>();

        [OneTimeSetUp]
        public void TestSetup()
        {
            var configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables()
               .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMvc().AddJsonOptions(jsonOptions =>
            {
                jsonOptions.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });
            services.AddOptions();

            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<IEmailSender, EmailSender>();

            var options = Options.Create(configuration.GetSection("AppSettings").Get<AppSettings>());
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddTestsLogger(_testLogsStore);

            var userManagerLogger = loggerFactory.CreateLogger<UserManager<ApplicationUser>>();

            var mockUserManager = new Mock<UserManager<ApplicationUser>>(MockBehavior.Default,
                new Mock<IUserStore<ApplicationUser>>().Object,
                  new Mock<IOptions<IdentityOptions>>().Object,
                  new Mock<IPasswordHasher<ApplicationUser>>().Object,
                  new IUserValidator<ApplicationUser>[0],
                  new IPasswordValidator<ApplicationUser>[0],
                  new Mock<ILookupNormalizer>().Object,
                  new Mock<IdentityErrorDescriber>().Object,
                  new Mock<IServiceProvider>().Object,
                  userManagerLogger);

            var context = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(" ... connection string ... ").Options);

            //var mockSignInManager = new Mock<SignInManager<ApplicationUser>>(MockBehavior.Strict, mockUserManager.Object, null, null, null, null, null, null, null, null);
            var emailSender = serviceProvider.GetService<IEmailSender>();
            var mockHostingEnvironment = new Mock<IHostingEnvironment>(MockBehavior.Strict);

            // configuring the HTTP context and user principal,
            // in order to be able to use the User.Identity.Name property in the controller action
            var validPrincipal = new ClaimsPrincipal(
                new[]
                {
                        new ClaimsIdentity(
                            new[] {new Claim(ClaimTypes.Name, "testsuser@testinbox.com") })
                });

            var mockHttpContext = new Mock<HttpContext>(MockBehavior.Strict);
            mockHttpContext.SetupGet(hc => hc.User).Returns(validPrincipal);
            mockHttpContext.SetupGet(c => c.Items).Returns(_httpContextItems);
            mockHttpContext.SetupGet(ctx => ctx.RequestServices)
                .Returns(serviceProvider);

            var collection = Mock.Of<IFormCollection>();
            var request = new Mock<HttpRequest>();
            request.Setup(f => f.ReadFormAsync(CancellationToken.None)).Returns(Task.FromResult(collection));

            var mockHeader = new Mock<IHeaderDictionary>();
            mockHeader.Setup(h => h["X-Requested-With"]).Returns("XMLHttpRequest");
            request.SetupGet(r => r.Headers).Returns(mockHeader.Object);

            mockHttpContext.SetupGet(c => c.Request).Returns(request.Object);

            var response = new Mock<HttpResponse>();
            response.SetupProperty(it => it.StatusCode);

            mockHttpContext.Setup(c => c.Response).Returns(response.Object);

            _controller = new HomeController(
                options,
                context,
                mockUserManager.Object,
                null,
                emailSender,
                loggerFactory,
                mockHostingEnvironment.Object,
                configuration);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = mockHttpContext.Object
            };
        }

        [Test]
        public void TestAboutAction()
        {
            var controllerActionResult = _controller.About();

            Assert.IsNotNull(controllerActionResult);
            Assert.IsInstanceOf<ViewResult>(controllerActionResult);
            var viewResult = controllerActionResult as ViewResult;
            Assert.AreSame(viewResult.ViewData["Message"], "Your application description page.");

            Assert.AreSame(_controller.User.Identity.Name, "testsuser@testinbox.com");

            Assert.AreSame(_controller.Request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest");

            Assert.DoesNotThrow(() => { _controller.Response.StatusCode = 500; });
        }
    }
}
