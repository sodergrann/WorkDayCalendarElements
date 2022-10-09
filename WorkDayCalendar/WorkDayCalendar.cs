using System;
using System.Collections.Generic;
using System.Text;

namespace WorkDayCalendarApp
{
  /// <summary>
  /// Calendar mode - when using Minute precision - we
  /// truncate interval in full minutes.
  /// </summary>
  public enum CalendarMode
  {
    CalendarMode_SecondPrecision = 0,
    CalendarMode_Default = CalendarMode_SecondPrecision,
    CalendarMode_MinutePrecision = 1
  }
  public class WorkDayCalendar
  {

    public enum CalendarTraversalDirection
    {
      CalendarTraversalDirection_Forward = 0,
      CalendarTraversalDirection_Default = CalendarTraversalDirection_Forward,
      CalendarTraversalDirection_Backward = 1
    }

    /// <summary>
    /// WorkDay used in calculartions.
    /// </summary>
    public WorkDay DefaultWorkDay { get; private set; }

    /// <summary>
    /// Current time in the calendar.
    /// </summary>
    public DateTime TimeInCalendar{ get; set; }

    /// <summary>
    /// List of single instance holidays.
    /// </summary>
    public List<DateTime> Holidays { get; set; }

    /// <summary>
    /// List of recurring holidays.
    /// </summary>
    public List<DateTime> RecurringHolidays { get; set; }

    public CalendarMode CurrentCalendarMode { get; }


    #region WorkDayCalendar constructors
    public WorkDayCalendar()
    {
      Holidays = new List<DateTime>();
      RecurringHolidays = new List<DateTime>();
      DefaultWorkDay = new WorkDay();
      TimeInCalendar = DateTime.Now;
      CurrentCalendarMode = CalendarMode.CalendarMode_MinutePrecision;
    }


    public WorkDayCalendar(WorkDay DefaultWorkDay) : this(DefaultWorkDay, DateTime.Now)
    {}



    public WorkDayCalendar(WorkDay DefaultWorkDay, DateTime TimeInCalendar) : this()
    {
      this.DefaultWorkDay = DefaultWorkDay;
      this.TimeInCalendar = TimeInCalendar;
    }

    #endregion

    
    
    public Boolean IsWorkingDay()
    {
      return IsWorkingDay(TimeInCalendar);
    }

    public Boolean IsWorkingDay(DateTime Day)
    {
      Boolean IsWorkingDay;

      switch (Day.DayOfWeek)
      {
        case DayOfWeek.Monday:
        case DayOfWeek.Tuesday:
        case DayOfWeek.Wednesday:
        case DayOfWeek.Thursday:
        case DayOfWeek.Friday:

          IsWorkingDay = !((IsHoliday(Day) || IsRecurringHoliday(Day)));
          break;

        default:

          IsWorkingDay = false;
          break;
      }

      return IsWorkingDay;
    }


    public Boolean IsRecurringHoliday()
    {
      return IsRecurringHoliday(TimeInCalendar);
    }


    public Boolean IsRecurringHoliday(DateTime Day)
    {
      foreach (var HolidayCandidate in RecurringHolidays)
      {
        if (Day.Month == HolidayCandidate.Month && Day.Day == HolidayCandidate.Day)
          return true;
      }

      return false;
    }



    public Boolean IsHoliday()
    {
      return IsHoliday(TimeInCalendar);
    }


    public Boolean IsHoliday(DateTime Day)
    {
      foreach (var HolidayCandidate in Holidays)
      {
        if (Day.Year == HolidayCandidate.Year
          && Day.Month == HolidayCandidate.Month
          && Day.Day == HolidayCandidate.Day)
            return true;
      }

      return false;
    }


    public DateTime TimeInCalendarSecondsTruncated 
    {
      get
      {
        return new DateTime(TimeInCalendar.Year, TimeInCalendar.Month, TimeInCalendar.Day, TimeInCalendar.Hour, TimeInCalendar.Minute, 00);
      }
    }



