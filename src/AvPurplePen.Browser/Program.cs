using Avalonia.Browser;
using System.Threading.Tasks;

namespace AvPurplePen
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            AppBootstrap.InitializeBeforeStart();

            await AppBootstrap.CreateAppBuilder()
                .UseBrowser()
                .StartBrowserAppAsync("out");
        }
    }
}
