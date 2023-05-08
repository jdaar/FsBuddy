using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using ConnectionInterface;
using Configuration;
using Serilog;

namespace Service
{
    class PipeManager : BackgroundService
    {
        private readonly WatcherManager _watcherManager;
        private readonly ManagerConfiguration _configurationManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;
        private NamedPipeServerStream _pipeServer;
        private StreamWriter _writer;
        private StreamReader _reader;

        const string PIPE_NAME = "fsbuddy-service";
        const int PIPE_STREAM_END_BYTE = -1;

        public PipeManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _serviceScope = _serviceProvider.CreateScope();

            _watcherManager = _serviceScope.ServiceProvider.GetRequiredService<WatcherManager>();
            _configurationManager = _serviceScope.ServiceProvider.GetRequiredService<ManagerConfiguration>();
            _pipeServer = PipeFactory();

            _writer = new StreamWriter(_pipeServer);
            _reader = new StreamReader(_pipeServer);
        }

        ~PipeManager()
        {
            _pipeServer.Dispose();
            _serviceScope.Dispose();
            _reader.Dispose();
            _writer.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows only app")]
        private static NamedPipeServerStream PipeFactory()
        {
            var pipe = new NamedPipeServerStream(
                PIPE_NAME,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous,
                0x4000,
                0x400
            );

            var pipeSecurity = pipe.GetAccessControl();

            var id = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

            pipeSecurity.AddAccessRule(
                new PipeAccessRule(
                    id,
                    PipeAccessRights.ReadWrite,
                    System.Security.AccessControl.AccessControlType.Allow
                )
            );

            return pipe;
        }


        private async Task SendPipeResponse(PipeResponse pipeResponse)
        {
            var responseString = await PipeSerializer.SerializeResponse(pipeResponse);
            _writer.WriteLine(responseString);
            await _writer.FlushAsync();
        }

        private async Task<PipeResponse> ProcessPipeRequest(PipeRequest pipeRequest)
        {
            List<Watcher> watchers;
            switch (pipeRequest.Command)
            {
                case t_PipeCommand.CREATE_WATCHER:
                    if (pipeRequest?.Payload?.WatcherData == null)
                    {
                        return new PipeResponse
                        {
                            Status = t_ResponseStatus.FAILURE,
                            Payload = new PipeResponsePayload {
                                ErrorMessage = "Watcher data not defined"
                            }
                        };
                    }
                    Log.Information("Creating watcher {@WatcherData}", pipeRequest.Payload.WatcherData);

                    await _configurationManager.CreateWatcher(pipeRequest.Payload.WatcherData);
                    watchers = await _configurationManager.GetWatchers();
                    _watcherManager.RefreshWatchers(watchers);

                    return new PipeResponse
                    {
                        Status = t_ResponseStatus.SUCCESS,
                        Payload = new PipeResponsePayload { }
                    };
                case t_PipeCommand.GET_ALL_WATCHER:
                    Log.Information("Getting all watchers");
                    watchers = await _configurationManager.GetWatchers();

                    return new PipeResponse
                    {
                        Status = t_ResponseStatus.SUCCESS,
                        Payload = new PipeResponsePayload {
                            Watchers = watchers
                        }
                    };
                case t_PipeCommand.GET_WATCHER:
                    if (pipeRequest?.Payload?.WatcherId == null)
                    {
                        return new PipeResponse
                        {
                            Status = t_ResponseStatus.FAILURE,
                            Payload = new PipeResponsePayload {
                                ErrorMessage = "Watcher id not defined"
                            }
                        };
                    }

                    Log.Information("Getting watcher with id {WatcherId}", pipeRequest.Payload.WatcherId);
                    watchers = new List<Watcher>() {
                        await _configurationManager.GetWatcher(pipeRequest.Payload.WatcherId ?? -1)
                    };
                    return new PipeResponse
                    {
                        Status = t_ResponseStatus.SUCCESS,
                        Payload = new PipeResponsePayload { 
                            Watchers = watchers
                        }
                    };
                default:
                    return new PipeResponse
                    {
                        Status = t_ResponseStatus.FAILURE,
                        Payload = new PipeResponsePayload
                        {
                            ErrorMessage = "Command not defined"
                        }
                    };
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                Log.Information("Waiting for client connection...");
                await _pipeServer.WaitForConnectionAsync(stoppingToken);
                Log.Information("Client connected");

                while (_pipeServer.IsConnected)
                {
                    var line = await _reader.ReadLineAsync();

                    if (line == null) break;

                    Log.Information("Received: {line}", line);

                    var request = default(PipeRequest);
                    try
                    {
                        request = await PipeSerializer.DeserializeRequest(line);
                    } catch (Exception error)
                    {
                        Log.Error(error, "Error parsing request");
                        await SendPipeResponse(PipeSerializer.ErrorFactory(error.Message));
                        continue;
                    }

                    if (request == null)
                    {
                        Log.Error("Error processing request");
                        await SendPipeResponse(PipeSerializer.ErrorFactory("Request body has to be set"));
                        continue;
                    }

                    Log.Information("Request: {@request}", request);

                    var response = await ProcessPipeRequest(request);

                    var responseString = await PipeSerializer.SerializeResponse(response);

                    _writer.WriteLine(responseString);

                    try
                    {
                        await _writer.FlushAsync();
                    } catch (Exception e)
                    {
                        Log.Error(e, "Error flushing pipe");
                        break;
                    }
                }

                _pipeServer.Disconnect();
            }
        }
    }
}
