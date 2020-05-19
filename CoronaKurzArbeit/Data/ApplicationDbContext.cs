using CoronaKurzArbeit.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CoronaKurzArbeit.Data
{
    public class ApplicationDbContext : DbContext
    {
        [Obsolete]
        public DbSet<TimeRegistration>? TimeRegistrations { get; set; }

        public DbSet<TimeBooking>? TimeBookings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
