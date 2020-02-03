// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AppSourceMockWebApp.DAL
{
    using AppSourceMockWebApp.Models;
    using Microsoft.EntityFrameworkCore;

    public class SubscriptionDbContext : DbContext
    {
        public SubscriptionDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Subscription> Subscriptions { get; set; }
    }
}
