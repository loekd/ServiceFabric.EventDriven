using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using OrderActor.Interfaces;
using ServiceFabric.PubSubActors.Helpers;

namespace OrderActor
{
	[StatePersistence(StatePersistence.Persisted)]
	internal class OrderActor : Actor, IOrderActor
	{
		private readonly IPublisherActorHelper _publisherActorHelper;

		public OrderActor(ActorService actorService, ActorId actorId, IPublisherActorHelper publisherActorHelper = null)
			: base(actorService, actorId)
		{
			_publisherActorHelper = publisherActorHelper ?? new PublisherActorHelper();
		}

		public async Task<Order> GetOrderInfoAsync()
		{
			if (!(await StateManager.ContainsStateAsync("info")))
			{
				return null;
			}
			var info = await StateManager.GetStateAsync<Order>("info");
			return info;
		}

		public async Task PlaceOrderAsync(Order order)
		{
			//update state
			await StateManager.AddOrUpdateStateAsync("info", order, (key, old) => order);

			//publish a notification about the state change
			await _publisherActorHelper.PublishMessageAsync(this, order);
			
			//update aggregate
			//var customerOrderActorProxy = CreateCustomerOrdersActorProxy(order.CustomerId);
			//await customerOrderActorProxy.AddOrderAsync(order);
		}

		//no longer need to know all interested parties:

		//private static ICustomerOrdersActor CreateCustomerOrdersActorProxy(Guid customerId)
		//{
		//	retry:
		//	try
		//	{
		//		return ActorProxy.Create<ICustomerOrdersActor>(new ActorId(customerId));
		//	}
		//	catch
		//	{
		//		Thread.Sleep(500);
		//		goto retry;
		//	}
		//}
	}
}
