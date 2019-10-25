
# EasyDatabase

## Description
Straightforward database which helps you store data with minimal overhead.

## Usage
All POCO entities must implement interface '**IEntity**'

```csharp
public class Animal : IEntity
{
	// From IEntity
	public Guid Id { get; set; } = Guid.NewGuid();

	public int NumberOfLegs { get; set; }
}
```
#### ASP.NET Core
Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.UseEasyDatabase();
}
```
Test.cs
```csharp
public class Test
{
    private readonly Storage _storage;

    public Test(Storage storage)
    {
        _storage = storage;
    }
}
```
#### Others
Test.cs
```csharp
public class Test
{
    private readonly Storage _storage;

    public Test()
    {
        _storage = StorageFactory.GetStorage();
    }
}
```

### Example

```csharp
public class Test
{
    private readonly Storage _storage;

    public Test(Storage storage)
    {
        _storage = storage;
    }

    public async Task SomeMethod()
    {
        await _storage.AddOrUpdate(new Animal());

        var results = await _storage.Get<Animal>();

        await _storage.Delete<Animal>(results);
    }
}
```

### Configuration
While initialization you can override default **configuration** and **repository**.

Lazy caching can be enabled if necessary.
```csharp
public class Configuration
{
    public Configuration(double? cacheSlidingExpirationTimeInHours = null);

    public double? CacheSlidingExpirationTimeInHours { get; }
}
```

Is also possible to implement your own repository over interface **IRepository**. As default is used **FileRepository** which store data in JSON files in your project.
```csharp
public interface IRepository
{
    Task<T> ReadEntity<T>(Guid id) where T : IEntity;
    Task<T> ReadEntity<T>(string id) where T : IEntity;
    Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity;
    Task WriteEntity<T>(T entity) where T : IEntity;
    Task DeleteEntity<T>(Guid id);
}
```