namespace StudentsFileSharingApp.Utility
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public string FilesPath { get; set; }

        public double MaxFileSizeInMB { get; set; }
    }
}