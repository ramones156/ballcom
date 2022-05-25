﻿using CustomerManagement.Models;
using CustomerManagement.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BallCore.RabbitMq;
using BallCore.Events;

namespace CustomerManagement.Controllers;

[Route("/api/[controller]")]
public class CustomersController : Controller
{
    CustomerManagementDbContext _dbContext;
    IMessageSender _messageSender;

    public CustomersController(CustomerManagementDbContext dbContext, IMessageSender messageSender)
    {
        _dbContext = dbContext;
        _messageSender = messageSender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var customers = await _dbContext.Set<Customer>().Select(
            x => new CustomerViewModel
            {
                City = x.City,
                Address = x.Address,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToListAsync();

        return Ok(await _dbContext.Customers.ToListAsync());
    }

    [HttpGet]
    [Route("{customerId}", Name = "GetByCustomerId")]
    public async Task<IActionResult> GetByCustomerId(int customerId)
    {
        var customer = await _dbContext
            .Set<Customer>()
            .FindAsync(customerId);

        if (customer == null)
        {
            return NotFound("Couldn't find customer");
        }

        return Ok(new CustomerViewModel
        {
            Email = customer.Email,
            Address = customer.Address,
            City = customer.City,
            FirstName = customer.FirstName,
            LastName = customer.LastName
        });
    }

    [HttpDelete]
    [Route("delete/{id}", Name = "Delete customer")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var customer = await _dbContext
            .Set<Customer>()
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
        {
            return NotFound("Couldn't find customer");
        }
        return Ok(customer);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("add")]
    public async Task<IActionResult> RegisterAsync([FromBody] CreateCustomerForm form)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    Email = form.Email,
                    FirstName = form.FirstName,
                    LastName = form.LastName,
                    Address = form.Address,
                    City = form.City
                };

                // insert customer
                _dbContext.Customers.Add(customer);
                await _dbContext.SaveChangesAsync();

                //TODO: send event
                _messageSender.Send(new DomainEvent(customer, EventType.Created, "customer_exchange", true));
                // return result
                return CreatedAtRoute("GetByCustomerId", new { customerId = customer.CustomerId }, customer);
            }

            return BadRequest();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Unable to save changes. ");
            return StatusCode(StatusCodes.Status500InternalServerError);
            throw;
        }
    }
}