using Service;
using Serilog;
using Configuration;
using Serilog.Formatting.Compact;

FileSystemService service = new FileSystemService();

await service.RunAsync();
