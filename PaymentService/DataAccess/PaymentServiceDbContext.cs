﻿using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.DataAccess;

public class PaymentServiceDbContext : DbContext
{
    public PaymentServiceDbContext(DbContextOptions<PaymentServiceDbContext> options) : base(options)
    {
    }


    public DbSet<Customer> Customers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Invoice>(builder =>
        {
            builder.Property(i => i.InvoiceNumber)
                .ValueGeneratedOnAdd();
        });
    }
}