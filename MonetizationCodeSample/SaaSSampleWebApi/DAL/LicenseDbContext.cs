// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace SaaSSampleWebApi.DAl
{
    using Microsoft.EntityFrameworkCore;
    using SaaSSampleWebApi.Models;

    public class LicenseDbContext : DbContext
    {
        public LicenseDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Licence> Licences { get; set; }

        public DbSet<LicenseManager> LicenseManagers { get; set; }
    }
}
