using System.IO.Pipelines;
using System.Reflection;

namespace Scripts.UnitTests;

public class TestTemplate
{
    [Fact]
    public void ExampleTest()
    {
        //Arrange scenario
        int i = 0;
 
        //test the unit in question
        bool result = i == 0;

        //check to see if test passes
        Assert.True(result);
    }
}
