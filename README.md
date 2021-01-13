# The cost of throwing exceptions in C#

This repository contains a small project showing the cost of throwing exceptions in C#.
It examines a very common case: An entity (in this case, a user) is requested from a service, but the service cannot find the entity. 
How should the service communicate this?

The project includes three methods that each communicates the result in a different way.

1. `GetUserWithExceptions()`: A method that will throw an exception if the user is not found.
1. `GetUserWithDefault()`: A method that will return `default` if the user is not found.
1. `GetUserWithTryGet()`: A method that will return `true` or `false` to indicate if the user was found or not, and - if the user was found - return him in an `out` variable.

## Benchmarking results

[BenchmarkDotNet](https://benchmarkdotnet.org/) is used to benchmark the three methods.

Using each of the three methods, the benchmark attempts to fetch a non-existent user from the user service.
For the sake of curiosity, I have used .NET 4.8, .NET Core 2.1., .NET Core 3.1 and .NET 5.0 as runtimes.

The results of the benchmark can be seen in the table below.

```ini
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT
  Job-ICSTSG : .NET Framework 4.8 (4.8.4300.0), X64 RyuJIT
  Job-OYULFM : .NET Core 2.1.23 (CoreCLR 4.6.29321.03, CoreFX 4.6.29321.01), X64 RyuJIT
  Job-ZTBEOE : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  Job-NTTAIL : .NET Core 5.0.1 (CoreCLR 5.0.120.57516, CoreFX 5.0.120.57516), X64 RyuJIT

IterationCount=30  LaunchCount=3  WarmupCount=10

|                Method |       Runtime |          Mean |       Error |      StdDev |        Median |    Ratio | RatioSD |
|---------------------- |-------------- |--------------:|------------:|------------:|--------------:|---------:|--------:|
|    GetUserWithDefault |      .NET 4.8 |      8.707 ns |   0.0959 ns |   0.2672 ns |      8.590 ns |     1.00 |    0.00 |
|     GetUserWithTryGet |      .NET 4.8 |      9.197 ns |   0.1616 ns |   0.4424 ns |      9.258 ns |     1.06 |    0.06 |
| GetUserWithExceptions |      .NET 4.8 | 14,500.979 ns |  53.0287 ns | 143.3661 ns | 14,506.671 ns | 1,664.59 |   48.59 |
|                       |               |               |             |             |               |          |         |
|     GetUserWithTryGet | .NET Core 2.1 |      6.878 ns |   0.0292 ns |   0.0789 ns |      6.899 ns |     0.92 |    0.04 |
|    GetUserWithDefault | .NET Core 2.1 |      7.499 ns |   0.1088 ns |   0.2961 ns |      7.339 ns |     1.00 |    0.00 |
| GetUserWithExceptions | .NET Core 2.1 | 14,962.689 ns | 154.2974 ns | 427.5574 ns | 14,913.022 ns | 1,997.56 |  114.99 |
|                       |               |               |             |             |               |          |         |
|    GetUserWithDefault | .NET Core 3.1 |      6.965 ns |   0.0212 ns |   0.0567 ns |      6.936 ns |     1.00 |    0.00 |
|     GetUserWithTryGet | .NET Core 3.1 |      7.814 ns |   0.0195 ns |   0.0504 ns |      7.797 ns |     1.12 |    0.01 |
| GetUserWithExceptions | .NET Core 3.1 | 13,392.766 ns |  63.8685 ns | 172.6721 ns | 13,303.902 ns | 1,923.56 |   32.22 |
|                       |               |               |             |             |               |          |         |
|     GetUserWithTryGet | .NET Core 5.0 |      6.320 ns |   0.0382 ns |   0.1051 ns |      6.334 ns |     0.95 |    0.05 |
|    GetUserWithDefault | .NET Core 5.0 |      6.682 ns |   0.1305 ns |   0.3573 ns |      6.517 ns |     1.00 |    0.00 |
| GetUserWithExceptions | .NET Core 5.0 | 13,241.254 ns |  76.2873 ns | 211.3918 ns | 13,232.031 ns | 1,985.94 |  104.29 |
```
(Take note of the time scale of *nanoseconds* when comparing the results.)

The table shows the results of benchmarking the three methods in each of the previously mentioned runtimes.
`Ratio` is the most interesting column and shows the performance of the specified method as a ratio of the baseline method performance (in this case, the baseline is `GetUserWithDefault()`).

The results show that the differences between `GetUserWithDefault()` and `GetUserWithTryGet()` are tiny. The differences should probably be attributed to uncertainties in measurement rather than an actual performance variations. 

It is however obvious that there is a great performance penalty for throwing exceptions: `GetUserWithExceptions()` is **~1700 to ~2000** times slower than its counterparts.

