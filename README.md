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
var blogs = connection.Select(query, new { UserId = 5 });
```
See [wiki](https://github.com/SaikiMikami/LtQuery/wiki/LtQuery) for details.


# Performance

'benchmarks/LtQueryBenchmarks' result. 

## Measurement environment
- .NET 7.0
- Windows
- SQL Server 2019 Express on local.
- LtQuery 0.2.1, Dapper 2.1.15, EFCore 7.0.13

## SelectOne from Single table

| ORM  | Mean      | Error    | StdDev   | Gen0    | Allocated |
|-------- |----------:|---------:|---------:|--------:|----------:|
| ADO.NET |  88.61 μs | 1.716 μs | 2.462 μs |  87.10 μs |  2.5635 |    5.3 KB |
| **LtQuery** |  **89.17 μs** | **1.768 μs** | **1.892 μs** |  **89.54 μs** |  **2.5635** |   **5.38 KB** |
| Dapper  | 108.11 μs | 0.267 μs | 0.250 μs | 108.03 μs |  2.8076 |   5.97 KB |
| EFCore  | 246.16 μs | 1.330 μs | 1.244 μs | 246.16 μs | 36.1328 |  74.47 KB |

## SelectMany(20) from Single table

| ORM  | Mean     | Error   | StdDev  | Gen0    | Allocated |
|-------- |---------:|--------:|--------:|--------:|----------:|
| ADO.NET | 209.2 μs | 2.53 μs | 2.37 μs | 32.7148 |   67.2 KB |
| **LtQuery** | **212.9 μs** | **1.66 μs** | **1.29 μs** | **32.7148** |  **67.23 KB** |
| Dapper  | 233.0 μs | 1.29 μs | 1.08 μs | 34.1797 |  70.02 KB |
| EFCore  | 381.0 μs | 2.47 μs | 2.06 μs | 69.3359 | 142.51 KB |

## SelectMany(20) from With children

| ORM  | Mean     | Error     | StdDev    | Gen0     | Gen1     | Allocated |
|-------- |---------:|----------:|----------:|---------:|---------:|----------:|
| ADO.NET | 3.883 ms | 0.0633 ms | 0.0592 ms | 296.8750 | 203.1250 |   1.44 MB |
| **LtQuery** | **3.906 ms** | **0.0380 ms** | **0.0337 ms** | **296.8750** | **195.3125** |   **1.44 MB** |
| Dapper  | 4.416 ms | 0.0255 ms | 0.0226 ms | 359.3750 | 187.5000 |   1.62 MB |
| EFCore  | 6.816 ms | 0.0720 ms | 0.0673 ms | 554.6875 | 367.1875 |    2.6 MB |

# Performance-aware code
In LtQuery, when the user holds the query object, 
the optimized process is executed when the second time Later.

```csharp
// hold query object
static readonly Query<Blog> _query = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

public Blog Find(int id)
{
  return _connection.Single(_query, new { Id = id });
}
```

# Policy
Aiming for the best ORM for DDD.

## License

`LtQuery` is licensed under the [MIT License](LICENSE).
