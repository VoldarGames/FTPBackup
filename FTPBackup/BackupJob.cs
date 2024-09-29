using Quartz;
using Renci.SshNet;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace FTPBackup
{
    public class BackupJob : IJob
    {
        public const string Settings = "Settings";
        public async Task Execute(IJobExecutionContext context)
        {
            var contextSettings = context.JobDetail.JobDataMap.Get(Settings)!.ToString()!;
            var backupJobSettings = JsonSerializer.Deserialize<BackupJobSettings>(contextSettings);

            if (backupJobSettings == null)
                throw new ArgumentNullException(nameof(BackupJobSettings));

            var toBackupFolderPathDirectoryInfo = new DirectoryInfo(backupJobSettings.ToBackupFolderPath);
            if (!toBackupFolderPathDirectoryInfo.Exists)
                throw new DirectoryNotFoundException(backupJobSettings.ToBackupFolderPath);

            Console.WriteLine($"Folder to backup: {backupJobSettings.ToBackupFolderPath} ...");

            var zipPath = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{toBackupFolderPathDirectoryInfo.Name}.zip")}";
            Console.WriteLine($"Zip path: {zipPath}");
            Console.WriteLine($"Ftp Host: {backupJobSettings.FTPHost}");
            Console.WriteLine($"Ftp User: {backupJobSettings.User}");
            Console.WriteLine($"Is Ftp Password set: {!string.IsNullOrWhiteSpace(backupJobSettings.Password)}");

            try
            {
                CompressFolder(backupJobSettings.ToBackupFolderPath, zipPath);

                await UploadFileToFtp(zipPath, backupJobSettings.FTPHost, backupJobSettings.User, backupJobSettings.Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void CompressFolder(string folderPath, string zipPath)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.WriteLine($"Compressing {folderPath} folder...");

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(folderPath, zipPath);
            stopWatch.Stop();

            Console.WriteLine($"{folderPath} folder compressed at {zipPath} in {stopWatch.ElapsedMilliseconds} ms");
        }

        private static async Task UploadFileToFtp(string filePath, string ftpHost, string ftpUser, string ftpPassword)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            FileInfo fileInfo = new FileInfo(filePath);

            using (var ftpClient = new SftpClient(ftpHost, ftpUser, ftpPassword))
            {
                ftpClient.Connect();

                using (var fileStream = File.OpenRead(filePath))
                {
                    bool uploadFinished = false;
                    void FinishUpload(IAsyncResult asyncResult)
                    {
                        stopWatch.Stop();
                        Console.WriteLine($"Backup successfully uploaded in {stopWatch.Elapsed.TotalSeconds} seconds.");

                        uploadFinished = true;
                    }

                    var responseStatus = ftpClient.BeginUploadFile(fileStream, fileInfo.Name, true, FinishUpload, fileStream, uploadedBytes =>
                    {
                        Console.WriteLine($"Uploaded [{uploadedBytes}] Bytes of [{fileInfo.Length}]");
                    });

                    while (!uploadFinished)
                    {
                        await Task.Delay(150);
                    }
                }
            }
        }
    }
}
