namespace Tests;

public class Person
{
    private readonly string _name;
    public string Name
    {
        get => _name;
        set => throw new NotImplementedException();
    }

    public int Age { get; }

    public Person(string name, int age)
    {
        _name = name;
        Age = age;
    }
}