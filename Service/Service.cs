using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;

if (!System.OperatingSystem.IsWindows())
    throw new PlatformNotSupportedException("Windows only");

FileSystemService service = new();

await service.RunAsync();

