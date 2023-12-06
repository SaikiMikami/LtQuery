# LtQuery - a high performance mapper for .Net.

## Packages

| Package | NuGet | Downloads |
| ------- | ----- | --------- |
| [LtQuery](https://www.nuget.org/packages/LtQuery/) | [![LtQuery](https://img.shields.io/nuget/v/LtQuery.svg)](https://www.nuget.org/packages/LtQuery/) | [![LtQuery](https://img.shields.io/nuget/dt/LtQuery.svg)](https://www.nuget.org/packages/LtQuery/) |
| [LtQuery.Relational](https://www.nuget.org/packages/LtQuery.Relational/) | [![LtQuery.Relational](https://img.shields.io/nuget/v/LtQuery.Relational.svg)](https://www.nuget.org/packages/LtQuery.Relational/) | [![LtQuery.Relational](https://img.shields.io/nuget/dt/LtQuery.Relational.svg)](https://www.nuget.org/packages/LtQuery.Relational/) |
| [LtQuery.SqlServer](https://www.nuget.org/packages/LtQuery.SqlServer/) | [![LtQuery.SqlServer](https://img.shields.io/nuget/v/LtQuery.SqlServer.svg)](https://www.nuget.org/packages/LtQuery.SqlServer/) | [![LtQuery.SqlServer](https://img.shields.io/nuget/dt/LtQuery.SqlServer.svg)](https://www.nuget.org/packages/LtQuery.SqlServer/) |
| [LtQuery.MySql](https://www.nuget.org/packages/LtQuery.MySql/) | [![LtQuery.MySql](https://img.shields.io/nuget/v/LtQuery.MySql.svg)](https://www.nuget.org/packages/LtQuery.MySql/) | [![LtQuery.MySql](https://img.shields.io/nuget/dt/LtQuery.MySql.svg)](https://www.nuget.org/packages/LtQuery.MySql/) |
| [LtQuery.Sqlite](https://www.nuget.org/packages/LtQuery.Sqlite/) | [![LtQuery.Sqlite](https://img.shields.io/nuget/v/LtQuery.Sqlite.svg)](https://www.nuget.org/packages/LtQuery.Sqlite/) | [![LtQuery.Sqlite](https://img.shields.io/nuget/dt/LtQuery.Sqlite.svg)](https://www.nuget.org/packages/LtQuery.Sqlite/) |

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
See [wiki](https://github.com/SaikiMikami/LtQuery/wiki) for details.


# Performance

'benchmarks/LtQueryBenchmarks' result. 

## Measurement environment
- .NET 7.0
- Windows 11
- SQL Server 2019 Express on local.
- LtQuery 1.2.0, Dapper 2.1.24, EFCore 7.0.13

## SelectOne from Single table

Result of `connection.Single(Lt.Query<Blog>().Where(_ => _.Id == 1))`

| ORM     | Mean      | Error    | StdDev   | Gen0    | Allocated |
|-------- |----------:|---------:|---------:|--------:|----------:|
| ADO.NET |  87.76 μs | 0.416 μs | 0.325 μs |  2.6855 |   5.59 KB |
| **LtQuery** |  **89.07 μs** | **1.376 μs** | **1.220 μs** |  **2.8076** |   **5.77 KB** |
| Dapper  |  90.79 μs | 0.278 μs | 0.260 μs |  2.8076 |   5.83 KB |
| EFCore  | 244.90 μs | 1.139 μs | 1.066 μs | 36.1328 |  74.47 KB |

## SelectMany(20) from Single table

Result of `connection.Select(Lt.Query<Blog>().Take(20))`

| ORM     | Mean     | Error   | StdDev  | Gen0    | Allocated |
|-------- |---------:|--------:|--------:|--------:|----------:|
| ADO.NET | 198.6 μs | 3.73 μs | 3.66 μs | 32.9590 |  67.48 KB |
| **LtQuery** | **201.6 μs** | **1.28 μs** | **1.13 μs** | **32.9590** |  **67.52 KB** |
| Dapper  | 201.8 μs | 1.35 μs | 1.27 μs | 34.1797 |  69.88 KB |
| EFCore  | 419.1 μs | 4.32 μs | 4.04 μs | 69.3359 | 142.51 KB |

## SelectMany(20) from With children

Result of `connection.Select(Lt.Query<Blog>().Include(_ => _.Posts).Take(20))`

| ORM     | Mean     | Error     | StdDev    | Gen0     | Gen1     | Allocated |
|-------- |---------:|----------:|----------:|---------:|---------:|----------:|
| ADO.NET | 4.581 ms | 0.0514 ms | 0.0481 ms | 289.0625 | 203.1250 |   1.44 MB |
| **LtQuery** | **4.743 ms** | **0.0941 ms** | **0.1121 ms** | **320.3125** | **195.3125** |   **1.44 MB** |
| Dapper  | 4.765 ms | 0.0253 ms | 0.0224 ms | 359.3750 | 195.3125 |   1.62 MB |
| EFCore  | 7.002 ms | 0.0498 ms | 0.0465 ms | 570.3125 | 359.3750 |    2.6 MB |

## AddRange(10)

Result of `connection.AddRange(blogs)`

| ORM     | Mean     | Error    | StdDev  | Gen0    | Allocated |
|-------- |---------:|---------:|--------:|--------:|----------:|
| ADO.NET | 344.2 μs |  3.58 μs | 3.35 μs |  5.3711 |  11.38 KB |
| **LtQuery** | **350.6 μs** |  **3.16 μs** | **2.95 μs** |  **6.3477** |   **13.8 KB** |
| Dapper  | 353.6 μs |  3.68 μs | 3.45 μs |  5.8594 |  12.67 KB |
| EFCore  | 729.4 μs | 10.67 μs | 9.98 μs | 78.1250 | 161.88 KB |


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
