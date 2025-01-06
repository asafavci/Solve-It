using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    // Singleton Instance
    public static TimerManager Instance { get; private set; }

    // Timer Class
    private class Timer
    {
        public float RemainingTime;
        public bool IsPaused;
        public Action OnTimerEnd;
        public Action<float> OnTimerUpdate;
        public string TimerID;

        public Timer(float duration, Action onTimerEnd, Action<float> onTimerUpdate, string id)
        {
            RemainingTime = duration;
            IsPaused = false;
            OnTimerEnd = onTimerEnd;
            OnTimerUpdate = onTimerUpdate;
            TimerID = id;
        }
    }

    private List<Timer> timers = new List<Timer>();

    [SerializeField] private Text debugText;
    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            Timer timer = timers[i];

            if (!timer.IsPaused)
            {
                timer.RemainingTime -= Time.deltaTime;
                timer.OnTimerUpdate?.Invoke(timer.RemainingTime);

                if (timer.RemainingTime <= 0)
                {
                    timer.OnTimerEnd?.Invoke();
                    timers.RemoveAt(i);
                }
            }
        }

        if (debugMode)
        {
            DebugTimers();
        }
    }

    public void StartTimer(float duration, Action onTimerEnd, Action<float> onTimerUpdate, string id = null)
    {
        id = id ?? Guid.NewGuid().ToString();
        timers.Add(new Timer(duration, onTimerEnd, onTimerUpdate, id));
    }

    public void PauseTimer(string timerID)
    {
        Timer timer = FindTimer(timerID);
        if (timer != null)
        {
            timer.IsPaused = true;
        }
    }

    public void ResumeTimer(string timerID)
    {
        Timer timer = FindTimer(timerID);
        if (timer != null)
        {
            timer.IsPaused = false;
        }
    }

    public void ResetTimer(string timerID, float newDuration)
    {
        Timer timer = FindTimer(timerID);
        if (timer != null)
        {
            timer.RemainingTime = newDuration;
            timer.IsPaused = false;
        }
    }

    public void AdjustTime(string timerID, float timeDelta)
    {
        Timer timer = FindTimer(timerID);
        if (timer != null)
        {
            timer.RemainingTime += timeDelta;
        }
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    private Timer FindTimer(string timerID)
    {
        return timers.Find(timer => timer.TimerID == timerID);
    }

    private void DebugTimers()
    {
        if (debugText == null) return;

        string debugInfo = "Timers:\n";
        foreach (Timer timer in timers)
        {
            debugInfo += $"ID: {timer.TimerID}, Remaining: {FormatTime(timer.RemainingTime)}, Paused: {timer.IsPaused}\n";
        }
        debugText.text = debugInfo;
    }

    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
