using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace WorkDayCalendarApp
{
  public class WorkDay
  {
    public DateTime WorkDayStart { get; set; }
    public DateTime WorkDayEnd { get; set; }

    public TimeSpan WorkDayLength
    {
      get
      {
        return (WorkDayEnd - WorkDayStart);
      }
    }

    /// <summary>
    /// Use the span [8:00,16:00] as default.
    /// </summary>
    public WorkDay() : this(new DateTime(1,1,1,8,0,0), new DateTime(1,1,1,16,0,0))
    {}


    /// <summary>
    /// Use provided parameter to define span of work day,
    /// public WorkDay(DateTime WorkDayStart, DateTime WorkDayEnd)
    /// </summary>
    /// <param name="WorkDayStart"></param>
    /// <param name="WorkDayEnd"></param>
    public WorkDay(DateTime WorkDayStart, DateTime WorkDayEnd)
    {
      this.WorkDayStart = WorkDayStart;
      this.WorkDayEnd = WorkDayEnd;

      if (WorkDayStart > WorkDayEnd)
      {
        throw new WorkDayException();
      }
    }


    public TimeSpan WorkDayStartSpan
    {
      get
      {
        DateTime StartOfDay = new DateTime(WorkDayStart.Year, WorkDayStart.Month, WorkDayStart.Day, 0, 0, 0);
        var span = WorkDayStart - StartOfDay;
        return span;
      }
    }


    public TimeSpan WorkDayEndSpan
    {
      get
      {
        DateTime StartOfDay = new DateTime(WorkDayStart.Year, WorkDayStart.Month, WorkDayStart.Day, 0, 0, 0);
        var span = WorkDayEnd - StartOfDay;
        return span;
      }
    }

    //    public static WorkDay operator *(WorkDay a, double workingdays)
    //    {
    //      TimeSpan timespan = a.WorkDayEnd.Subtract(a.WorkDayStart);
    //      double interval = timespan.TotalSeconds * workingdays;
    //
    //      return new WorkDay(a.WorkDayStart, a.WorkDayStart.AddSeconds(interval));
    //    }
    //
    //    public static DateTime operator +(DateTime a, WorkDay b)
    //    {
    //      TimeSpan timespan = b.WorkDayEnd.Subtract(b.WorkDayStart);
    //      var c = a.AddSeconds(timespan.TotalSeconds);
    //      return c;
    //    }


  }
}
