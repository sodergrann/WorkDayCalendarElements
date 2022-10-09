using System;

namespace WorkDayCalendarApp
{
  class Program
  {
    static void Main()
    {
      WorkDayCalendar calendar = new WorkDayCalendar();

      Console.WriteLine("Example WorkDayCalendar");

      calendar.RecurringHolidays.Add(new DateTime(1, 5, 17));
      calendar.Holidays.Add(new DateTime(2004, 5, 27));
      calendar.TimeInCalendar = new DateTime(2004, 5, 24, 18, 05, 00);

      calendar.AddWorkDays(-5.5, VerboseDebug:true);
      calendar.AddWorkDays(0, VerboseDebug: true);


    }
  }
}
