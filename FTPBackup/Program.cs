using Quartz.Impl;
using Quartz;
using System.Text.Json;

namespace FTPBackup
{
    internal class Program
    {
        private const string BACKUP_JOB_NAME = "Backup Job";
        private const string BACKUP_JOB_GROUP = "Backup";
        private const string BACKUP_JOB_TRIGGERNAME = "Cron Backup Trigger";
        private static IScheduler? _scheduler;

        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += HandleProcessExit;

            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();

            var cronExpression = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.CronExpression);
            var backupJobSettings = new BackupJobSettings(
                Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.ToBackupFolderPath)!,
                Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.FTPHost)!,
                Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.FTPUser)!,
                Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.FTPPassword)!);

            if (string.IsNullOrWhiteSpace(cronExpression))
                throw new ArgumentNullException(Constants.EnvironmentVariables.CronExpression);

            IJobDetail job = JobBuilder.Create<BackupJob>()
                .WithIdentity(BACKUP_JOB_NAME, BACKUP_JOB_GROUP)
                .UsingJobData(BackupJob.Settings, JsonSerializer.Serialize(backupJobSettings))
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(BACKUP_JOB_TRIGGERNAME, BACKUP_JOB_GROUP)
                .WithCronSchedule(cronExpression)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            Console.WriteLine($"Scheduled backup job with cron expression: {cronExpression}");
            Console.ReadLine();
        }

        private static void HandleProcessExit(object? sender, EventArgs e)
        {
            _scheduler?
                .Shutdown()
                .GetAwaiter()
                .GetResult();
        }
    }
}
