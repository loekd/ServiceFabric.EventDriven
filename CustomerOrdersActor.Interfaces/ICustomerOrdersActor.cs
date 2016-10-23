using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using OrderActor.Interfaces;
using ServiceFabric.PubSubActors.Interfaces;

namespace CustomerOrdersActor.Interfaces
{
	public interface ICustomerOrdersActor : ISubscriberActor
	{
		Task<OrderCollection> GetOrdersAsync();
		
		Task AddOrderAsync(Order order);

		Task InitializeAsync();
	}

	[DataContract]
	public class OrderCollection
	{
		[DataMember]
		private List<Order> _copy;

		public ImmutableList<Order> Values { get; private set; }

		public OrderCollection(params Order[] values)
			:this(values as IList<Order>)
		{
		}

		public OrderCollection(IList<Order> values)
		{
			if (values == null)
			{
				Values = ImmutableList<Order>.Empty;
			}
			else
			{
				Values = values.ToImmutableList();
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			Values = _copy.ToImmutableList();
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			if (Values != null)
			{
				_copy = new List<Order>(Values);
			}
		}
	}
}
