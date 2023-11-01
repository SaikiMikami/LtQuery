# LtQuery

## About

LtQuery is a ORM focus on Easy-to-use and Performance. 

LtQuery does not accept the input of SQL which is a string.
Instead, call giving a diverty, tiny query object.

## How to Use

```csharp
// setup DI Container
var collection = new ServiceCollection();
collection.AddLtQuerySqlServer();
collection.AddSingleton<IModelConfiguration, ModelConfiguration>();	// User-defined ModelConfiguration
collection.AddScoped<DbConnection>(_ => new SqlConnection(/*ConnectionString*/);
var provider = collection.BuildServiceProvider();

using(var scope = provider.CreateScope())
{
	// get ILtConnection
	var connection = scope.ServiceProvider.GetRequiredService<ILtConnection>();

	// create query object
	var query = Lt.Query<Blog>().Include(_ => _.Posts).Where(_ => _.UserId == Lt.Arg<int>("UserId")).OrderBy(_ => _.Date).Take(20);

	// execute query
	var blogs = connection.Query(query, new { UserId = 5 });
}

```

## Install

```powershell
dotnet add package LtQuery
```
