# Norns.Urd

![build](https://github.com/fs7744/Norns.Urd/workflows/build/badge.svg)
[![GitHub](https://img.shields.io/github/license/fs7744/Norns.Urd)](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)
[![GitHub Repo stars](https://img.shields.io/github/stars/fs7744/Norns.Urd?style=social)](https://github.com/fs7744/Norns.Urd)

## Nuget Packages

| Package Name |  NuGet | Downloads  |
|--------------|  ------- |  ----  |
| Norns.Urd | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd)](https://www.nuget.org/packages/Norns.Urd/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd) |
| Norns.Urd.Extensions.Polly | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Extensions.Polly)](https://www.nuget.org/packages/Norns.Urd.Extensions.Polly/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Extensions.Polly) |

## Welcome to Norns.Urd

Norns.urd is a lightweight AOP framework based on emit which do dynamic proxy.

It base on netstandard2.0.

The purpose of completing this framework mainly comes from the following personal wishes:

- Static AOP and dynamic AOP are implemented once
- How can an AOP framework only do dynamic proxy but can work with other DI frameworks like `Microsoft.Extensions.DependencyInjection`
- How can an AOP make both sync and Async methods compatible and leave implementation options entirely to the user

Hopefully, this library will be of some useful to you

## How to use

[中文文档](https://fs7744.github.io/Norns.Urd/zh-cn/index.html) |  [Document](https://fs7744.github.io/Norns.Urd/index.html)

## Simple Benchmark

Just simple benchmark test, and does not represent the whole scenario

Castle and AspectCore are excellent libraries,

Many implementations of Norns.urd refer to the source code of Castle and AspectCore.

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1198 (1909/November2018Update/19H2)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT


```
|                                         Method |      Mean |    Error |    StdDev |    Median |  Gen 0 | Allocated |
|----------------------------------------------- |----------:|---------:|----------:|----------:|-------:|----------:|
|       TransientInstanceCallSyncMethodWhenNoAop |  69.10 ns | 1.393 ns |  2.512 ns |  69.70 ns | 0.0178 |     112 B |
|    TransientInstanceCallSyncMethodWhenNornsUrd | 148.38 ns | 2.975 ns |  5.588 ns | 145.76 ns | 0.0534 |     336 B |
|      TransientInstanceCallSyncMethodWhenCastle | 222.48 ns | 0.399 ns |  0.312 ns | 222.50 ns | 0.0815 |     512 B |
|  TransientInstanceCallSyncMethodWhenAspectCore | 576.04 ns | 7.132 ns | 10.229 ns | 573.46 ns | 0.1030 |     648 B |
|      TransientInstanceCallAsyncMethodWhenNoAop | 114.61 ns | 0.597 ns |  0.499 ns | 114.58 ns | 0.0408 |     256 B |
|   TransientInstanceCallAsyncMethodWhenNornsUrd | 206.36 ns | 0.937 ns |  0.830 ns | 206.18 ns | 0.0763 |     480 B |
|     TransientInstanceCallAsyncMethodWhenCastle | 250.98 ns | 3.315 ns |  3.101 ns | 252.16 ns | 0.1044 |     656 B |
| TransientInstanceCallAsyncMethodWhenAspectCore | 576.00 ns | 4.160 ns |  3.891 ns | 574.99 ns | 0.1373 |     864 B |

## License
[MIT](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)