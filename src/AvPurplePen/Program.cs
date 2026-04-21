using Avalonia;

namespace AvPurplePen
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppBootstrap.InitializeBeforeStart();

            AppBootstrap.CreateAppBuilder()
                .UsePlatformDetect()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}
