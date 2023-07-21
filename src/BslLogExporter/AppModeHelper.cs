using Microsoft.Extensions.Hosting.WindowsServices;

namespace LogExporter;

public static class AppModeHelper
{
    public static string GetAppRunMode()
    {
        if (WindowsServiceHelpers.IsWindowsService())
        {
            return "WindowsService";
        }

        return "ConsoleApp";
    }
}