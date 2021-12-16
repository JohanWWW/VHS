using System;

namespace VHS.Backend.Entities
{
    public class ResultdriveJournalEntity
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set;}
        public double TotalDistance { get; set; }
        public double EnergyConsumption { get; set; }
        public double AverageSpeed { get; set; }

        /// <summary>
        /// Average energy consumption per 100 km
        /// </summary>
        public double AverageConsumption => (EnergyConsumption / TotalDistance) * 100;
        public double TotalTime => (EndTime - StartTime).TotalSeconds;
        public int LogCount { get; set; }

    }
}

