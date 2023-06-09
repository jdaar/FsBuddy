﻿using System;
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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Service
{
    class PipeManager : BackgroundService
    {
        private readonly WatcherManager _watcherManager;
        private readonly ManagerConfiguration _configurationManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;
        private readonly NamedPipeServerStream _pipeServer;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;

        const string PIPE_NAME = "fsbuddy-service";

        private readonly Dictionary<PipeCommand, Func<PipeRequest, Task<PipeResponse>>> Handlers;

        public PipeManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _serviceScope = _serviceProvider.CreateScope();

            _watcherManager = _serviceScope.ServiceProvider.GetRequiredService<WatcherManager>();
            _configurationManager = _serviceScope.ServiceProvider.GetRequiredService<ManagerConfiguration>();
            _pipeServer = PipeFactory();

            Handlers = new() {
                { PipeCommand.CREATE_WATCHER, HandleCreateWatcher },
                { PipeCommand.UPDATE_WATCHER, HandleUpdateWatcher },
                { PipeCommand.DELETE_WATCHER, HandleDeleteWatcher },
                { PipeCommand.GET_ALL_WATCHER, HandleGetAllWatcher },
                { PipeCommand.GET_WATCHER, HandleGetWatcher }
            };

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

        private async Task<PipeResponse> HandleCreateWatcher(PipeRequest pipeRequest)
        {
            if (pipeRequest?.Payload?.WatcherData == null)
            {
                return new PipeResponse
                {
                    Status = ResponseStatus.FAILURE,
                    Payload = new PipeResponsePayload {
                        ErrorMessage = "Watcher data not defined"
                    }
                };
            }
            Log.Information("Creating watcher {@WatcherData}", pipeRequest.Payload.WatcherData);

            await _configurationManager.CreateWatcher(pipeRequest.Payload.WatcherData);
            var watchers = await _configurationManager.GetWatchers();
            _watcherManager.RefreshWatchers(watchers);

            return new PipeResponse
            {
                Status = ResponseStatus.SUCCESS,
                Payload = new PipeResponsePayload { }
            };
        }
        private async Task<PipeResponse> HandleUpdateWatcher(PipeRequest pipeRequest)
        { 
            if (pipeRequest?.Payload?.WatcherId == null || pipeRequest?.Payload?.WatcherData == null)
            {
                return new PipeResponse
                {
                    Status = ResponseStatus.FAILURE,
                    Payload = new PipeResponsePayload {
                        ErrorMessage = "Watcher id or data not defined"
                    }
                };
            }

            Log.Information("Getting watcher with id {WatcherId}", pipeRequest.Payload.WatcherId);
            await _configurationManager.UpdateWatcher(pipeRequest.Payload.WatcherId ?? -1, pipeRequest.Payload.WatcherData);

            var watchers = await _configurationManager.GetWatchers();
            _watcherManager.RefreshWatchers(watchers);

            return new PipeResponse
            {
                Status = ResponseStatus.SUCCESS,
                Payload = new PipeResponsePayload { 
                }
            };
        }
        private async Task<PipeResponse> HandleDeleteWatcher(PipeRequest pipeRequest)
        {
            if (pipeRequest?.Payload?.WatcherId == null)
            {
                return new PipeResponse
                {
                    Status = ResponseStatus.FAILURE,
                    Payload = new PipeResponsePayload {
                        ErrorMessage = "Watcher id is not defined"
                    }
                };
            }

            Log.Information("Deleting watcher with id {WatcherId}", pipeRequest.Payload.WatcherId);
            await _configurationManager.DeleteWatcher(pipeRequest.Payload.WatcherId ?? -1);

            var watchers = await _configurationManager.GetWatchers();
            _watcherManager.RefreshWatchers(watchers);

            return new PipeResponse
            {
                Status = ResponseStatus.SUCCESS,
                Payload = new PipeResponsePayload { 
                }
            };
        }
        private async Task<PipeResponse> HandleGetAllWatcher(PipeRequest pipeRequest)
        {
            Log.Information("Getting all watchers");
            var watchers = await _configurationManager.GetWatchers();

            return new PipeResponse
            {
                Status = ResponseStatus.SUCCESS,
                Payload = new PipeResponsePayload {
                    Watchers = watchers
                }
            };
        }


        private async Task<PipeResponse> HandleGetWatcher(PipeRequest pipeRequest)
        {
            if (pipeRequest?.Payload?.WatcherId == null)
            {
                return new PipeResponse
                {
                    Status = ResponseStatus.FAILURE,
                    Payload = new PipeResponsePayload {
                        ErrorMessage = "Watcher id not defined"
                    }
                };
            }

            Log.Information("Getting watcher with id {WatcherId}", pipeRequest.Payload.WatcherId);
            var watchers = new List<Watcher>() {
                await _configurationManager.GetWatcher(pipeRequest.Payload.WatcherId ?? -1)
            };
            return new PipeResponse
            {
                Status = ResponseStatus.SUCCESS,
                Payload = new PipeResponsePayload { 
                    Watchers = watchers
                }
            };

        }
        

        private async Task<PipeResponse> ProcessPipeRequest(PipeRequest pipeRequest)
        {
            var handler = Handlers.GetValueOrDefault(pipeRequest.Command);

            if (handler == null)
            {
                return new PipeResponse
                {
                    Status = ResponseStatus.FAILURE,
                    Payload = new PipeResponsePayload
                    {
                        ErrorMessage = "Command not defined"
                    }
                };
            }
            
            return await handler(pipeRequest);
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

                    PipeRequest? request;
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
