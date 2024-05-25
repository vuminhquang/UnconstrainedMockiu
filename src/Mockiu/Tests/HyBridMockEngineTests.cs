using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using Mockiu.Hybrid;
using Moq;

namespace Tests;

using System;
using Xunit;

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
            .Setup(bar => bar.DoSomething(), () => Console.WriteLine("Mocked Bar DoSomething"))
            .Setup(bar => bar.GetNumber(), () => 99);

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
        var barNumber = bar.GetNumber();  // Should return 99

        Assert.Equal(99, barNumber);
    }
}


public interface IFoo
{
    void DoSomething();
    int GetNumber();
    string SomeProperty { get; set; }
}

public class Bar
{
    public virtual void DoSomething() { /* Original implementation */ }
    public virtual int GetNumber() { return 0; /* Original implementation */ }
}