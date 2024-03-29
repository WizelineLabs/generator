namespace Reusable.Utils;

public class DateTimeUtils
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly List<string> _holidays;
    private readonly OpenHours _openHours;

    public DateTimeUtils(IEnumerable<DateTimeOffset> holidays, OpenHours openHours)
    {
        _holidays = dateListToStringList(holidays);
        _openHours = openHours;
    }

    public double getElapsedMinutes(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (_openHours.StartHour == 0 || _openHours.EndHour == 0)
            throw new InvalidOperationException("Open hours cannot be started with zero hours or ended with zero hours");

        int hour = startDate.Hour;
        int minute = startDate.Minute;
        if (hour == 0 && minute == 0)
        {
            startDate = DateTimeOffset.Parse(string.Format("{0} {1}:{2}", startDate.ToString(DateFormat), _openHours.StartHour, _openHours.StartMinute));
        }
        hour = endDate.Hour;
        minute = endDate.Minute;
        if (hour == 0 && minute == 0)
        {
            endDate = DateTimeOffset.Parse(string.Format("{0} {1}:{2}", endDate.ToString(DateFormat), _openHours.EndHour, _openHours.EndMinute));
        }

        startDate = nextOpenDay(startDate);
        endDate = prevOpenDay(endDate);

        if (startDate > endDate)
            return 0;

        if (startDate.ToString(DateFormat).Equals(endDate.ToString(DateFormat)))
        {
            if (!isWorkingDay(startDate))
                return 0;

            if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday ||
                _holidays.Contains(startDate.ToString(DateFormat)))
                return 0;

            if (isDateBeforeOpenHours(startDate))
            {
                startDate = getStartOfDay(startDate);
            }
            if (isDateAfterOpenHours(endDate))
            {
                endDate = getEndOfDay(endDate);
            }
            var endminutes = (endDate.Hour * 60) + endDate.Minute;
            var startminutes = (startDate.Hour * 60) + startDate.Minute;

            return endminutes - startminutes;

        }

        var endOfDay = getEndOfDay(startDate);
        var startOfDay = getStartOfDay(endDate);
        var usedMinutesinEndDate = endDate.Subtract(startOfDay).TotalMinutes;
        var usedMinutesinStartDate = endOfDay.Subtract(startDate).TotalMinutes;
        var tempStartDate = startDate.AddDays(1);
        var workingHoursInMinutes = (_openHours.EndHour - _openHours.StartHour) * 60;
        var totalUsedMinutes = usedMinutesinEndDate + usedMinutesinStartDate;

        for (DateTimeOffset day = tempStartDate.Date; day < endDate.Date; day = day.AddDays(1.0))
        {
            if (isWorkingDay(day))
            {
                totalUsedMinutes += workingHoursInMinutes;
            }
        }

        return totalUsedMinutes;
    }
    public DateTimeOffset add(DateTimeOffset date, int minutes)
    {
        if (_openHours != null)
        {
            if (_openHours.StartHour == 0 || _openHours.EndHour == 0)
                throw new InvalidOperationException("Open hours cannot be started with zero hours or ended with zero hours");

            date = nextOpenDay(date);
            var endOfDay = getEndOfDay(date);
            var minutesLeft = (int)endOfDay.Subtract(date).TotalMinutes;

            if (minutesLeft < minutes)
            {
                date = nextOpenDay(endOfDay.AddMinutes(1));
                date = nextOpenDay(date);
                minutes -= minutesLeft;
            }
            var workingHoursInMinutes = (_openHours.EndHour - _openHours.StartHour) * 60;
            while (minutes > workingHoursInMinutes)
            {
                date = getStartOfDay(date.AddDays(1));
                date = nextOpenDay(date);
                minutes -= workingHoursInMinutes;
            }
        }

        return date.AddMinutes(minutes);

    }

    private List<string> dateListToStringList(IEnumerable<DateTimeOffset> dates)
    {
        return dates.Select(piDate => piDate.ToString(DateFormat)).ToList();
    }

    private DateTimeOffset prevOpenDay(DateTimeOffset endDate)
    {
        if (_holidays.Contains(endDate.ToString(DateFormat)))
        {
            return prevOpenDayAfterHoliday(endDate);
        }
        if (endDate.DayOfWeek == DayOfWeek.Saturday)
        {
            return prevOpenDayAfterHoliday(endDate);
        }
        if (endDate.DayOfWeek == DayOfWeek.Sunday)
        {
            return prevOpenDayAfterHoliday(endDate);
        }
        if (isDateBeforeOpenHours(endDate))
        {
            return getStartOfDay(endDate);
        }
        if (isDateAfterOpenHours(endDate))
        {
            return getEndOfDay(endDate);
        }
        return endDate;
    }

    private bool isWorkingDay(DateTimeOffset date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday &&
               !_holidays.Contains(date.ToString(DateFormat));
    }

    private DateTimeOffset nextOpenDay(DateTimeOffset startDate)
    {
        if (_holidays.Contains(startDate.ToString(DateFormat)))
        {
            return nextOpenDayAfterHoliday(startDate);
        }
        if (startDate.DayOfWeek == DayOfWeek.Saturday)
        {
            return nextOpenDayAfterHoliday(startDate);
        }
        if (startDate.DayOfWeek == DayOfWeek.Sunday)
        {
            return nextOpenDayAfterHoliday(startDate);
        }
        if (isDateBeforeOpenHours(startDate))
        {
            return getStartOfDay(startDate);
        }
        if (isDateAfterOpenHours(startDate))
        {

            var nextDate = startDate.AddDays(1);

            if (_holidays.Contains(nextDate.ToString(DateFormat)))
            {
                return nextOpenDayAfterHoliday(nextDate);
            }
            return getStartOfDay(nextDate);
        }
        return startDate;
    }

    private DateTimeOffset nextOpenDayAfterHoliday(DateTimeOffset holiday)
    {
        var nextDay = holiday.AddDays(1);
        if (nextDay.DayOfWeek == DayOfWeek.Saturday)
            nextDay = nextDay.AddDays(2);
        if (nextDay.DayOfWeek == DayOfWeek.Sunday)
            nextDay = nextDay.AddDays(1);
        while (_holidays.Contains(nextDay.ToString(DateFormat)))
        {
            nextDay = nextDay.AddDays(1);
        }
        return getStartOfDay(nextDay);
    }

    private DateTimeOffset prevOpenDayAfterHoliday(DateTimeOffset holiday)
    {
        var prevDay = holiday.AddDays(-1);
        if (prevDay.DayOfWeek == DayOfWeek.Saturday)
            prevDay = prevDay.AddDays(-1);
        if (prevDay.DayOfWeek == DayOfWeek.Sunday)
            prevDay = prevDay.AddDays(-2);
        while (_holidays.Contains(prevDay.ToString(DateFormat)))
        {
            prevDay = prevDay.AddDays(-1);
        }
        return getEndOfDay(prevDay);
    }

    private DateTimeOffset getStartOfDay(DateTimeOffset nextDate)
    {
        return DateTimeOffset.Parse(string.Format("{0} {1}:{2}", nextDate.ToString(DateFormat), _openHours.StartHour, _openHours.StartMinute));
    }

    private DateTimeOffset getEndOfDay(DateTimeOffset startDate)
    {
        return DateTimeOffset.Parse(string.Format("{0} {1}:{2}", startDate.ToString(DateFormat), _openHours.EndHour, _openHours.EndMinute));
    }

    private bool isDateBeforeOpenHours(DateTimeOffset startDate)
    {
        return startDate.Hour < _openHours.StartHour || (startDate.Hour == _openHours.StartHour && startDate.Minute < _openHours.StartMinute);
    }
    private bool isDateAfterOpenHours(DateTimeOffset startDate)
    {
        return startDate.Hour > _openHours.EndHour || (startDate.Hour == _openHours.EndHour && startDate.Minute > _openHours.EndMinute);
    }

}

public class OpenHours
{
    public OpenHours(string openHours)
    {
        var openClose = openHours.Split(new[] { ':', ';' });
        StartHour = int.Parse(openClose[0]);
        StartMinute = int.Parse(openClose[1]);
        EndHour = int.Parse(openClose[2]);
        EndMinute = int.Parse(openClose[3]);
    }

    public int StartHour
    {
        get;
        set;
    }

    public int StartMinute { get; set; }

    public int EndHour
    {
        get;
        set;
    }

    public int EndMinute
    {
        get;
        set;
    }
}
