using System;
using Quartz;
using System.Threading.Tasks;
namespace Scheduler
{
	public class NotificationJob :IJob
	{
            private readonly ILogger<NotificationJob> _logger;

            public NotificationJob(ILogger<NotificationJob> logger)
        {
            this._logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Notify user at {DateTime.Now} and Jobtype: {context.JobDetail.JobType}");
            return Task.CompletedTask;
        }
		}
    }
