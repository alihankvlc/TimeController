using UnityEngine;

namespace _Project.Scripts.TimeSystem
{
    public class TimeData : ISaveable
    {
        [field: SerializeField] public int Minute { get; private set; }
        [field: SerializeField] public int Hours { get; private set; }
        [field: SerializeField] public int Days { get; private set; }

        public void SetData(int minute, int hours, int days)
        {
            this.Minute = minute;
            this.Hours = hours;
            this.Days = days;
        }
    }
}