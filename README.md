# Unconstrained Mockiu

Mock anything you want, static, private, SEALED, ...

Upon various online developer forums and question and answer sites, I have observed discussions concerning the mocking of static and sealed, etc.
Many responses indicated it was not possible, and questioners were sometimes challenged to justify their need.
However, the primary purpose of unit tests is to validate functionality in isolation.
-> THERE IS NO REASON TO REDESIGN A SOFTWARE TO MAKE IT BE TESTABLE (according to some xxx testing rules of others), that is truly silly.

This library aims to respect developers' testing freedoms by enabling the mocking of methods irrespective of their declared access or implementation.
It may prove useful for scenarios where traditional mocking approaches cannot be applied yet isolation and repeatability remain priorities.
-> If you respect your FREEDOM in testing, you can use this library.

---
### Short glance or see the test cases in test project
```csharp
public class MyTests
{
    [Fact]
    public void Test_MyService_And_StaticMethod()
    {
        using (var mockEngine = new HybridMockEngine(Guid.NewGuid().ToString()))
        {
            // Mock an interface
            var mockService = mockEngine.Mock<IMyService>();
            mockService.Setup(s => s.GetData(It.IsAny<int>())).Returns("Mocked data");

            // Mock a concrete class
            var mockRepository = mockEngine.Mock<MyRepository>();
            mockRepository.Setup(r => r.FetchData(It.IsAny<int>())).Returns("Mocked repository data");

            // Setup a static method
            mockEngine.SetupStaticMethod(typeof(MyStaticClass), "StaticMethod", new Func<string>(() =>
            {
                return "Mocked static method result";
            }));

            // Use the mocks in your test
            Assert.Equal("Mocked data", mockService.Object.GetData(1));
            Assert.Equal("Mocked repository data", mockRepository.Object.FetchData(1));
            Assert.Equal("Mocked static method result", MyStaticClass.StaticMethod());
        }
    }
}
```
---

### Guidelines for Using `HybridMockEngine`

#### 1. **Initialization**

To use `HybridMockEngine`, you need to create an instance of it. Ensure to provide a unique `instanceId` to avoid conflicts if multiple instances are used.

```csharp
var mockEngine = new HybridMockEngine("uniqueInstanceId");
```

#### 2. **Mocking Dependencies**

You can mock dependencies using the `Mock` method. This method supports both Moq and Harmony-based mocking. You can specify a preferred mock setup or let the engine decide based on the type.

- **Interface or Abstract Class**: Uses Moq.
- **Concrete Class**: Uses Harmony.

```csharp
// Mock an interface or abstract class
var mockInterface = mockEngine.Mock<IMyInterface>();

// Mock a concrete class
var mockConcreteClass = mockEngine.Mock<MyConcreteClass>();
```

You can also pass an existing mock setup if you have one:

```csharp
var preferredMockSetup = new MoqMockSetup<IMyInterface>(new Mock<IMyInterface>());
var mockWithPreferredSetup = mockEngine.Mock(preferredMockSetup);
```

#### 3. **Setting Up Static Methods**

To mock static methods, use the `SetupStaticMethod` method. Provide the target type, method name, and delegate implementation.

```csharp
mockEngine.SetupStaticMethod(typeof(MyClass), "MyStaticMethod", new Action(() =>
{
    // Mock implementation
}));
```

#### 4. **Disposing Resources**

To clean up resources and remove all patches, call the `Dispose` method. This is important to avoid memory leaks and unintended side effects.

```csharp
mockEngine.Dispose();
```

For a more robust implementation, consider using the `using` statement to ensure disposal:

```csharp
using (var mockEngine = new HybridMockEngine("uniqueInstanceId"))
{
    // Mocking and setup code here
}
```

### Example Usage

Here is a complete example of how to use the `HybridMockEngine`:

```csharp
using (var mockEngine = new HybridMockEngine("testInstance"))
{
    // Mock an interface
    var mockService = mockEngine.Mock<IMyService>();

    // Mock a concrete class
    var mockRepository = mockEngine.Mock<MyRepository>();

    // Setup a static method
    mockEngine.SetupStaticMethod(typeof(MyStaticClass), "StaticMethod", new Action(() =>
    {
        // Your mock implementation
    }));

    // Use the mocks in your tests
    // ...
}
```

### Summary

- **Initialization**: Create an instance with a unique `instanceId`.
- **Mocking**: Use the `Mock` method for interfaces, abstract, or concrete classes.
- **Static Methods**: Use `SetupStaticMethod` to mock static methods.
- **Disposal**: Ensure to call `Dispose` to clean up resources.

By following these guidelines, you can effectively use the `HybridMockEngine` to mock dependencies and static methods in your unit tests.