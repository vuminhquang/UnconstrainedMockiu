using System.Reflection;
using Mockiu;

namespace Tests;

using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Test_Add_WithMock()
    {
        using var mockEngine = new MockEngine("com.example.calculator.mock");
        // HarmonyInstanceContainer2.Instance = mockEngine;

        mockEngine.Mock<Calculator>("Add", (Calculator _, int a, int b)=> a*b); // Mock Add to multiply instead

        var calculator = new Calculator();
        var result = calculator.Add(2, 3);

        Assert.Equal(6, result); // 2 * 3 because of the mock
    }

    [Fact]
    public void Test_Add_WithoutMock()
    {
        var calculator = new Calculator();
        var result = calculator.Add(2, 3);
    
        Assert.Equal(5, result); // 2 + 3 because no mock
    }
    
    [Fact]
    public void Test_Multiply_NoMock()
    {
        var calculator = new Calculator();
        var result = calculator.Multiply(2, 3);
    
        Assert.Equal(6, result); // 2 * 3, original method
    }
    
    [Fact]
    public void Test_Property_WithMock()
    {
        using (var mockEngine = new MockEngine("com.example.calculator.mock"))
        {
            mockEngine.Mock<Calculator>("Value", getterImplementation: (_) => 42, setterImplementation: (_, value) => { });
    
            var calculator = new Calculator
            {
                Value = 100 // Setter is mocked and does nothing
            };
            var result = calculator.Value; // Getter is mocked to return 42
    
            Assert.Equal(42, result);
        }
    }
    
    [Fact]
    public void Test_Method_With_Zero_Arguments()
    {
        using (var mockEngine = new MockEngine("com.example.calculator.mock"))
        {
            // Set the instance of the mock engine container
            // HarmonyInstanceContainer2.Instance = mockEngine;

            // Mock the NoArgs method to return 100
            mockEngine.Mock<Calculator>("NoArgs", (Func<Calculator, int>)(_ => 100));

            var calculator = new Calculator();
            var result = calculator.NoArgs();

            Assert.Equal(100, result); // Mocked to return 100
        }

        // Clean up the instance after test
        // HarmonyInstanceContainer2.Instance = null;
    }
    
    [Fact]
    public void Test_Method_With_Multiple_Arguments()
    {
        using (var mockEngine = new MockEngine("com.example.calculator.mock"))
        {
            mockEngine.Mock<Calculator>("Subtract", (Calculator _, int a, int b) => a + b);
    
            var calculator = new Calculator();
            var result = calculator.Subtract(1, 2);
    
            Assert.Equal(3, result); // Mocked to return sum of three numbers
        }
    }
    
    [Fact]
    public void Test_Generic_Method()
    {
        using (var mockEngine = new MockEngine("com.example.calculator.mock"))
        {
        
            // // Mock the generic method 'Echo' for Calculator class
            mockEngine.MockGeneric<Calculator>("Echo", 
                new Type[] { typeof(int) }, 
                (Func<Calculator,int, int>)((_, arg) => arg));
        
            var calculator = new Calculator();
            var result = calculator.Echo<int>(42);
        
            Assert.Equal(42, result); // Mocked to return the input value
        }
    }
    
    [Fact]
    public void Test_Constructor_WithMock()
    {
        using var mockEngine = new MockEngine("com.example.person.mock");

        // Mock the constructor for Person class
        mockEngine.MockConstructor<Person>((Action<Person, Dictionary<string, object>, object[]>)((instance, fields, args) =>
        {
            fields["_name"] = "Mocked Name";
            fields["Age"] = 99;
        }));

        var person = new Person("Original Name", 30);

        Assert.Equal("Mocked Name", person.Name);
        Assert.Equal(99, person.Age); // Mocked constructor values
    }
}