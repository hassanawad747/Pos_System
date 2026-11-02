namespace Pos_System.Services
{
    internal static class BackupScheduleService
    {
        public static string CurrentSchedule()
        {
            return POS_System.Program.SettingsManager.GetSetting("backup_schedule", "Daily");
        }
    }
}
