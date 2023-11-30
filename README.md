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
- LtQuery 1.0.0, Dapper 2.1.24, EFCore 7.0.13

## SelectOne from Single table

Result of `connection.Single(Lt.Query<Blog>().Where(_ => _.Id == 1))`

| ORM  | Mean      | Error    | StdDev   | Gen0    | Allocated |
|-------- |----------:|---------:|---------:|--------:|----------:|
| ADO.NET |  87.05 μs | 0.457 μs | 0.405 μs |  87.23 μs |  2.5635 |    5.3 KB |
| **LtQuery** |  **86.66 μs** | **1.512 μs** | **1.414 μs** |  **86.97 μs** |  **2.5635** |   **5.38 KB** |
| Dapper  |  90.27 μs | 1.800 μs | 5.193 μs |  87.69 μs |  2.8076 |   5.83 KB |
| EFCore  | 264.16 μs | 1.883 μs | 1.669 μs | 263.51 μs | 36.1328 |  74.47 KB |

## SelectMany(20) from Single table

Result of `connection.Select(Lt.Query<Blog>().Take(20))`

| ORM  | Mean     | Error   | StdDev  | Gen0    | Allocated |
|-------- |---------:|--------:|--------:|--------:|----------:|
| ADO.NET | 200.5 us | 3.85 us | 4.11 us | 32.7148 |   67.2 KB |
| **LtQuery** | **201.1 us** | **3.88 us** | **4.16 us** | **32.7148** |  **67.23 KB** |
| Dapper  | 207.5 us | 2.55 us | 2.38 us | 34.1797 |  69.88 KB |
| EFCore  | 360.8 us | 2.43 us | 2.27 us | 69.3359 | 142.51 KB |

## SelectMany(20) from With children

Result of `connection.Select(Lt.Query<Blog>().Include(_ => _.Posts).Take(20))`

| ORM  | Mean     | Error     | StdDev    | Gen0     | Gen1     | Allocated |
|-------- |---------:|----------:|----------:|---------:|---------:|----------:|
| ADO.NET | 4.594 ms | 0.0793 ms | 0.0742 ms | 289.0625 | 195.3125 |   1.44 MB |
| **LtQuery** | **4.602 ms** | **0.0839 ms** | **0.0785 ms** | **304.6875** | **179.6875** |   **1.44 MB** |
| Dapper  | 4.718 ms | 0.0735 ms | 0.0651 ms | 351.5625 | 195.3125 |   1.62 MB |
| EFCore  | 7.007 ms | 0.0736 ms | 0.0688 ms | 554.6875 | 367.1875 |    2.6 MB |

## AddRange(10)

Result of `connection.AddRange(blogs)`

| Method  | Mean       | Error    | StdDev   | Gen0    | Allocated |
|-------- |-----------:|---------:|---------:|--------:|----------:|
| ADO.NET |   361.4 us |  6.49 us |  8.44 us |  3.9063 |   8.09 KB |
| **LtQuery** |   **351.0 us** |  **5.51 us** |  **5.16 us** |  **6.3477** |  **13.73 KB** |
| Dapper  |   365.9 us |  5.79 us |  5.42 us |  5.8594 |  12.67 KB |
| EFCore  | 1,001.4 us | 20.01 us | 41.78 us | 78.1250 | 161.88 KB |


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
