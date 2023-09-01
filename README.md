# What's this?

I'm undertaking another project to create a product catalogue to run in Azure.

This is a small POC project to create a Redis cache as a proxy to see how the mechanics of this will work.

The configuration here uses the Cosmos DB Emulator and a local Redis server. The Cosmos DB needs the following database
and container created ahead of time:

Database name: "products"
Container name: "things"
Container partition key: "/id"

The Cosmos container won't be cleared out before / after each run, so if you don't clear it out yourself, you'll get
extra items reported in the count. 

The CosmosStorage class doesn't ensure that the database and container are created either. Ideally, we'd want to 
ensure they are constructed before the CosmosStorage class is available. One way of doing this would be with a proxy
that is responsible for ensuring they are created and prevents access to the underlying CosmosStorage class until they 
are available.
