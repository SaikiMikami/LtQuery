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
| ADO.NET |  91.19 μs | 0.923 μs | 0.819 μs |  2.6855 |   5.59 KB |
| **LtQuery** |  **87.38 μs** | **0.612 μs** | **0.572 μs** |  **2.6855** |   **5.67 KB** |
| Dapper  |  90.76 μs | 0.986 μs | 0.922 μs |  2.8076 |   5.83 KB |
| EFCore  | 277.68 μs | 3.422 μs | 3.201 μs | 36.1328 |  74.47 KB |

## SelectMany(20) from Single table

Result of `connection.Select(Lt.Query<Blog>().Take(20))`

| ORM     | Mean     | Error   | StdDev  | Gen0    | Allocated |
|-------- |---------:|--------:|--------:|--------:|----------:|
| ADO.NET | 196.0 μs | 1.42 μs | 1.33 μs | 32.9590 |  67.48 KB |
| **LtQuery** | **193.4 μs** | **1.82 μs** | **1.70 μs** | **32.9590** |  **67.52 KB** |
| Dapper  | 198.1 μs | 2.30 μs | 2.04 μs | 34.1797 |  69.88 KB |
| EFCore  | 435.2 μs | 4.40 μs | 4.11 μs | 69.3359 | 142.52 KB |

## SelectMany(20) from With children

Result of `connection.Select(Lt.Query<Blog>().Include(_ => _.Posts).Take(20))`

| ORM     | Mean     | Error     | StdDev    | Gen0     | Gen1     | Allocated |
|-------- |---------:|----------:|----------:|---------:|---------:|----------:|
| ADO.NET | 4.506 ms | 0.0169 ms | 0.0158 ms | 281.2500 | 195.3125 |   1.44 MB |
| **LtQuery** | **4.473 ms** | **0.0570 ms** | **0.0533 ms** | **273.4375** | **210.9375** |   **1.44 MB** |
| Dapper  | 4.517 ms | 0.0268 ms | 0.0251 ms | 335.9375 | 218.7500 |   1.62 MB |
| EFCore  | 6.757 ms | 0.0898 ms | 0.0750 ms | 554.6875 | 367.1875 |    2.6 MB |

## AddRange(10)

Result of `connection.AddRange(blogs)`

| ORM     | Mean     | Error    | StdDev   | Gen0    | Allocated |
|-------- |---------:|---------:|---------:|--------:|----------:|
| ADO.NET | 392.9 μs |  7.67 μs |  9.13 μs |  6.3477 |  13.46 KB |
| **LtQuery** | **359.7 μs** |  **6.87 μs** |  **8.44 μs** |  **6.3477** |  **13.74 KB** |
| Dapper  | 395.9 μs |  7.85 μs | 13.33 μs |  9.7656 |   20.4 KB |
| EFCore  | 983.0 μs | 19.65 μs | 35.43 μs | 78.1250 | 161.88 KB |


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
