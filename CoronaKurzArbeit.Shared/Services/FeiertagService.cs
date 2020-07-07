using CoronaKurzArbeit.Shared.Extensions;
using CoronaKurzArbeit.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoronaKurzArbeit.Shared.Services
{
    public interface IFeiertagService
    {
        Feiertag GetFeiertag(DateTime value);
        bool IsFeiertag(DateTime value);
        bool IsFenstertag(DateTime value, DayOfWeek lastDayOfWorkWeek = DayOfWeek.Friday);
    }

    public class FeiertagService : IFeiertagService
    {
        private readonly List<Feiertag> _feierTage = new List<Feiertag>();

        public FeiertagService(IDateTimeProvider dateTimeProvider)
        {
            load(dateTimeProvider.GetCurrentTime().Year);
        }

        public bool IsFeiertag(DateTime value)
        {
            load(value.Year);
            return _feierTage.Any(x => x.Date.Date == value.Date);
        }

        public Feiertag GetFeiertag(DateTime value)
        {
            load(value.Year);
            return _feierTage.FirstOrDefault(x => x.Date.Date == value.Date);
        }

        public bool IsFenstertag(DateTime value, DayOfWeek lastDayOfWorkWeek = DayOfWeek.Friday)
        {
            load(value.Year);
            if (IsFeiertag(value)) return false;
            if (IsFeiertag(value.Date.AddDays(-1)) && !(value.Date.IsSaturday() || value.Date.IsSunday()) && value.DayOfWeek == lastDayOfWorkWeek) return true;
            if (IsFeiertag(value.Date.AddDays(1)) && !(value.Date.IsSaturday() || value.Date.IsSunday()) && value.DayOfWeek == DayOfWeek.Monday) return true;
            return false;
        }

        private void load(int year)
        {
            if (_feierTage.Any(x => x.Date.Year == year)) return; //Bereits geladen

            _feierTage.Add(new Feiertag(true, new DateTime(year, 1, 1), "Neujahr"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 1, 6), "Heilige Drei Könige"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 5, 1), "Staatsfeiertag"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 8, 15), "Mariä Himmelfahrt"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 10, 26), "Nationalfeiertag"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 11, 1), "Allerheiligen "));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 12, 8), "Maria Empfängnis "));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 12, 24), "Hl. Abend"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 12, 25), "Weihnachten"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 12, 26), "Stefanitag"));
            _feierTage.Add(new Feiertag(true, new DateTime(year, 12, 31), "Silverster"));

            DateTime osterSonntag = getOsterSonntag(year);
            _feierTage.Add(new Feiertag(false, osterSonntag, "Ostersonntag"));
            //_feiertage.Add(new Feiertag(false, osterSonntag.AddDays(-2), "Karfreitag"));
            _feierTage.Add(new Feiertag(false, osterSonntag.AddDays(1), "Ostermontag"));
            _feierTage.Add(new Feiertag(false, osterSonntag.AddDays(39), "Christi Himmelfahrt"));
            _feierTage.Add(new Feiertag(false, osterSonntag.AddDays(49), "Pfingstsonntag"));
            _feierTage.Add(new Feiertag(false, osterSonntag.AddDays(50), "Pfingstmontag"));
            _feierTage.Add(new Feiertag(false, osterSonntag.AddDays(60), "Fronleichnam"));
        }

        private DateTime getOsterSonntag(int year)
        {
            int g, h, c, j, l, i;

            g = year % 19;
            c = year / 100;
            h = ((c - (c / 4)) - (((8 * c) + 13) / 25) + (19 * g) + 15) % 30;
            i = h - (h / 28) * (1 - (29 / (h + 1)) * ((21 - g) / 11));
            j = (year + (year / 4) + i + 2 - c + (c / 4)) % 7;

            l = i - j;
            int month = (int)(3 + ((l + 40) / 44));
            int day = (int)(l + 28 - 31 * (month / 4));

            return new DateTime(year, month, day);
        }
    }
}
