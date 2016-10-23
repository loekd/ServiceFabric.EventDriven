using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using CustomerOrdersActor.Interfaces;
using OrderActor.Interfaces;

namespace CustomerOrdersActor
{
	[StatePersistence(StatePersistence.Persisted)]
	internal class CustomerOrdersActor : Actor, ICustomerOrdersActor
	{
		public CustomerOrdersActor(ActorService actorService, ActorId actorId)
			: base(actorService, actorId)
		{
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
	}
}
