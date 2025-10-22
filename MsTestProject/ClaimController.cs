using Microsoft.AspNetCore.Mvc;

namespace MsTestProject;

public class ClaimController : Controller
{
    public ViewResult Create()
    {
        return View();
    }
}

[TestClass]
public class ClaimControllerTests
{
    [TestMethod]
    public void Create_GET_ReturnsView()
    {
        // Arrange
        var controller = new ClaimController();

        // Act
        var result = controller.Create() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
    }
}
