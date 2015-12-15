namespace AirBand
{
    public static class Switcher
    {
        public static PageSwitcher pageSwitcher;

        public static VM_EnvironmentVariables VM_EnvironmentVariables = new VM_EnvironmentVariables();

        public static void Switch(System.Windows.Controls.UserControl newPage)
        {
            pageSwitcher.Navigate(newPage);
        }
    }
}