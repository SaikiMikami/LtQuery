# LtQuery - a high performance mapper for .Net.

## Features

LtQuery is a ORM focus on Easy-to-use and high performance. 

LtQuery does not accept the input of SQL which is a string.
Instead, call giving a diverty, tiny query object.

```csharp
// create query object
var query = Lt.Query<Blog>().Include(_ => _.Posts)
	.Where(_ => _.UserId == Lt.Arg<int>("UserId"))
	.OrderBy(_ => _.Date).Take(20);

// execute query
var blogs = connection.Query(query, new { UserId = 5 });
```
See [wiki](wiki/LtQuery) for details.


# Performance

'benchmarks/LtQueryBenchmarks' result. 

## Measurement environment
- .NET 7.0
- Windows
- SQL Server 2019 Express on local.
- LtQuery 0.2.0, Dapper 2.1.15, EFCore 7.0.13

## SelectOne from Single table

| Method  | Mean      | Error    | StdDev   | Gen0    | Allocated |
|-------- |----------:|---------:|---------:|--------:|----------:|
| ADO.NET |  90.59 μs | 0.582 μs | 0.545 μs |  2.6855 |   5.59 KB |
| **LtQuery** |  **96.23 μs** | **0.731 μs** | **0.571 μs** |  **2.5635** |   **5.38 KB** |
| Dapper  | 110.75 μs | 0.440 μs | 0.390 μs |  2.8076 |   5.97 KB |
| EFCore  | 290.41 μs | 2.543 μs | 2.254 μs | 36.1328 |  74.47 KB |

## SelectMany(20) from Single table

| Method  | Mean     | Error   | StdDev  | Gen0    | Allocated |
|-------- |---------:|--------:|--------:|--------:|----------:|
| ADO.NET | 215.3 μs | 1.19 μs | 1.11 μs | 32.9590 |  67.48 KB |
| **LtQuery** | **226.7 μs** | **0.99 μs** | **0.93 μs** | **32.7148** |  **67.23 KB** |
| Dapper  | 243.5 μs | 2.63 μs | 2.46 μs | 34.1797 |  70.02 KB |
| EFCore  | 465.6 μs | 4.59 μs | 4.29 μs | 69.3359 | 142.51 KB |

## SelectMany(20) from With children

| Method  | Mean     | Error     | StdDev    | Gen0     | Gen1     | Allocated |
|-------- |---------:|----------:|----------:|---------:|---------:|----------:|
| ADO.NET | 4.103 ms | 0.0469 ms | 0.0439 ms | 296.8750 | 195.3125 |   1.44 MB |
| **LtQuery** | **4.309 ms** | **0.0844 ms** | **0.1263 ms** | **304.6875** | **195.3125** |   **1.44 MB** |
| Dapper  | 4.426 ms | 0.0481 ms | 0.0402 ms | 351.5625 | 218.7500 |   1.62 MB |
| EFCore  | 7.258 ms | 0.1214 ms | 0.1136 ms | 554.6875 | 351.5625 |    2.6 MB |

# Performance-aware code
In LtQuery, when the user holds the query object, 
the optimized process is executed when the second time Later.

```csharp
// hold query object
static readonly Query<Blog> _query = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>()).ToImmutable();

public Blog Find(int id)
{
  return _connection.QuerySingle(_query, new{ Id = id });
}
```

# Policy
Aiming for the best ORM for DDD.

## License

`LtQuery` is licensed under the [MIT License](LICENSE).
