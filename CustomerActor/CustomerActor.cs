using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using CustomerActor.Interfaces;

namespace CustomerActor
{
	[StatePersistence(StatePersistence.Persisted)]
	internal class CustomerActor : Actor, ICustomerActor
	{
		public CustomerActor(ActorService actorService, ActorId actorId)
			: base(actorService, actorId)
		{
		}

		public async Task<Customer> GetCustomerInfoAsync()
		{
			if (!(await StateManager.ContainsStateAsync("info")))
			{
				return null;
			}
			var info = await StateManager.GetStateAsync<Customer>("info");
			return info;
		}

		public Task SetCustomerInfoAsync(Customer customerInfo)
		{
			return StateManager.AddOrUpdateStateAsync("info", customerInfo, (key, old) => customerInfo);
		}		
	}
}
