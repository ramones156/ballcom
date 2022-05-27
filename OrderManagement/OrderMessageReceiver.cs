﻿using BallCore.Events;
using BallCore.RabbitMq;
using OrderManagement.DataAccess;
using OrderManagement.Models;
using RabbitMQ.Client;

namespace OrderManagement;

public class OrderMessageReceiver : MessageReceiver
{
    private readonly OrderManagementDbContext _dbContext;
    private readonly IMessageSender _rmq;

    public OrderMessageReceiver(IConnection connection, OrderManagementDbContext dbContext, IMessageSender rmq) :
        base(connection, new[] { "order_management" })
    {
        _dbContext = dbContext;
        _rmq = rmq;
    }

    protected override Task HandleMessage(IEvent e)
    {
        Console.WriteLine("Received message");
        if (e is DomainEvent de)
            switch (de.Payload)
            {
                case Order c:
                {
                    Console.WriteLine(
                        $"Received ex: {de.UseExchange} {de.Type} message ({de.Name}) from {de.Destination} : {c.ArrivalAdress}");
                    if (de.Type == EventType.Updated)
                    {
                        // Update het order.
                        var existingOrder = _dbContext.Orders.FirstOrDefault(o => o.OrderId == c.OrderId);
                        existingOrder.StatusProcess = c.StatusProcess ?? existingOrder.StatusProcess;

                        _dbContext.Orders.Update(existingOrder);
                        _dbContext.SaveChanges();

                        _rmq.Send(new DomainEvent(existingOrder, EventType.Updated, "order_exchange_order", true));
                    }

                    break;
                }

                case Customer c:
                {
                    Console.WriteLine(
                        $"Received ex: {de.UseExchange} {de.Type} message ({de.Name}) from {de.Destination} : {c.Email}");
                    if (de.Type == EventType.Created)
                    {
                        var customer = new Customer
                        {
                            CustomerId = c.CustomerId,
                            Email = c.Email
                        };
                        _dbContext.Customers.Add(customer);
                        _dbContext.SaveChanges();
                        break;
                    }

                    if (de.Type == EventType.Deleted)
                    {
                        var customer = _dbContext.Customers.FirstOrDefault(cu => cu.CustomerId == c.CustomerId);
                        if (customer == null) break;

                        _dbContext.Customers.Remove(customer);
                        _dbContext.SaveChanges();
                        break;
                    }

                    if (de.Type == EventType.Updated)
                    {
                        var customer = _dbContext.Customers.FirstOrDefault(cu => cu.CustomerId == c.CustomerId);
                        if (customer == null) break;

                        customer.Email = c.Email;
                        _dbContext.Customers.Update(customer);
                        _dbContext.SaveChanges();
                    }

                    break;
                }

                case Product c:
                {
                    Console.WriteLine(
                        $"Received ex: {de.UseExchange} {de.Type} message ({de.Name}) from {de.Destination} : {c.Name}");
                    if (de.Type == EventType.Created)
                    {
                        // Add het product toe.
                        _dbContext.Products.Add(c);
                        _dbContext.SaveChanges();
                        break;
                    }

                    if (de.Type == EventType.Deleted)
                    {
                        // Remove het product
                        _dbContext.Products.Remove(c);
                        _dbContext.SaveChanges();
                        break;
                    }

                    if (de.Type == EventType.Updated)
                    {
                        // Update het product
                        _dbContext.Products.Update(c);
                        _dbContext.SaveChangesAsync();
                    }

                    break;
                }
            }

        return Task.CompletedTask;
    }
}