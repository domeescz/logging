using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Scheduler;

namespace Scheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IJobFactory, JobFactory>();
                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
                    services.AddSingleton<NotificationJob>();
                    services.AddSingleton<SkriptJob>();

                    services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(NotificationJob), "Notify Job", "0/10 * * * * ?"));
                    //services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(SkriptJob), "SkriptJob", "0 47 13 * * ?"));
                    //services.AddSingleton(new JobMetadata(Guid.NewGuid(), typeof(SkriptJob), "SkriptJob", "0 47 14 * * ?"));


                    services.AddHostedService<Schedular>();
                });
    }
}