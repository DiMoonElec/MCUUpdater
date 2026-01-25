using System.Windows.Forms;
using System;

public static class ControlExtensions
{
  public static void InvokeIfRequired(this Control control, Action action)
  {
    if (control.InvokeRequired)
    {
      control.Invoke(action);
    }
    else
    {
      action();
    }
  }

  public static T InvokeIfRequired<T>(this Control control, Func<T> func)
  {
    if (control.InvokeRequired)
    {
      return (T)control.Invoke(func);
    }
    else
    {
      return func();
    }
  }
}
