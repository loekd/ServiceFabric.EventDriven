using System;
using System.Threading;
using CustomerActor.Interfaces;
using CustomerOrdersActor.Interfaces;
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
					Console.WriteLine($"Adding new customer '{customerInfo.CustomerId}'...");
					customerActorProxy.SetCustomerInfoAsync(customerInfo).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				else
				{
					Console.WriteLine($"Using existing customer '{customerInfo.CustomerId}'...");
				}

				//register for events
				Console.WriteLine($"Start listening for changes for customer '{customerInfo.CustomerId}'...");
				var customerOrdersActorProxy = CreateCustomerOrdersActorProxy(customerInfo.CustomerId);
				customerOrdersActorProxy.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				

				Console.WriteLine("1: Show existing customer order.");
				Console.WriteLine("2: Create new customer order.");
				Console.WriteLine("3: Show all existing customer orders.");

				var key = Console.ReadKey(true);

				switch (key.Key)
				{
					case ConsoleKey.D1:
						ShowExistingOrderInfo();

						break;

					case ConsoleKey.D2:
						OrderProduct(customerInfo);
						break;

					case ConsoleKey.D3:
						ShowAllExistingOrderInfo(customerInfo);

						break;
				}
			}
		}
		
		private static void ShowExistingOrderInfo()
		{
			Console.WriteLine($"Enter orderid:");
			Guid orderId;
			string orderIdInput = Console.ReadLine();

			if (!Guid.TryParse(orderIdInput, out orderId))
			{
				Console.WriteLine("invalid input...");
				return;
			}

			var orderActorProxy = CreateOrderActorProxy(orderId);
			var orderInfo = orderActorProxy.GetOrderInfoAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			if (orderInfo != null)
			{
				Console.WriteLine(
					$"Found order: {orderInfo.Amount} of '{orderInfo.Product}' with orderId:\t'{orderInfo.OrderId}' for customer '{orderInfo.CustomerId}'");
			}
			else
			{
				Console.WriteLine(
					$"Found no order with orderId:\t'{orderId}'");
			}
			Console.WriteLine("Hit any key to continue...");
			Console.ReadKey(true);
		}

		private static void OrderProduct(Customer customerInfo)
		{
			Console.WriteLine("Enter product name to order:");
			var productName = Console.ReadLine();
			Console.WriteLine($"Enter amount of '{productName}' to order:");
			string amountInput = Console.ReadLine();
			int amount;
			if (!int.TryParse(amountInput, out amount))
			{
				amount = 1;
			}
			var orderId = Guid.NewGuid();
			var orderActorProxy = CreateOrderActorProxy(orderId);

			var order = new Order(orderId, customerInfo.CustomerId, productName, amount);
			orderActorProxy.PlaceOrderAsync(order).ConfigureAwait(false).GetAwaiter().GetResult();

			Console.WriteLine($"Placed order: {amount} of '{productName}' with orderId:\t'{orderId}'");
			Console.WriteLine("Copy the orderid. Hit any key to continue...");
			Console.ReadKey(true);
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

		private static void ShowAllExistingOrderInfo(Customer customerInfo)
		{
			var customerOrdersActorProxy = CreateCustomerOrdersActorProxy(customerInfo.CustomerId);
			var orderInfos = customerOrdersActorProxy.GetOrdersAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			if (orderInfos?.Values != null && orderInfos.Values.Count > 0)
			{
				foreach (var orderInfo in orderInfos.Values)
				{
					Console.WriteLine(
						$"Found order: {orderInfo.Amount} of '{orderInfo.Product}' with orderId:\t'{orderInfo.OrderId}' for customer '{orderInfo.CustomerId}'");
				}
			}
			else
			{
				Console.WriteLine($"Found no orders for customer '{customerInfo.CustomerId}'");
			}
			Console.WriteLine("Hit any key to continue...");
			Console.ReadKey(true);
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

		private static ICustomerOrdersActor CreateCustomerOrdersActorProxy(Guid customerId)
		{
			retry:
			try
			{
				return ActorProxy.Create<ICustomerOrdersActor>(new ActorId(customerId), "fabric:/ServiceFabric.EventDriven", nameof(ICustomerOrdersActor));
			}
			catch
			{
				Thread.Sleep(500);
				goto retry;
			}
		}
	}
}
