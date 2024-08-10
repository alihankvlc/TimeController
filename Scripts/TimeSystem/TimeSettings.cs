using System;
using TMPro;
using Zenject;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace _Project.Scripts.TimeSystem
{
    [System.Serializable]
    public class NotifyTimeChangedEvent : UnityEvent
    {
    }

    public interface ITimeSettings
    {
        float Latitude { get; }
        float Longitude { get; }
        int SunsetHour { get; }
        int SunriseHour { get; }
    }

    public interface ITimeProvider
    {
        int Days { get; }
        int Hours { get; }
        int Minute { get; }
    }

    public sealed class TimeSettings : MonoBehaviour, ITimeSettings, ITimeProvider
    {
        [Header("Time Settings")] [SerializeField, Range(0, 59)]
        private int _minute;

        [SerializeField, Range(0, 23)] private int _hours;
        [SerializeField, Range(1, 365)] private int _day;
        [SerializeField, Range(0, 23)] private int _sunSetHour = 18;
        [SerializeField, Range(0, 23)] private int _sunRiseHour = 6;
        [SerializeField] private int _timeScale = 1;

        [Header("Location Settings")] [SerializeField] [Range(-90, 90)]
        private int _latitude = 45;

        [SerializeField] [Range(-180, 180)] private int _longitude = 0;

        [Header("Events")] [SerializeField] private NotifyTimeChangedEvent _onMinuteChanged = new();
        [SerializeField] private NotifyTimeChangedEvent _onHourChanged = new();
        [SerializeField] private NotifyTimeChangedEvent _onDayChanged = new();

        private SaveLoadService _saveLoadService;
        private TimeData _timeData = new();

        private bool _isNight;


        private float _elapsedSeconds;

        public float Latitude => _latitude;
        public float Longitude => _longitude;

        public int SunsetHour => _sunSetHour;
        public int SunriseHour => _sunRiseHour;

        public int Days
        {
            get => _day;
            private set
            {
                _day = value;
                _onDayChanged.Invoke();
            }
        }

        public int Hours
        {
            get => _hours;
            private set
            {
                _hours = value;
                _onHourChanged.Invoke();
            }
        }

        public int Minute
        {
            get => _minute;
            private set
            {
                _minute = value;
                _onMinuteChanged.Invoke();
            }
        }

        [Inject]
        private void Consturctor(SaveLoadService saveLoadService)
        {
            this._saveLoadService = saveLoadService;
            LoadData();
        }

        private void Update()
        {
            _elapsedSeconds += Time.deltaTime * _timeScale;

            if (_elapsedSeconds >= 60)
            {
                IncrementMinute();
                _elapsedSeconds -= 60;
            }
        }

        private void IncrementMinute()
        {
            Minute++;
            if (Minute > 59)
            {
                Minute = 0;
                IncrementHour();
            }
        }

        private void IncrementHour()
        {
            Hours++;
            _isNight = Hours < _sunRiseHour || Hours >= _sunSetHour;

            if (Hours > 23)
            {
                Hours = 0;
                Days++;
            }
        }

        private void LoadData()
        {
            TimeData data = _saveLoadService.LoadData<TimeData>();

            if (data != null)
            {
                this._timeData = data;

                _minute = _timeData.Minute;
                _hours = _timeData.Hours;
                _day = _timeData.Days;
            }
        }

        private void SaveData()
        {
            _timeData.SetData(_minute, _hours, _day);
            _saveLoadService.SaveData(_timeData);
        }

        private void OnApplicationQuit()
        {
            SaveData();
        }
    }
}