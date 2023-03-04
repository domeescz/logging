using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler
{
    public class JobMetadata
    {
        //internal TimeZoneInfo TimeZone;

        public Guid JobId { get; set; }
        public Type JobType { get; }
        public string JobName { get; }
        public string CronExpression { get; }
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

        public JobMetadata(Guid Id, Type jobType, string jobName,
        string cronExpression)
        {
            JobId = Id;
            JobType = jobType;
            JobName = jobName;
            CronExpression = cronExpression;
        }
    }
}