﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

using NLog;

namespace Lenovo.WiFi.Client.Model
{
    internal class DeskbandPipeServer : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string PipeName = "LenovoWiFi";

        private readonly NamedPipeServerStream _pipeServerStream;

        internal DeskbandPipeServer()
        {
            Logger.Trace(".ctor: Invoked");
            _pipeServerStream = new NamedPipeServerStream(
                PipeName, 
                PipeDirection.InOut, 
                1, 
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);

            Logger.Trace(".ctor: Named pipe created");
        }

        public void Dispose()
        {
            _pipeServerStream.Dispose();
        }

        internal event EventHandler DeskbandMouseEnter;
        internal event EventHandler DeskbandMouseLeave;
        internal event EventHandler DeskbandLeftButtonClick;
        internal event EventHandler DeskbandRightButtonClick;
        internal event EventHandler DeskbandExit;
        internal event EventHandler HookFinished;

        internal void Start()
        {
            Logger.Trace("Start: Start waiting for deskband...");
            _pipeServerStream.WaitForConnection();

            using (var reader = new StreamReader(_pipeServerStream, Encoding.Unicode))
            {
                while (true)
                {
                    try
                    {
                        var line = reader.ReadLine();
                        var exit = false;
                        Logger.Info("Start: New message arrived: {0}", line);

                        switch (line)
                        {
                            case "mouseenter":
                                OnDeskbandMouseEnter();
                                break;
                            case "mouseleave":
                                OnDeskbandMouseLeave();
                                break;
                            case "lbuttonclick":
                                OnDeskbandLeftButtonClick();
                                break;
                            case "rbuttonclick":
                                OnDeskbandRightButtonClick();
                                break;
                            case "exit":
                                OnDeskbandExit();
                                exit = true;
                                break;
                            case "handshake":
                                OnHookFinished();
                                break;
                        }

                        if (exit)
                        {
                            Logger.Trace("Start: Exiting...");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.TraceException("Start: An exception was catched", e);
                    }
                }
            }
        }

        internal void Send(string message)
        {
            Logger.Trace("Send: Invoked with message: {0}", message);
            if (!_pipeServerStream.IsConnected)
            {
                Logger.Warn("Send: Pipe was disconnected...Message ignored...");
                return;
            }

            using (var writer = new StreamWriter(_pipeServerStream, Encoding.Unicode, 1024, true))
            {
                try
                {
                    writer.WriteLine(message);
                }
                catch (Exception e)
                {
                    Logger.TraceException("Send: An exception was thrown while writing to the pipe", e);
                }
            }
        }

        internal void Stop()
        {
            if (_pipeServerStream.IsConnected)
            {
                _pipeServerStream.Disconnect();
            }
            Logger.Trace("Stop: The pipe was closed");
        }

        private void OnDeskbandMouseEnter()
        {
            if (DeskbandMouseEnter != null)
            {
                DeskbandMouseEnter(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandMouseLeave()
        {
            if (DeskbandMouseLeave != null)
            {
                DeskbandMouseLeave(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandLeftButtonClick()
        {
            if (DeskbandLeftButtonClick != null)
            {
                DeskbandLeftButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandRightButtonClick()
        {
            if (DeskbandRightButtonClick != null)
            {
                DeskbandRightButtonClick(this, EventArgs.Empty);
            }
        }

        private void OnDeskbandExit()
        {
            if (DeskbandExit != null)
            {
                DeskbandExit(this, EventArgs.Empty);
            }
        }

        private void OnHookFinished()
        {
            if (HookFinished != null)
            {
                HookFinished(this, EventArgs.Empty);
            }
        }
    }
}
