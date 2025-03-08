using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AbilityTrackerLibrary.Models;
public class CountDownTimer : IDisposable
{
    public Stopwatch _stpWatch = new Stopwatch();

    public event ElapsedEventHandler TimeChanged;
    public Action CountDownFinished;

    public bool IsRunning => timer.Enabled;

    public double StepMs
    {
        get => timer.Interval;
        set => timer.Interval = value;
    }

    private System.Timers.Timer timer = new System.Timers.Timer();

    private TimeSpan _max = TimeSpan.FromMilliseconds(30000);

    public TimeSpan TimeLeft => (_max.TotalMilliseconds - _stpWatch.ElapsedMilliseconds) > 0 ? TimeSpan.FromMilliseconds(_max.TotalMilliseconds - _stpWatch.ElapsedMilliseconds) : TimeSpan.FromMilliseconds(0);

    private bool _mustStop => (_max.TotalMilliseconds - _stpWatch.ElapsedMilliseconds) < 0;

    public string TimeLeftStr => TimeLeft.ToString(@"\mm\:ss");

    public string TimeLeftMsStr => TimeLeft.ToString(@"mm\:ss\.fff");

    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        TimeChanged?.Invoke(sender, e);

        if (_mustStop)
        {
            CountDownFinished?.Invoke();
            _stpWatch.Stop();
            timer.Enabled = false;
        }
    }

    public CountDownTimer(int min, int sec)
    {
        SetTime(min, sec);
        Init();
    }

    public CountDownTimer(TimeSpan ts)
    {
        SetTime(ts);
        Init();
    }

    public CountDownTimer()
    {
        Init();
    }

    private void Init()
    {
        StepMs = 1000;
        timer.Elapsed += Timer_Elapsed;
    }

    public void SetTime(TimeSpan ts)
    {
        _max = ts;
        TimeChanged?.Invoke(null, null);
    }

    public void SetTime(int min, int sec = 0) => SetTime(TimeSpan.FromSeconds(min * 60 + sec));

    public void Start()
    {
        timer.Start();
        _stpWatch.Start();
    }

    public void Pause()
    {
        timer.Stop();
        _stpWatch.Stop();
    }

    public void Stop()
    {
        Reset();
        Pause();
    }

    public void Reset()
    {
        _stpWatch.Reset();
    }

    public void Restart()
    {
        _stpWatch.Reset();
        timer.Start();
    }

    public void Dispose() => timer.Dispose();
}