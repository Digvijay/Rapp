```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7 (24G222) [Darwin 24.6.0]
Intel Core i7-4980HQ CPU 2.80GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3


```
| Method                                   | Mean         | Error        | StdDev        | Median       |
|----------------------------------------- |-------------:|-------------:|--------------:|-------------:|
| RappSerialize                            |    397.23 ns |    11.577 ns |     33.771 ns |    397.37 ns |
| RappDeserialize                          |    240.85 ns |     4.884 ns |     12.072 ns |    240.53 ns |
| MemoryPackSerialize                      |    196.96 ns |     4.010 ns |      6.002 ns |    196.42 ns |
| MemoryPackDeserialize                    |    180.01 ns |     3.654 ns |      8.754 ns |    180.54 ns |
| JsonSerialize                            |  1,764.09 ns |    30.938 ns |     31.771 ns |  1,771.27 ns |
| JsonDeserialize                          |  4,238.09 ns |    87.943 ns |    248.044 ns |  4,213.97 ns |
| HybridCache_Rapp                         |    436.93 ns |     8.138 ns |      7.992 ns |    434.61 ns |
| HybridCache_MemoryPack                   |    416.51 ns |    17.887 ns |     51.321 ns |    438.44 ns |
| DirectMemoryCache                        |     93.88 ns |     6.428 ns |     18.751 ns |     90.44 ns |
| RealisticWorkload_HybridCache_Rapp       | 30,483.58 ns |   517.234 ns |    672.550 ns | 30,243.31 ns |
| RealisticWorkload_HybridCache_MemoryPack | 44,092.34 ns | 4,565.780 ns | 13,462.307 ns | 46,787.07 ns |
| RealisticWorkload_DirectMemory           | 12,969.63 ns |   253.903 ns |    451.313 ns | 12,965.60 ns |
