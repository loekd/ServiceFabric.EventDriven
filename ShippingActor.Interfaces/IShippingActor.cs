using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using OrderActor.Interfaces;
using ServiceFabric.PubSubActors.Interfaces;

namespace ShippingActor.Interfaces
{
	public interface IShippingActor : ISubscriberActor
	{
		Task InitializeAsync();

		Task AddOrderAsync(Order order);

		Task<Shipping> GetShippingInfoAsync(Guid orderId);
	}

	[DataContract]
	public class Shipping
	{
		[DataMember]
		public readonly string Product;

		[DataMember]
		public readonly int Amount;

		[DataMember]
		public readonly Guid CustomerId;

		[DataMember]
		public readonly Guid OrderId;

		[DataMember]
		public readonly string Status;

		public Shipping(string product, int amount, Guid customerId, Guid orderId)
		{
			Product = product;
			Amount = amount;
			CustomerId = customerId;
			OrderId = orderId;
			Status = ((new Random().Next(0, 10)) % 2 == 0) ? "Ready to ship" : "Awaiting confirmation";
		}
	}
}
