using BallCore.Events;
using BallCore.RabbitMq;
using Orderpicker.DataAccess;
using Orderpicker.Models;
using RabbitMQ.Client;

namespace Orderpicker;

public class OrderPickerReceiver : MessageReceiver
{
    private readonly OrderpickerDbContext _dbContext;

    public OrderPickerReceiver(OrderpickerDbContext dbContext, IConnection connection) : base(connection,
        new[] { "orderpicker_client", "general" })
    {
        _dbContext = dbContext;
    }

    // Example of how to handle message
    protected override Task HandleMessage(IEvent e)
    {
        Console.WriteLine("Received message");

        if (e is DomainEvent de)
            switch (de.Payload)
            {
                case Product c:
                {
                    Console.WriteLine(
                        $"Received ex: {de.UseExchange} {de.Type} message ({de.Name}) from {de.Destination} : {c.Name}");
                    if (de.Type == EventType.Created) _dbContext.Set<Product>().Add(c);

                    break;
                }
                case Order o:
                {
                    Console.WriteLine(
                        $"Received ex: {de.UseExchange} {de.Type} message ({de.Name}) from {de.Destination}");
                    if (de.Type == EventType.Created) _dbContext.Set<Order>().Add(o);

                    break;
                }
            }

        _dbContext.SaveChanges();

        return Task.CompletedTask;
    }
}