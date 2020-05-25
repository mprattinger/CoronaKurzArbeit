using CoronaKurzArbeit.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CoronaKurzArbeit.DAL.DataAccessSQL
{
    //dotnet ef --startup-project ..\CoronaKurzArbeit\ migrations add initial
    //dotnet ef --startup-project ..\CoronaKurzArbeit database update
    public class ApplicationDbContext : DbContext
    {

        public DbSet<TimeBooking>? TimeBookings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
