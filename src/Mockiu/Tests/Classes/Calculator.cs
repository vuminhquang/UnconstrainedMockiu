namespace Tests;

public class Calculator
{
    public virtual int Add(int a, int b) => a + b;
    public virtual int Multiply(int a, int b) => a * b;
    public virtual int Subtract(int a, int b) => a - b;
    public virtual int Divide(int a, int b) => a / b;
    public virtual int NoArgs() => 42;

    public T Echo<T>(T value) => value;

    public int Value { get; set; }
}