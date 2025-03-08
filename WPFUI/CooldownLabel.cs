using AbilityTrackerLibrary;
using System;
using System.Timers;
using System.Windows.Controls;

namespace WPFUI;

public class CooldownLabel:Label
{
    public Ability Ability { get; set; }
    public AbilityDisplayer displayer { get; set; }

    public void ActionEvery10MS(object sender, ElapsedEventArgs e)
    {
        if (displayer != null)
        {
            displayer.Dispatcher.Invoke(() =>
            {
                if (Ability != null)
                {
                    double cd = Ability.CurrentCooldownInMs / 1000.0;
                    if (cd > 0)
                        this.Content = Math.Round(cd);
                    else
                        this.Content = "";
                }
            });
        }
    }
}
