using CoronaKurzArbeit.Services;
using CoronaKurzArbeit.Tests.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace CoronaKurzArbeit.Tests.Services
{
    public class FeiertagsServiceTests
    {
        [Fact]
        public void IstKeinFeiertag()
        {
            var sut = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
            var t = new DateTime(2020, 05, 20);
            sut.IsFeiertag(t).Should().BeFalse();            
        }

        [Fact]
        public void IstEinFeiertag()
        {
            var sut = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
            var t = new DateTime(2020, 05, 21);
            sut.IsFeiertag(t).Should().BeTrue();
        }

        [Fact]
        public void IstEinFenstertag_Freitag()
        {
            var sut = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
            var t = new DateTime(2019, 12, 26);
            sut.IsFeiertag(t).Should().BeTrue();
        }

        [Fact]
        public void IstEinFenstertag_Montag()
        {
            var sut = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
            var t = new DateTime(2019, 12, 24);
            sut.IsFeiertag(t).Should().BeTrue();
        }

        [Fact]
        public void IstKeinFenstertag_Freitag_aber_Feiertag()
        {
            var sut = new FeiertagService(new FakeDateTimeProvider(new DateTime(2020, 05, 19)));
            var t = new DateTime(2019, 11, 1);
            sut.IsFeiertag(t).Should().BeTrue();
            sut.IsFenstertag(t).Should().BeFalse();
        }
    }
}
