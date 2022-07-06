
using Beans.Common.Enumerations;

namespace Beans.Common.Tests;

[TestClass]
public class ApiErrorTests
{
    [TestMethod]
    public void TestApiErrorSuccess()
    {
        var error = ApiError.FromDalResult(DalResult.Success);
        Assert.IsTrue(error.Successful);
    }

    [TestMethod]
    public void TestApiError()
    {
        const string errorMessage = "This is a test error message";

        var dalresult = new DalResult(DalErrorCode.NotFound, new Exception(errorMessage));
        var error = ApiError.FromDalResult(dalresult);
        Assert.AreEqual(error.Code, (int)dalresult.ErrorCode);
        Assert.AreEqual(error.Message, errorMessage);
    }
}
