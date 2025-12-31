```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.7 (24G222) [Darwin 24.6.0]
Intel Core i7-4980HQ CPU 2.80GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3


```
| Method                             | Mean        | Error       | StdDev       | Median      |
|----------------------------------- |------------:|------------:|-------------:|------------:|
| RappSerialize                      |    582.9 ns |    48.95 ns |    143.56 ns |    558.8 ns |
| RappDeserialize                    |    813.6 ns |    48.54 ns |    140.81 ns |    806.7 ns |
| MemoryPackSerialize                |    275.2 ns |    19.61 ns |     56.57 ns |    260.0 ns |
| MemoryPackDeserialize              |    609.7 ns |    51.33 ns |    149.73 ns |    611.2 ns |
| HybridCache_GetOrCreate_Rapp       |    769.7 ns |    80.86 ns |    238.40 ns |    741.2 ns |
| HybridCache_GetOrCreate_MemoryPack |    755.5 ns |    54.44 ns |    153.56 ns |    736.1 ns |
| MemoryCache_GetOrCreate            |    106.9 ns |     6.29 ns |     18.56 ns |    103.2 ns |
| RealisticCacheWorkload_Rapp        | 95,447.7 ns | 6,210.52 ns | 18,214.38 ns | 87,959.7 ns |
| RealisticCacheWorkload_MemoryPack  | 85,583.3 ns | 4,315.02 ns | 12,587.12 ns | 81,206.2 ns |
| RealisticCacheWorkload_Memory      | 17,453.4 ns |   617.79 ns |  1,711.89 ns | 16,991.7 ns |
