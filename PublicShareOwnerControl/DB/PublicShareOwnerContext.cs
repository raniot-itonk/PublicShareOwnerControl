﻿using Microsoft.EntityFrameworkCore;

namespace PublicShareOwnerControl.DB
{
    public class PublicShareOwnerContext : DbContext
    {
        public PublicShareOwnerContext(DbContextOptions<PublicShareOwnerContext> options)
            : base(options)
        {
        }
        public DbSet<Stock> Stocks { get; set; }
    }
}