    public void AddWorkDays(double WorkDaysToAdd, Boolean VerboseDebug = false)
    {
      StringBuilder sb = new StringBuilder(string.Format("Adding {0} workdays to {1} results in ", WorkDaysToAdd, TimeInCalendar));
      TimeSpan timespan = DefaultWorkDay.WorkDayEnd.Subtract(DefaultWorkDay.WorkDayStart);
      double interval = timespan.TotalSeconds * WorkDaysToAdd;

      if (CurrentCalendarMode == CalendarMode.CalendarMode_MinutePrecision)
      {
        interval = ((int)interval / 60) * 60;
      }

      // If value is zero - remain unchanged
      if ((int)interval == 0)
      {
        sb.Append(TimeInCalendar);
        if (VerboseDebug)
        {
          Console.WriteLine(sb.ToString());
        }
        return;
      }

      DateTime StartOfDay = new DateTime(TimeInCalendar.Year, TimeInCalendar.Month, TimeInCalendar.Day, 0, 0, 0);
      DateTime EndOfTheDay = new DateTime(TimeInCalendar.Year, TimeInCalendar.Month, TimeInCalendar.Day, 23, 59, 59);
      DateTime StartOfWorkDay = StartOfDay.Add(DefaultWorkDay.WorkDayStartSpan);
      DateTime EndOfWorkDay = StartOfDay.Add(DefaultWorkDay.WorkDayEndSpan);

      CalendarTraversalDirection direction = (interval < 0) ? CalendarTraversalDirection.CalendarTraversalDirection_Backward : CalendarTraversalDirection.CalendarTraversalDirection_Forward;

      TraverseTimeInCalendarIntoWorkDaySpan(direction, ref StartOfDay, ref EndOfTheDay, ref StartOfWorkDay, ref EndOfWorkDay);
      TraverseTimeInCalendarPastHoliday(direction, ref StartOfDay, ref EndOfTheDay, ref StartOfWorkDay, ref EndOfWorkDay);
      TraverseTimeInCalendarFullWorkDays(direction, ref interval, ref StartOfDay, ref EndOfTheDay, ref StartOfWorkDay, ref EndOfWorkDay);
      TraverseTimeInCalendarPastHoliday(direction, ref StartOfDay, ref EndOfTheDay, ref StartOfWorkDay, ref EndOfWorkDay);
      TimeInCalendar = TimeInCalendar.AddSeconds(interval);

      sb.Append(TimeInCalendar);
      if (VerboseDebug)
      {
        Console.WriteLine(sb.ToString());
      }


    }


    /// <summary>
    /// Traverse current time full work day blocks in stated direction based on remaining interval.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="interval"></param>
    /// <param name="StartOfDay"></param>
    /// <param name="EndOfTheDay"></param>
    /// <param name="StartOfWorkDay"></param>
    /// <param name="EndOfWorkDay"></param>
    private void TraverseTimeInCalendarFullWorkDays(CalendarTraversalDirection direction, ref double interval, ref DateTime StartOfDay, ref DateTime EndOfTheDay, ref DateTime StartOfWorkDay, ref DateTime EndOfWorkDay)
    {
      switch (direction)
      {
        default:
        case CalendarTraversalDirection.CalendarTraversalDirection_Forward:
          {
            while (TimeInCalendar.AddSeconds(interval) > EndOfWorkDay)
            {
              TimeSpan RemainingThisDay = EndOfWorkDay - TimeInCalendar;

              TraverseDayInCalendarSingleDay(direction, ref StartOfDay, ref EndOfTheDay, out StartOfWorkDay, out EndOfWorkDay);

              TimeInCalendar = StartOfWorkDay;
              if (IsWorkingDay(TimeInCalendar))
              {
                interval -= RemainingThisDay.TotalSeconds;
              }
            }
          }
          break;

        case CalendarTraversalDirection.CalendarTraversalDirection_Backward:
          {
            while (TimeInCalendar.AddSeconds(interval) < StartOfWorkDay)
            {
              TimeSpan RemainingThisDay = TimeInCalendar - StartOfWorkDay;

              TraverseDayInCalendarSingleDay(direction, ref StartOfDay, ref EndOfTheDay, out StartOfWorkDay, out EndOfWorkDay);

              TimeInCalendar = EndOfWorkDay;
              if (IsWorkingDay(TimeInCalendar))
              {
                interval += RemainingThisDay.TotalSeconds;
              }
            }
          }
          break;
      }
    }

