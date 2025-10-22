using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClaimSystem.Controllers;
using System.Security.Claims;
using System.Collections.Generic;

namespace MsTestProject
{
    [TestClass]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
    [TestClass]
    public sealed class HomeControllerTests
    {
        private readonly ClaimSystem.Controllers.HomeController _controller;

        [TestMethod]
        public void Index_ReturnsView()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
