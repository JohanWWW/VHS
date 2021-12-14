using System;

namespace VHS.Backend.Entities
{
    public class ResultdriveJournalEntity
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set;}

        public double TotalDistance { get; set; }

        public double EnergyConsumption { get; set; }

        public double AvrageConsumption => EnergyConsumption /TotalTime;

        public double AvrageSpeed => TotalDistance / TotalTime;

        public double TotalTime => (EndTime- StartTime).TotalSeconds;
    }
}

