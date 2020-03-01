# EasyDatabase

## Description
Straightforward database for .NET applications which helps you store data with minimal overhead.

#### Azure blob storage

Data are stored in the blob storage in the Azure platform

Package: **[EasyDatabase.AzureBlobRepository](https://www.nuget.org/packages/EasyDatabase.AzureBlobRepository)**

#### Local file storage

Data are stored locally in the file system

Package: **[EasyDatabase.FileRepository](https://www.nuget.org/packages/EasyDatabase.FileRepository)**


## Usage
All POCO entities must implement interface '**IEntity**'

```csharp
public class Animal : IEntity
{
	// From IEntity
	public Guid Id { get; set; } = Guid.NewGuid();

	public int NumberOfLegs { get; set; } = 5;
}
```
#### ASP.NET Core
Install package **EasyDatabase.Extensions**
```csharp
public void ConfigureServices(IServiceCollection services)
{
	// Local file storage
    services.UseEasyDatabase(new FileRepository());
    
    // Azure blob storage
    services.UseEasyDatabase(new AzureBlobRepository("ConnectionString", "ContainerName");
}
```
Inject **Storage**  to your own class
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
Use static factory class **StorageFactory**
```csharp
public class Test
{
    private readonly Storage _fileStorage;
    private readonly Storage _azureBlobStorage;

    public Test()
    {
        _fileStorage = StorageFactory.GetStorage(new FileRepository());
        
        _azureBlobStorage = StorageFactory.GetStorage(new AzureBlobRepository("ConnectionString", "ContainerName"));
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
    	var entity = new Animal();
    	
        await _storage.AddOrUpdate(entity);
		
		// Specific entity
		var result = await _storage.Get<Animal>(entity.Id);
		
		// All entities of type 'Animal'
        var results = await _storage.Get<Animal>();

        await _storage.Delete<Animal>(result);
    }
}
```