using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using OrderActor.Interfaces;
using CustomerOrdersActor.Interfaces;

namespace OrderActor
{
	[StatePersistence(StatePersistence.Persisted)]
	internal class OrderActor : Actor, IOrderActor
	{
		public OrderActor(ActorService actorService, ActorId actorId)
			: base(actorService, actorId)
		{
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

			//update aggregate
			var customerOrderActorProxy = CreateCustomerOrdersActorProxy(order.CustomerId);
			await customerOrderActorProxy.AddOrderAsync(order);
		}

		private static ICustomerOrdersActor CreateCustomerOrdersActorProxy(Guid customerId)
		{
			retry:
			try
			{
				return ActorProxy.Create<ICustomerOrdersActor>(new ActorId(customerId));
			}
			catch
			{
				Thread.Sleep(500);
				goto retry;
			}
		}
	}
}
