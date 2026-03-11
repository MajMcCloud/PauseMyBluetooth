using System.Runtime.InteropServices;

namespace PauseMyBluetooth;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Ensure we're on Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            MessageBox.Show("Dieses Programm läuft nur unter Windows.", "Fehler",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ApplicationConfiguration.Initialize();

        // Dark title bar on Windows 10/11
        //Application.SetColorMode(SystemColorMode.Dark);

        Application.Run(new frmMainForm());
    }
}
