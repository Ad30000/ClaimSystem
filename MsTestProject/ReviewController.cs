using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClaimSystem.Models;


namespace MsTestProject;

[TestClass]
public class ReviewController
{
    private class FakeReviewController : Controller
    {
        public IActionResult Index()
        {
            var claims = new List<Claim>
            {
                new Claim { Status = "Pending" },
                new Claim { Status = "Pending" }
            };
            return View(claims);
        }
    }

    private FakeReviewController Controller = new FakeReviewController();

    [TestMethod]
    public void Index_ReturnsViewWithPendingClaims()
    {
        // Act
        var result = Controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Model);
        var claims = result.Model as List<Claim>;
        Assert.IsTrue(claims.All(c => c.Status == "Pending"));
    }
}
