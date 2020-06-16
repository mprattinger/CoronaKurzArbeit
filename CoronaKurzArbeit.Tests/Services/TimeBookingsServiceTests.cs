using CoronaKurzArbeit.DAL.DataAccessSQL;
using CoronaKurzArbeit.Logic.Services;
using CoronaKurzArbeit.Shared.Models;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class TimeBookingsServiceTests : IDisposable
    {
        private readonly NullLogger<TimeBookingsService> logger;
        private readonly ApplicationDbContext ctx;

        #region Arrange
        public TimeBookingsServiceTests()
        {
            logger = new NullLogger<TimeBookingsService>();
            ctx = DbContextHelper.GetContext("tbs");
        }

        public void Dispose()
        {
            ctx.Dispose();
        }
        #endregion

        #region GetBookingsForDay
        [Fact]
        public async Task OneBooking()
        {
            //Prepare
            var theDay = new DateTime(2020, 05, 19);
            ctx.TimeBookings.RemoveRange();
            await ctx.SaveChangesAsync();
            ctx.TimeBookings.Add(new TimeBooking
            {
                BookingTime = theDay.AddHours(6).AddMinutes(3)
            });
            await ctx.SaveChangesAsync();

            var sut = new TimeBookingsService(logger, ctx);
            (await sut.GetBookingsForDayAsync(theDay.Date)).Count.Should().Be(1);
        }
        #endregion
    }
}
