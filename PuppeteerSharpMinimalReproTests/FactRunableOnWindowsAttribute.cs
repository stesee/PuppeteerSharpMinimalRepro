using System.Runtime.InteropServices;
using Xunit;

namespace PuppeteerSharpMinimalReproTests
{
    internal class FactRunableOnWindowsAttribute : FactAttribute
    {
        public FactRunableOnWindowsAttribute()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            Skip = "Test will only run on windows.";
        }
    }
}