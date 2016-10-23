using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using OrderActor.Interfaces;

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

		public Task PlaceOrderAsync(Order order)
		{
			return StateManager.AddOrUpdateStateAsync("info", order, (key, old) => order);
		}
	}
}