    /// <summary>
    /// Traverse provided variables one day in stated direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="StartOfDay"></param>
    /// <param name="EndOfTheDay"></param>
    /// <param name="StartOfWorkDay"></param>
    /// <param name="EndOfWorkDay"></param>
    private void TraverseDayInCalendarSingleDay(CalendarTraversalDirection direction, ref DateTime StartOfDay, ref DateTime EndOfTheDay, out DateTime StartOfWorkDay, out DateTime EndOfWorkDay)
    {
      StartOfDay = StartOfDay.AddDays((direction == CalendarTraversalDirection.CalendarTraversalDirection_Forward) ? 1.0 : -1.0);
      EndOfTheDay = EndOfTheDay.AddDays((direction == CalendarTraversalDirection.CalendarTraversalDirection_Forward) ? 1.0 : -1.0);
      StartOfWorkDay = StartOfDay.Add(DefaultWorkDay.WorkDayStartSpan);
      EndOfWorkDay = StartOfDay.Add(DefaultWorkDay.WorkDayEndSpan);
    }

    /// <summary>
    /// Ensure that the start datetime before traversal is within a working day.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="StartOfDay"></param>
    /// <param name="EndOfTheDay"></param>
    /// <param name="StartOfWorkDay"></param>
    /// <param name="EndOfWorkDay"></param>
    private void TraverseTimeInCalendarIntoWorkDaySpan(CalendarTraversalDirection direction, ref DateTime StartOfDay, ref DateTime EndOfTheDay, ref DateTime StartOfWorkDay, ref DateTime EndOfWorkDay)
    {
      switch (direction)
      {
        default:
        case CalendarTraversalDirection.CalendarTraversalDirection_Forward:
          {
            if (TimeInCalendar < StartOfWorkDay)
            {
              TimeInCalendar = StartOfWorkDay;
            }

            if (TimeInCalendar > EndOfWorkDay)
            {
              TraverseDayInCalendarSingleDay(direction, ref StartOfDay, ref EndOfTheDay, out StartOfWorkDay, out EndOfWorkDay);
              TimeInCalendar = StartOfWorkDay;
            }
          }
          break;

        case CalendarTraversalDirection.CalendarTraversalDirection_Backward:
          {
            if (TimeInCalendar > EndOfWorkDay)
            {
              TimeInCalendar = EndOfWorkDay;
            }

            if (TimeInCalendar < StartOfWorkDay)
            {
              TraverseDayInCalendarSingleDay(direction, ref StartOfDay, ref EndOfTheDay, out StartOfWorkDay, out EndOfWorkDay);
              TimeInCalendar = EndOfWorkDay;
            }
          }
          break;
      }
    }


    /// <summary>
    /// Yep - that is what is performed - move the time past holidays.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="StartOfDay"></param>
    /// <param name="EndOfTheDay"></param>
    /// <param name="StartOfWorkDay"></param>
    /// <param name="EndOfWorkDay"></param>
    private void TraverseTimeInCalendarPastHoliday(CalendarTraversalDirection direction, ref DateTime StartOfDay, ref DateTime EndOfTheDay, ref DateTime StartOfWorkDay, ref DateTime EndOfWorkDay)
    {
      while (!IsWorkingDay(TimeInCalendar))
      {
        TraverseDayInCalendarSingleDay(direction, ref StartOfDay, ref EndOfTheDay, out StartOfWorkDay, out EndOfWorkDay);
        TimeInCalendar = (direction == CalendarTraversalDirection.CalendarTraversalDirection_Forward) ? StartOfWorkDay : EndOfWorkDay;
      }
    }


  }
}
