
# EasyDatabase

## Description
Very simple document database for different uses.

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

The instance of Easy Database class can be a singleton, scoped or transient, it is not important, operations are thread-safe

### Simple

```csharp
var db = new Storage();
await db.AddOrUpdate<Animal>(new Animal());

var results = await db.Get<Animal>();

await db.Delete<Animal>(results);
```

### With custom configuration
Default values, can be overrided
```csharp
var documentsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Database");
var encoding = Encoding.UTF8;
var jsonSerializerSettings = new JsonSerializerSettings
	{
	    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
	    PreserveReferencesHandling = PreserveReferencesHandling.Objects
	};
	
var configuration = new Configuration(documentsPath, encoding, jsonSerializerSettings);

var db = new Storage(configuration);
...
```