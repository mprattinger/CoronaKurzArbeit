using CoronaKurzArbeit.Models;
using Microsoft.EntityFrameworkCore;

namespace CoronaKurzArbeit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<TimeRegistration>? TimeRegistrations { get; set; }

        public DbSet<TimeBooking>? TimeBookings { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
    }
}
