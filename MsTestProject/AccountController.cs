using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MsTestProject;

[TestClass]
public class AccountController
{

    private readonly ClaimSystem.Controllers.AccountController _controller;

    public AccountController()
    {
        _controller = new ClaimSystem.Controllers.AccountController();
    }



    [TestMethod]
    public void Login_GET_ReturnsView()
    {
        // Act
        var result = _controller.Login();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ViewResult));
    }
}
