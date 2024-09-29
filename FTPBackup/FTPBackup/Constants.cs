namespace FTPBackup
{
    public static class Constants
    {
        public static class EnvironmentVariables
        {
            public const string CronExpression = "FTPBACKUP_CRONEXPRESSION";
            public const string ToBackupFolderPath = "FTPBACKUP_TOBACKUPFOLDER";
            public const string FTPHost = "FTPBACKUP_HOST";
            public const string FTPUser = "FTPBACKUP_USER";
            public const string FTPPassword = "FTPBACKUP_PASSWORD";
        }
    }
}
