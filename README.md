# FTP Backup

This project implements a simple backup utility using Quartz and SSH.NET to compress a folder and upload it to an FTP server (Tested with IONOS). It uses a scheduled cron job to perform the backup task at regular intervals.

## Features

- **Folder Compression**: Compresses the target folder into a `.zip` archive.
- **FTP Upload**: Uploads the compressed file to a specified FTP server using SFTP.
- **Job Scheduling**: Uses Quartz.NET to schedule the backup job based on a cron expression.
- **Environment Configuration**: Configurable settings such as folder paths, FTP server details, and cron expressions through environment variables.

## How it Works

1. **Compression**: The `BackupJob` class is responsible for compressing a specified folder into a `.zip` archive.
2. **FTP Upload**: After compression, the `.zip` file is uploaded to an FTP server via SFTP.
3. **Cron Scheduling**: A cron expression determines when the backup job should run, allowing for flexible scheduling.
