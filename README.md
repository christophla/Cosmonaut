[![Build status](https://ci.appveyor.com/api/projects/status/au32jna62iue4wut?svg=true)](https://ci.appveyor.com/project/Elfocrash/cosmonaut) [![NuGet Package](https://img.shields.io/nuget/v/Cosmonaut.svg)](https://www.nuget.org/packages/Cosmonaut)

# What is Cosmonaut?

> The word was derived from "kosmos" (Ancient Greek: κόσμος) which means world/universe and "nautes" (Ancient Greek: ναῦς) which means sailor/navigator

Cosmonaut is an object mapper that enables .NET developers to work with a CosmosDB using .NET objects. It eliminates the need for most of the data-access code that developers usually need to write.

### Usage 
The idea is pretty simple. You can have one CosmoStore per entity (POCO/dtos etc)
This entity will be used to create a collection in the cosmosdb and it will offer all the data access for this object

Registering the CosmosStores in ServiceCollection for DI support
```csharp
 var cosmosSettings = new CosmosStoreSettings("<<databaseName>>", 
    "<<cosmosUri>>"), 
    "<<authkey>>");
                
serviceCollection.AddCosmosStore<Book>(cosmosSettings);
```

##### Adding an entity in the entity store
```csharp
var newUser = new User
{
    Name = "Nick"
};
var added = await cosmoStore.AddAsync(newUser);

var multiple = await cosmoStore.AddRangeAsync(manyManyUsers);
```

##### Quering for entities
```csharp
var user = await cosmoStore.FirstOrDefaultAsync(x => x.Username == "elfocrash");
var users = await cosmoStore.ToListAsync(x => x.HairColor == HairColor.Black);
```

##### Updating entities
When it comes to updating you have two options.

Update...
```csharp
await cosmoStore.UpdateAsync(entity);
```

... and Upsert
```csharp
await cosmoStore.UpsertAsync(entity);
```

The main difference is of course in the functionality.
Update will only update if the item you are updating exists in the database with this id.
Upsert on the other hand will either add the item if there is no item with this id or update it if an item with this id exists.

##### Removing entities
```csharp
await cosmoStore.RemoveAsync(x => x.Name == "Nick"); // Removes all the entities that match the criteria
await cosmoStore.RemoveAsync(entity);// Removes the specific entity
await cosmoStore.RemoveByIdAsync("<<anId>>");// Removes an entity with the specified ID
```

#### Collection sharing
Cosmonaut is all about making the integration with CosmosDB easy as well as making things like cost optimisation part of the library.

That's why Cosmonaut support collection sharing between different types of entities.

Why would you do that?

Cosmos is charging you based on how many RU/s your individual collection is provisioned at. This means that if you don't need to have one collection per entity because you won't use it that much, even on the minimum 400 RU/s, you will be charged money. That's where the magic of schemaless comes in.

How can you do that?

Well it's actually pretty simple. Just implement the `ISharedCosmosEntity` interface and decorate your object with the `SharedCosmosCollection` attribute.

The attribute accepts two properties, `SharedCollectionName` which is mandatory and `EntityPrefix` which is optional.
The `SharedCollectionName` property will be used to name the collection that the entity will share with other entities. 

The `EntityPrefix` will be used to make the object identifiable for Cosmosnaut. Be default it will pluralize the name of the class, but you can specify it to override this behavior.

Once you set this up you can add individual CosmosStores with shared collections.

Something worths noting is that because you will use this to share objects partitioning will be virtually impossible. For that reason the `id` will be used as a partition key by default as it is the only property that will be definately shared between all objects.


#### Indexing
By default CosmosDB is created with the following indexing rules

```javascript
{
    "indexingMode": "consistent",
    "automatic": true,
    "includedPaths": [
        {
            "path": "/*",
            "indexes": [
                {
                    "kind": "Range",
                    "dataType": "Number",
                    "precision": -1
                },
                {
                    "kind": "Hash",
                    "dataType": "String",
                    "precision": 3
                }
            ]
        }
    ],
    "excludedPaths": []
}
```

Indexing in necessary for things like querying the collections.
Keep in mind that when you manage indexing policy, you can make fine-grained trade-offs between index storage overhead, write and query throughput, and query consistency.

For example if the String datatype is Hash then exact matches like the following,
`cosmoStore.FirstOrDefaultAsync(x => x.SomeProperty.Equals($"Nick Chapsas")`
will return the item if it exists in CosmosDB but 
`cosmoStore.FirstOrDefaultAsync(x => x.SomeProperty.StartsWith($"Nick Ch")`
will throw an error. Changing the Hash to Range will work.

More about CosmosDB Indexing [here](https://docs.microsoft.com/en-us/azure/cosmos-db/indexing-policies)

#### Partitioning
Cosmonaut supports partitions out of the box. You can specify which property you want to be your Partition Key by adding the `[CosmosPartitionKey]` attribute above it.

Unless you really know what you're doing, it is recommended make your `Id` property the Partition Key. This will enable random distribution for your collection.

If you do not set a Partition Key then the collection created will be single partition. Here is a quote from Microsoft about single partition collections: 
> Single-partition collections have lower price options and the ability to execute queries and perform transactions across all collection data. They have the scalability and storage limits of a single partition (10GB and 10,000 RU/s). You do not have to specify a partition key for these collections. For scenarios that do not need large volumes of storage or throughput, single partition collections are a good fit.
[link](https://azure.microsoft.com/en-gb/blog/10-things-to-know-about-documentdb-partitioned-collections/)

##### Known hiccups
Partitions are great but you should these 3 very important things about them and about the way Cosmonaut will react.

* Once a collection is created with a partition key, it cannot be removed or changed.
* You cannot add a partition key later to a single partition collection.
* If you use the Update or the Upsert methods to update an entity that had the value of the property that is the partition key changed, then CosmosDB won't update the document but instead it will create a whole different document with the same id but the changed partition key value.

There is a plan however to deal with this on the Update method eventually.

More on the third issue here [Unique keys in Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/unique-keys)

#### Collection naming
Your collections will automatically be named based on the plural of the object you are using in the generic type.
However you can override that by decorating the class with the `CosmosCollection` attribute.

Example:
```csharp
[CosmosCollection("somename")]
```

#### Performance
Performance can vary dramatically based on the throughput (RU/s*) you are using.
By default Cosmonaut will set the throughput to the lowest value of `400` mainly because I don't want to affect how much you pay accidentaly.
You can set the default throughput for all the collections when you set up your `CosmosStore` by setting the `CollectionThroughput` option to whatever you see fit or by simply setting it in Azure.
You can also set the throughput at the collection level by using the `CosmosCollection` attribute at the entity's class.

Example:
```csharp
[CosmosCollection(Throughput = 1000)]
```
Note here that this functionality is disabled by default. Usage of Azure to adjust is recommended.

#### Benchmarks

##### Averages of 1000 iterations for 500 documents per operation on collection with default indexing and 5000 RU/s (POCO serialization)

| Operation used | Duration |
| ------------- |:-------------:|
| AddRangeAsync | 596.5ms |
| ToListAsync |23.1ms|
| UpdateRangeAsync |653.6ms|
| UpsertRangeAsync |620.2ms|
| RemoveAsync | 502.2ms |

##### Averages of 10000 iterations for 1 document per operation on collection with default indexing and 5000 RU/s (POCO serialization)
| Operation used | Duration |
| ------------- |:-------------:|
| AddAsync | 3.9433ms |
| FirstOrDefaultAsync | 2.7492ms |
| UpdateAsync | 4.1562ms |
| UpsertAsync | 4.1842ms |
| RemoveAsync | 3.9682ms |

### Restrictions
Because of the way the internal `id` property of Cosmosdb works, there is a mandatory restriction made.
You cannot have a property named Id or a property with the attribute `[JsonProperty("id")]` without it being a string.
A cosmos id need to exist somehow on your entity model. For that reason if it isn't part of your entity you can just implement the `ICosmosEntity` interface.
