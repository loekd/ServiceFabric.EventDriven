using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace OrderActor.Interfaces
{
	public interface IOrderActor : IActor
	{
		Task<Order> GetOrderInfoAsync();

		Task PlaceOrderAsync(Order order);
	}

	[DataContract]
	public class Order
	{
		[DataMember]
		public readonly Guid OrderId;

		[DataMember]
		public readonly Guid CustomerId;

		[DataMember]
		public readonly string Product;

		[DataMember]
		public readonly int Amount;

		public Order(Guid orderId, Guid customerId, string product, int amount)
		{
			if (product == null) throw new ArgumentNullException(nameof(product));
			if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

			OrderId = orderId;
			Product = product;
			Amount = amount;
			CustomerId = customerId;
		}
	}

	//[DataContract]
	//public class OrderCollection
	//{
	//	[DataMember]
	//	public readonly ImmutableList<Order> Values;

	//	public OrderCollection(IEnumerable<Order> values)
	//	{
	//		if (values == null) throw new ArgumentNullException(nameof(values));
	//		Values = ImmutableList<Order>.Empty.AddRange(values);
	//	}
	//}
}
