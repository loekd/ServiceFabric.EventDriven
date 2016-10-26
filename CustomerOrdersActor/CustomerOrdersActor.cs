using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using CustomerOrdersActor.Interfaces;
using OrderActor.Interfaces;
using ServiceFabric.PubSubActors.Helpers;
using ServiceFabric.PubSubActors.Interfaces;
using ServiceFabric.PubSubActors.SubscriberActors;

namespace CustomerOrdersActor
{
	[ActorService(Name = nameof(ICustomerOrdersActor))]
	[StatePersistence(StatePersistence.Persisted)]
	internal class CustomerOrdersActor : Actor, ICustomerOrdersActor
	{
		private readonly ISubscriberActorHelper _subscriberActorHelper;

		public CustomerOrdersActor(ActorService actorService, ActorId actorId, ISubscriberActorHelper subscriberActorHelper = null)
			: base(actorService, actorId)
		{
			_subscriberActorHelper = subscriberActorHelper ?? new SubscriberActorHelper();
		}

		public async Task<OrderCollection> GetOrdersAsync()
		{
			if (!(await StateManager.ContainsStateAsync("info")))
			{
				return new OrderCollection();
			}
			var info = await StateManager.GetStateAsync<OrderCollection>("info");
			return info;
		}

		public Task AddOrderAsync(Order order)
		{
			return StateManager.AddOrUpdateStateAsync<OrderCollection>("info"
				, new OrderCollection(order)
				, (key, old) => new OrderCollection(old.Values.Add(order)));
		}

		public Task InitializeAsync()
		{
			return _subscriberActorHelper.RegisterMessageTypeAsync(this, typeof(OrderCreatedEvent)); //register as subscriber for this type of events
		}

		public Task ReceiveMessageAsync(MessageWrapper message)
		{
			var payload = this.Deserialize<Order>(message);
			if (payload.CustomerId == Id.GetGuidId())
			{
				return AddOrderAsync(payload);
			}

			return Task.FromResult(false);
		}
	}
}
