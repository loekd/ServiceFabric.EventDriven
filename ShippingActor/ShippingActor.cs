using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using OrderActor.Interfaces;
using ServiceFabric.PubSubActors.Helpers;
using ServiceFabric.PubSubActors.Interfaces;
using ServiceFabric.PubSubActors.SubscriberActors;
using ShippingActor.Interfaces;

namespace ShippingActor
{
	[ActorService(Name = nameof(IShippingActor))]
	[StatePersistence(StatePersistence.Persisted)]
	internal class ShippingActor : Actor, IShippingActor
	{
		private readonly ISubscriberActorHelper _subscriberActorHelper;

		public ShippingActor(ActorService actorService, ActorId actorId, ISubscriberActorHelper subscriberActorHelper = null)
			: base(actorService, actorId)
		{
			_subscriberActorHelper = subscriberActorHelper ?? new SubscriberActorHelper();
		}

		public Task InitializeAsync()
		{
			return _subscriberActorHelper.RegisterMessageTypeAsync(this, typeof(Order)); //register as subscriber for this type of messages
		}

		public Task ReceiveMessageAsync(MessageWrapper message)
		{
			var payload = this.Deserialize<Order>(message);
			if (payload.OrderId == Id.GetGuidId())
			{
				return AddOrderAsync(payload);
			}

			return Task.FromResult(false);
		}

		public Task AddOrderAsync(Order order)
		{
			var shipping = new Shipping(order.Product, order.Amount, order.CustomerId, order.OrderId);
			return StateManager.AddOrUpdateStateAsync("info"
				, shipping
				, (key, old) => shipping);
		}

		public async Task<Shipping> GetShippingInfoAsync(Guid orderId)
		{
			if (!(await StateManager.ContainsStateAsync("info")))
			{
				return null;
			}
			var info = await StateManager.GetStateAsync<Shipping>("info");
			return info;
		}
	}
}
