using System;
using System.Collections.Generic;
using System.Text;

namespace WorkDayCalendarApp
{
  public class WorkDayException : Exception
  {
    public WorkDayException()
    {
    }

    public WorkDayException(string message)
        : base(message)
    {
    }

    public WorkDayException(string message, Exception inner)
        : base(message, inner)
    {
    }
  }
}
