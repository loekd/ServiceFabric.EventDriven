using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace CustomerActor.Interfaces
{
	public interface ICustomerActor : IActor
	{
		Task<Customer> GetCustomerInfoAsync();

		
		Task SetCustomerInfoAsync(Customer customerInfo);
	}

	[DataContract]
	public class Customer
	{
		[DataMember]
		public readonly Guid CustomerId;

		[DataMember]
		public readonly string Name;

		public Customer(Guid customerId, string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

			Name = name;
			CustomerId = customerId;
		}
	}
}
