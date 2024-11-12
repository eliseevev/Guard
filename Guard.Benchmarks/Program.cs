using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Guard.Static;
using System.Linq.Expressions;

namespace Guard.Benchmarks;

#region Модельки для теста
public class Example
{
    public Example2? Name { get; set; }
}

public class Example2
{
    public Example3? Name { get; set; }
}

public class Example3
{
    public string? Name { get; set; }
}

#endregion

public static class DirectFuncForBaseLine
{
    /// <summary>
    /// Вызов напрямую. (для baseline в тестах)
    /// </summary>
    public static TResult DirectFunc<T, TResult>(T obj, Func<T, TResult> selector)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj));

        try
        {
            return selector(obj);
        }
        catch (NullReferenceException ex)
        {
            throw new ArgumentNullException();
        }
    }
}


[MemoryDiagnoser]
public class Benchmarks
{
    private readonly Example _exampleWithNull = new()
    {
        Name = null // Example2 is null
    };

    private readonly Example _exampleWithoutNull = new()
    {
        Name = new Example2
        {
            Name = new Example3 { Name = "Test" }
        }
    };

    private readonly Func<Example, string?> _funcSelector = it => it.Name.Name.Name;
    private readonly Expression<Func<Example, string?>> _expression = it => it.Name.Name.Name;
    internal Guard<Example> setuped_guard;
    internal Polled.Guard<Example> setuped_guard_polled;

    [GlobalSetup]
    public void Setup()
    {
        setuped_guard = Guard.New(_exampleWithoutNull);
        setuped_guard_polled = Polled.Guard.New(_exampleWithoutNull);
        setuped_guard.Get(it => it.Name.Name.Name);
        setuped_guard_polled.Get(it => it.Name.Name.Name);
    }


    /// <summary>
    /// Вызов напрямую.
    /// </summary>
    [Benchmark(Baseline = true)]
    public string? DirectFunc_NoNull()
    {
        return DirectFuncForBaseLine.DirectFunc(_exampleWithoutNull, _funcSelector);
    }

    /// <summary>
    /// ЗАкешированный экпрешн через статик.
    /// </summary>
    [Benchmark]
    public string? Static_CachedExpression_NoNull()
    {
        return StaticGuard.CachedExpression(_exampleWithoutNull, _expression);
    }

    /// <summary>
    /// Не кешированный экпрешн (как сейчас)
    /// </summary>
    [Benchmark]
    public string? Static_UncachedExpression_NoNull()
    {
        return StaticGuard.UncachedExpression(_exampleWithoutNull, _expression);
    }

    /// <summary>
    /// ЗАкешированный экпрешн.
    /// </summary>
    [Benchmark]
    public string? Guard_CachedExpression_NoNull()
    {
        var guard = Guard.New(_exampleWithoutNull);
        return guard.Get(it2 => it2.Name.Name.Name);
    }

    /// <summary>
    /// ЗАкешированный экпрешн.
    /// </summary>
    [Benchmark]
    public string? Guard_Setuped_CachedExpression_NoNull()
    {
        return setuped_guard.Get(it => it.Name.Name.Name);
    }

    /// <summary>
    /// ЗАкешированный экпрешн.
    /// </summary>
    [Benchmark]
    public string? Guard_Polled_CachedExpression_NoNull()
    {
        var guard = Polled.Guard.New(_exampleWithoutNull);
        return guard.Get(it2 => it2.Name.Name.Name);
    }

    /// <summary>
    /// ЗАкешированный экпрешн.
    /// </summary>
    [Benchmark]
    public string? Guard_Polled_Setuped_CachedExpression_NoNull()
    {
        return setuped_guard_polled.Get(it => it.Name.Name.Name);
    }
}

class Program
{
    static void Main(string[] args)
    {
        var _exampleWithoutNull = new Example()
        {
            Name = new Example2
            {
                Name = new Example3 { Name = "Test" }
            }
        };

        BenchmarkRunner.Run<Benchmarks>();
    }
}
