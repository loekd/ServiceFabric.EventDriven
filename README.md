# ServiceFabric.EventDriven
Event Driven Architecture using Service Fabric Actors - Sample


Open master branch
	- Customer
	- Order
	- Insight into both

Open CustomerOrders branch
	- Adds CustomerOrders, holds all the Customers' Orders
	- Insight into all customer orders
	- Requires knowledge of CustomerOrdersActor from within the OrderActor
	- CustomerOrders branch doesn't run

Open PubSubActors branch
	- Adds PubSubActors Nuget packages
	- Need to register as Subscriber and start Publishing events
	- Other subscribers can easily be added
	- Does not require knowledge of CustomerOrdersActor from within the OrderActor

Open MoreListeners branch
	- Adds ShippingActor
	- Is just another Subscriber 
	
