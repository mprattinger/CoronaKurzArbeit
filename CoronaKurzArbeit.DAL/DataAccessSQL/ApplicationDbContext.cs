using CoronaKurzArbeit.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CoronaKurzArbeit.DAL.DataAccessSQL
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<TimeBooking>? TimeBookings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
