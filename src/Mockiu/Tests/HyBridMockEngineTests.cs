using System.Reflection;
using Mockiu.Hybrid;
using Moq;

namespace Tests;
public class ExampleTests
{
    [Fact]
    public void TestMethodMocking()
    {
        using var engine = new HybridMockEngine("test");

        // Mocking IFoo using Moq
        var fooMock = engine.Mock<IFoo>()
            .Setup(foo => foo.DoSomething(), () => Console.WriteLine("Mocked DoSomething"))
            .Setup(foo => foo.GetNumber(), () => 42)
            .SetupProperty(foo => foo.SomeProperty, "MockedProperty");

        // Retrieve the mocked object using GetObject
        var foo = fooMock.GetObject();

        // Mocking Bar using Harmony
        var barMock = engine.Mock<Bar>()
            .Setup(bar => bar.DoSomething(), () => Console.WriteLine("Mocked Bar DoSomething"));

        // Retrieve the mocked object using GetObject
        var bar = barMock.GetObject();

        // Test the mocked behavior for IFoo
        foo.DoSomething();  // Should print "Mocked DoSomething"
        var fooNumber = foo.GetNumber();  // Should return 42
        var fooProperty = foo.SomeProperty;  // Should be "MockedProperty"

        Assert.Equal(42, fooNumber);
        Assert.Equal("MockedProperty", fooProperty);

        // Test the mocked behavior for Bar
        bar.DoSomething();  // Should print "Mocked Bar DoSomething"
        var barNumber = bar.GetNumber(2);  // Should return 0

        Assert.Equal(0, barNumber);
    }
    
    [Fact]
    public void MockingFooUsingMoq_ShouldInvokeMockedDoSomething()
    {
        var engine = new HybridMockEngine("TestInstance");

        // Mocking Bar using Harmony
        var barMock = engine.Mock<Bar>()
            .Setup(bar => bar.DoSomething(), () => Console.WriteLine("Mocked Bar DoSomething"))
            .Setup(bar => bar.GetNumber(It.IsAny<int>()), (int a) => a * 2);

        var bar = barMock.GetObject();
        bar.DoSomething(); // Outputs: Mocked Bar DoSomething
        int result = bar.GetNumber(5); // result is 10
        Assert.Equal(10, result);
    }
    
    [Fact]
    public void MockingBarUsingHarmony_ShouldInvokeMockedDoSomething()
    {
        using var engine = new HybridMockEngine(Guid.NewGuid().ToString());
        // Redirect console output to capture the mocked method's output
        using (var sw = new StringWriter())
        {
            Console.SetOut(sw);

            // Mocking Bar using Harmony
            var barMock = engine.Mock<Bar>()
                .Setup(bar => bar.DoSomething(), () => Console.WriteLine("Mocked Bar DoSomething"));
            
            var barInstance = barMock.GetObject();
            barInstance.DoSomething();

            var output = sw.ToString().Trim();
            Assert.Equal("Mocked Bar DoSomething", output);
        }
    }
    
    [Fact]
    public void SetupStaticMethod_ShouldReuseHarmonyMockSetup()
    {
        using var engine = new HybridMockEngine(Guid.NewGuid().ToString());
        // Arrange
        engine.SetupStaticMethod(typeof(MyStaticClass), nameof(MyStaticClass.MyStaticMethod), new Func<string>(() => "Mocked"));
        engine.SetupStaticMethod(typeof(MyStaticClass), nameof(MyStaticClass.MyOtherStaticMethod), new Func<int>(() => 99));

        // Act
        var result1 = MyStaticClass.MyStaticMethod();
        var result2 = MyStaticClass.MyOtherStaticMethod();

        // Assert
        Assert.Equal("Mocked", result1);
        Assert.Equal(99, result2);

        // Check that only one HarmonyMockSetup was created for MyStaticClass
        var field = typeof(HybridMockEngine).GetField("_harmonySetups", BindingFlags.NonPublic | BindingFlags.Instance);
        var harmonySetups = (Dictionary<Type, HarmonyMockSetup<object>>)field.GetValue(engine);
        
        Assert.Single(harmonySetups);
        Assert.True(harmonySetups.ContainsKey(typeof(MyStaticClass)));
    }
}


// --- Test classes and interfaces ---

public interface IFoo
{
    void DoSomething();
    int GetNumber();
    string SomeProperty { get; set; }
}

public class Bar
{
    public virtual void DoSomething() { /* Original implementation */ }
    public virtual int GetNumber(int a) { return 0; /* Original implementation */ }
}

public static class MyStaticClass
{
    public static string MyStaticMethod()
    {
        return "Original";
    }

    public static int MyOtherStaticMethod()
    {
        return 42;
    }
}