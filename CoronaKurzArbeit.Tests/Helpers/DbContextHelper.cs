using CoronaKurzArbeit.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoronaKurzArbeit.Tests.Helpers
{
    public static class DbContextHelper
    {
        public static ApplicationDbContext GetContext(string dbName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseInMemoryDatabase(dbName + Guid.NewGuid().ToString());

            var ctx = new ApplicationDbContext(optionsBuilder.Options);

            ctx.RemoveRange(ctx.TimeBookings);
            ctx.SaveChanges();

            return ctx;
        }
    }
}
