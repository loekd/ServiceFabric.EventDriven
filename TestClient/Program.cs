using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using OrderActor.Interfaces;

namespace TestClient
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				Console.Clear();
				Console.WriteLine("Enter customer name:");

				string customerName = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(customerName))
				{
					continue;
				}

				
				Console.WriteLine("Making sure that the customer exists...");
				var customerActorProxy = CreateCustomerActorProxy(customerName);
				var customerInfo = customerActorProxy.GetCustomerInfoAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (customerInfo == null)
				{
					customerInfo = new Customer(Guid.NewGuid(), customerName);
					Console.WriteLine("Adding new customer '{customerInfo.CustomerId}'...");
					customerActorProxy.SetCustomerInfoAsync(customerInfo).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				else
				{
					Console.WriteLine($"Using existing customer '{customerInfo.CustomerId}'...");
				}

				Console.WriteLine("1: Show existing customer orders.");
				Console.WriteLine("2: Create new customer order.");

				var key = Console.ReadKey(true);
				Guid orderId;
				IOrderActor orderActorProxy;

				switch (key.Key)
				{
					case ConsoleKey.D1:
						Console.WriteLine($"Enter orderid:");
						string orderIdInput = Console.ReadLine();
						
						if (!Guid.TryParse(orderIdInput, out orderId))
						{
							Console.WriteLine("invalid input...");
							continue;
						}

						orderActorProxy = CreateOrderActorProxy(orderId);
						var orderInfo = orderActorProxy.GetOrderInfoAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						Console.WriteLine($"Found order: {orderInfo.Amount} of '{orderInfo.Product}' with orderId:\t'{orderInfo.OrderId}' for customer '{orderInfo.CustomerId}'");

						Console.WriteLine("Hit any key to continue...");
						Console.ReadKey(true);

						break;
					
					case ConsoleKey.D2:
						Console.WriteLine("Enter product name to order:");
						var productName = Console.ReadLine();
						Console.WriteLine($"Enter amount of '{productName}' to order:");
						string amountInput = Console.ReadLine();
						int amount;
						if (!int.TryParse(amountInput, out amount))
						{
							amount = 1;
						}
						orderId = Guid.NewGuid();
						orderActorProxy = CreateOrderActorProxy(orderId);

						Order order = new Order(orderId, customerInfo.CustomerId, productName, amount);
						orderActorProxy.PlaceOrderAsync(order).ConfigureAwait(false).GetAwaiter().GetResult();

						Console.WriteLine($"Placed order: {amount} of '{productName}' with orderId:\t'{orderId}'");
						Console.WriteLine("Copy the orderid. Hit any key to continue...");
						Console.ReadKey(true);
						break;
				}
			}
		}

		private static ICustomerActor CreateCustomerActorProxy(string customerName)
		{
			retry:
			try
			{
				return ActorProxy.Create<ICustomerActor>(new ActorId(customerName), "fabric:/ServiceFabric.EventDriven");
			}
			catch 
			{
				Thread.Sleep(500);
				goto retry;
			}
		}

		private static IOrderActor CreateOrderActorProxy(Guid orderId)
		{
			retry:
			try
			{
				return ActorProxy.Create<IOrderActor>(new ActorId(orderId), "fabric:/ServiceFabric.EventDriven");
			}
			catch
			{
				Thread.Sleep(500);
				goto retry;
			}
		}
	}
}
