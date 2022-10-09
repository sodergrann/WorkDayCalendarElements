using NUnit.Framework;
using System;
using WorkDayCalendarApp;

namespace TestWorkDayCalendar
{
  [TestFixture]
  public class Tests
  {
    private WorkDayCalendar calendar;

    [SetUp]
    public void Setup()
    {
      calendar = new WorkDayCalendar();
    }

    [Test]
    public void WorkDay_CorrectTimeSpan()
    {
      WorkDay aShortWorkDay = new WorkDay(new DateTime(1, 1, 1, 10, 0, 0), new DateTime(1, 1, 1, 15, 0, 0));
      Assert.IsTrue(aShortWorkDay.WorkDayLength.Equals(new TimeSpan(5, 0, 0)), "Short workday should be 5 hrs in length.");
      calendar = new WorkDayCalendar(aShortWorkDay);
      Assert.IsTrue(calendar.DefaultWorkDay.WorkDayLength.Equals(new TimeSpan(5, 0, 0)), "Short workday should be 5 hrs in length.");
    }

    [Test]
    public void WorkDay_EndCannotBeBeforeStart()
    {
      bool Fail = true;
      try
      {
        WorkDay wd = new WorkDay(new DateTime(1, 1, 1, 16, 0, 0), new DateTime(1, 1, 1, 8, 0, 0));
      }
      catch (WorkDayException ex)
      {
        Console.WriteLine(ex.Message);
        Fail = false;
      }

      Assert.IsFalse(Fail, "Having WorkDay end time before start time should throw an exception.");
    }


    [Test]
    public void WorkDay_DefaultLength()
    {
      WorkDay wd = new WorkDay();
      Assert.IsTrue(wd.WorkDayLength.Equals(new TimeSpan(8, 0, 0)), "Default workday in should be 8 hrs in length.");
    }


    [Test]
    public void WorkDay_GetStartSpan()
    {
      Assert.IsTrue(calendar.DefaultWorkDay.WorkDayStartSpan.Equals(new TimeSpan(8, 0, 0)));
    }


    [Test]
    public void WorkDay_GetEndSpan()
    {
      Assert.IsTrue(calendar.DefaultWorkDay.WorkDayEndSpan.Equals(new TimeSpan(16, 0, 0)));
    }


    [Test]
    public void Calendar_WorkDay_DefaultLength()
    {
      Assert.IsTrue(calendar.DefaultWorkDay.WorkDayLength.Equals(new TimeSpan(8, 0, 0)), "Default workday in calendar should be 8 hrs in length.");
    }


    [Test]
    public void Calendar_AddWorkday_1()
    {
      calendar.TimeInCalendar = new DateTime(2022, 10, 3, 15, 7, 0);
      calendar.AddWorkDays(0.25);

      Assert.IsTrue(calendar.TimeInCalendar == new DateTime(2022, 10, 4, 9, 7, 0), "2022-10-03 15:07 + 0.25 should result in 2022-10-04 09:07");
    }


    [Test]
    public void Calendar_AddWorkday_2()
    {
      calendar.TimeInCalendar = new DateTime(2022, 10, 3, 4, 0, 0);
      calendar.AddWorkDays(0.5);
      Assert.IsTrue(calendar.TimeInCalendar == new DateTime(2022, 10, 3, 12, 0, 0), "2022-10-03 04:00 + 0.5 should result in 2022-10-03 12:00"); 
    }


    [Test]
    public void Calendar_AddNegativeWorkday_1()
    {
      calendar.TimeInCalendar = new DateTime(2022, 10, 3, 20, 0, 0);
      calendar.AddWorkDays(-0.5);
      Assert.IsTrue(calendar.TimeInCalendar == new DateTime(2022, 10, 3, 12, 0, 0), "2022-10-03 20:00 -0.5 should result in 2022-10-03 12:00");
    }


    [TestCase(2022, 10, 3)]
    [TestCase(2022, 10, 4)]
    [TestCase(2022, 10, 5)]
    [TestCase(2022, 10, 6)]
    [TestCase(2022, 10, 7)]
    public void Calendar_Date_IsWorkingDay(int year, int month, int day)
    {
      Assert.IsTrue(calendar.IsWorkingDay(new DateTime(year, month, day)));
    }

    [TestCase(2022, 10, 8)]
    [TestCase(2022, 10, 9)]
    public void Calendar_Date_IsDayOff(int year, int month, int day)
    {
      Assert.IsFalse(calendar.IsWorkingDay(new DateTime(year, month, day)));
    }


    [TestCase(2022, 12, 26)] // Is a Monday :)
    public void Calendar_Date_IsHoliday(int year, int month, int day)
    {
      calendar.Holidays.Add(new DateTime(year, month, day));
      calendar.RecurringHolidays.Add(new DateTime(year, month, day));
      Assert.IsFalse(calendar.IsWorkingDay(new DateTime(year, month, day)));
      Assert.IsTrue(calendar.IsHoliday(new DateTime(year, month, day)));
      Assert.IsTrue(calendar.IsRecurringHoliday(new DateTime(year, month, day)));
    }


