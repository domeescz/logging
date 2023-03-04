using System;
using Quartz;
using Quartz.Spi;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace Scheduler
{
    class Schedular : IHostedService
    {
        public IScheduler Scheduler { get; set; }
        private readonly IJobFactory jobFactory;
        private readonly JobMetadata jobMetadata;
        private readonly ISchedulerFactory schedulerFactory;

        public Schedular(ISchedulerFactory schedulerFactory, JobMetadata jobMetadata,IJobFactory jobFactory)
        {
            this.jobFactory = jobFactory;
            this.jobMetadata = jobMetadata;
            this.schedulerFactory = schedulerFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //Creating schedular
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            Scheduler = await schedulerFactory.GetScheduler();
            Scheduler.JobFactory = jobFactory;
            Scheduler.Context.Add("timezone", timeZone);

            //Create Job
            IJobDetail jobDetail = CreateJob(jobMetadata);

            //Create trigger
            ITrigger trigger = CreateTrigger(jobMetadata);

            //Schedule job
            await Scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);

            //Start the Schedular
            await Scheduler.Start(cancellationToken);
        }


        private ITrigger CreateTrigger(JobMetadata jobMetadata) { 

            TimeZoneInfo timeZone = (TimeZoneInfo)Scheduler.Context.Get("timezone");
            return TriggerBuilder.Create()
                .WithIdentity(jobMetadata.JobId.ToString())
                .WithDescription(jobMetadata.JobName)
                .Build();
        }

        private IJobDetail CreateJob(JobMetadata jobMetadata)
        {
            return JobBuilder.Create(jobMetadata.JobType)
                .WithIdentity(jobMetadata.JobId.ToString())
                .WithDescription(jobMetadata.JobName)
                .Build();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

