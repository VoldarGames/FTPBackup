namespace FTPBackup
{
    public record BackupJobSettings(string ToBackupFolderPath, string FTPHost, string User, string Password);
}