    [TestCase(2022, 12, 26)] // Is a Monday :)
    public void Calendar_Date_IsRecurringHoliday(int year, int month, int day)
    {
      calendar.RecurringHolidays.Add(new DateTime(year, month, day));
      Assert.IsFalse(calendar.IsWorkingDay(new DateTime(year, month, day)));
      Assert.IsTrue(calendar.IsRecurringHoliday(new DateTime(year, month, day)));
      Assert.IsTrue(calendar.IsRecurringHoliday(new DateTime(year+1, month, day)));
    }


    [TestCase(2022, 12, 26)] // Is a Monday :)
    public void Calendar_CurrentDate_IsHoliday(int year, int month, int day)
    {
      calendar.Holidays.Add(new DateTime(year, month, day));
      calendar.RecurringHolidays.Add(new DateTime(year, month, day));
      calendar.TimeInCalendar = new DateTime(year, month, day);
      Assert.IsFalse(calendar.IsWorkingDay());
      Assert.IsTrue(calendar.IsHoliday());
      Assert.IsTrue(calendar.IsRecurringHoliday());
      Assert.IsTrue(calendar.IsRecurringHoliday(new DateTime(year + 1, month, day)));
    }


    [Test()]
    public void Calendar_CurrentDate_AddZero()
    {
      calendar.TimeInCalendar = new DateTime(2022, 12, 27); // Normal Tuesday.
      calendar.RecurringHolidays.Add(new DateTime(2022, 12, 26));
      calendar.AddWorkDays(0);
      Assert.IsTrue(calendar.TimeInCalendar.Equals(new DateTime(2022, 12, 27, 00, 00, 00)), "The result should be 27.12.2022 08:00");
    }


    [Test]
    public void Calendar_Date_AdjustmentsAccordingToExample()
    {
      calendar.RecurringHolidays.Add(new DateTime(1, 5, 17));
      calendar.Holidays.Add(new DateTime(2004, 5, 27));
      calendar.TimeInCalendar = new DateTime(2004, 5, 24, 18, 05, 00);

      calendar.AddWorkDays(-5.5);
      Console.WriteLine(calendar.TimeInCalendar);
      Assert.IsTrue(calendar.TimeInCalendar.Equals(new DateTime(2004, 05, 14, 12, 00, 00)), "The result should be 14.05.2004 12:00");

      calendar.TimeInCalendar = new DateTime(2004, 5, 24, 19, 03, 00);
      calendar.AddWorkDays(44.723656);
      Console.WriteLine(calendar.TimeInCalendar);
      Assert.IsTrue(calendar.TimeInCalendar.Equals(new DateTime(2004, 07, 27, 13, 47, 00)), "24-05-2004 19:03 +44.723656 working days is 27-07-2004 13:47");
      Assert.IsTrue(calendar.TimeInCalendarSecondsTruncated.Equals(new DateTime(2004, 07, 27, 13, 47, 00)), "24-05-2004 19:03 +44.723656 working days is 27-07-2004 13:47");

      calendar.TimeInCalendar = new DateTime(2004, 5, 24, 18, 03, 00);
      calendar.AddWorkDays(-6.7470217);
      Console.WriteLine(calendar.TimeInCalendar);
      Assert.IsTrue(calendar.TimeInCalendar.Equals(new DateTime(2004, 05, 13, 10, 02, 00)), "24-05-2004 18:03 -6.7470217 working days is 13-05-2004 10:02");
      Assert.IsFalse(calendar.TimeInCalendarSecondsTruncated.Equals(new DateTime(2004, 05, 13, 10, 01, 00)), "24-05-2004 18:03 -6.7470217 working days is 13-05-2004 10:01 (truncated)");


      calendar.TimeInCalendar = new DateTime(2004, 5, 24, 08, 03, 00);
      calendar.AddWorkDays(12.782709);
      Console.WriteLine(calendar.TimeInCalendar);
      Assert.IsTrue(calendar.TimeInCalendar.Equals(new DateTime(2004, 06, 10, 14, 18, 00)), "24-05-2004 08:03 +12.782709 working days is 10-06-2004 14:18");
      Assert.IsTrue(calendar.TimeInCalendarSecondsTruncated.Equals(new DateTime(2004, 06, 10, 14, 18, 00)), "24-05-2004 08:03 +12.782709 working days is 10-06-2004 14:18 (truncated)");

      calendar.TimeInCalendar = new DateTime(2004, 5, 24, 07, 03, 00);
      calendar.AddWorkDays(8.276628);
      Console.WriteLine(calendar.TimeInCalendar);
      Assert.IsTrue(calendar.TimeInCalendar.Equals(new DateTime(2004, 06, 04, 10, 12, 00)), "24-05-2004 07:03 +8.276628 working days is 04-06- 2004 10:12");
      Assert.IsTrue(calendar.TimeInCalendarSecondsTruncated.Equals(new DateTime(2004, 06, 04, 10, 12, 00)), "24-05-2004 07:03 +8.276628 working days is 04-06- 2004 10:12 (Truncated)");
    }

  }
}