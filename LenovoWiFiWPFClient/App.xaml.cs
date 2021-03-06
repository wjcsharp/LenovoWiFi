﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Lenovo.WiFi.Client.Model;
using Lenovo.WiFi.Client.View;
using Lenovo.WiFi.Client.ViewModel;

using Autofac;
using NLog;
using IContainer = Autofac.IContainer;

namespace Lenovo.WiFi.Client
{
    public partial class App : Application, IDisposable, IHostedNetworkServiceCallback
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Guid CLSIDLenovoWiFiDeskBand = new Guid("{23ED1551-904E-4874-BA46-DBE1489D4D34}");

        private readonly IContainer _container = new Bootstrapper().Build();
        private readonly BackgroundWorker _pipeServerWorker = new BackgroundWorker();
        private readonly DeskbandPipeServer _pipeServer = new DeskbandPipeServer();
        private readonly DeskbandWarningDialogHook _dialogHook = new DeskbandWarningDialogHook();

        private Window _currentWindow;
        private MainWindow _mainWindow;
        private StatusWindow _statusWindow;

        public void Dispose()
        {
            _pipeServerWorker.Dispose();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Trace("OnStartup: Invoked");

            _pipeServerWorker.DoWork += (sender, args) =>
            {
                Logger.Trace("OnStartup: Registering event handlers");
                _pipeServer.DeskbandMouseEnter += (o, eventArgs) => this.Dispatcher.BeginInvoke(new Action(OnMouseEnter));
                _pipeServer.DeskbandMouseLeave += (o, eventArgs) => this.Dispatcher.BeginInvoke(new Action(OnMouseLeave));
                _pipeServer.DeskbandLeftButtonClick += (o, eventArgs) => this.Dispatcher.BeginInvoke(new Action(OnLButtonClick));
                _pipeServer.DeskbandRightButtonClick += (o, eventArgs) => this.Dispatcher.BeginInvoke(new Action(OnRButtonClick));
                _pipeServer.DeskbandExit += (o, eventArgs) => this.Dispatcher.BeginInvoke(new Action(OnExit));
                _pipeServer.HookFinished += (o, eventArgs) => this.Dispatcher.BeginInvoke(new Action(OnDeskbandShown));

                _pipeServer.Start();
            };
            _pipeServerWorker.RunWorkerAsync();

            ShowDeskband();

            var splashWindow = _container.Resolve<SplashWindow>();
            splashWindow.Show();
            splashWindow.Focus();
            Logger.Trace("OnStartup: Splash window in on");

            Task.Run(() =>
            {
                _pipeServer.Send("ics_loading");
                _container.Resolve<ISplashViewModel>().Start();

            }).ContinueWith(t =>
            {
                _pipeServer.Send("ics_on");
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    splashWindow.Close();
                    Logger.Trace("OnStartup: Splash window in off");

                    var successWindow = _container.Resolve<SuccessWindow>();
                    successWindow.DataContext = _container.Resolve<ISuccessViewModel>();
                    successWindow.Show();
                    successWindow.Focus();
                    Logger.Trace("OnStartup: Success window in on");

                    _currentWindow = successWindow;
                }));
            });

            _mainWindow = _container.Resolve<MainWindow>();
            _mainWindow.DataContext = _container.Resolve<IMainViewModel>();

            _statusWindow = _container.Resolve<StatusWindow>();
            _statusWindow.DataContext = _container.Resolve<IStatusViewModel>();
        }

        private void OnMouseEnter()
        {
            if (_currentWindow != null && !(_currentWindow is MainWindow))
            {
                _currentWindow.Hide();
                _currentWindow = null;
            }

            if (_currentWindow == null)
            {
                _currentWindow = _statusWindow;
                _currentWindow.Show();
            }
        }

        private void OnMouseLeave()
        {
            if (_currentWindow != null && !(_currentWindow is MainWindow))
            {
                _currentWindow.Hide();
                _currentWindow = null;
            }
        }

        private void OnLButtonClick()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Hide();
            }

            _currentWindow = _mainWindow;
            _currentWindow.Show();
        }

        private void OnRButtonClick()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Hide();
                _currentWindow = null;
            }
        }

        private void OnExit()
        {
            _pipeServer.Stop();
            _pipeServer.Dispose();

            HideDeskband();

            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        private void OnDeskbandShown()
        {
            _dialogHook.StopHook();
            Logger.Trace("ShowDeskband: Hook was uninstalled");
        }

        private void ShowDeskband()
        {
            Logger.Trace("ShowDeskband: Invoked");
            _dialogHook.StartHook();
            Logger.Trace("ShowDeskband: Hook was installed");

            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();
            trayDeskBand.ShowDeskBand(CLSIDLenovoWiFiDeskBand);
            Logger.Trace("ShowDeskband: Deskband was showed");
        }

        private void HideDeskband()
        {
            Logger.Trace("HideDeskband: Invoked");
            var trayDeskBand = (ITrayDeskBand)new TrayDesktopBand();
            trayDeskBand.HideDeskBand(CLSIDLenovoWiFiDeskBand);
            Logger.Trace("ShowDeskband: Deskband was hid now");
        }

        public void DeviceConnected(byte[] macAddress)
        {
            // _pipeServer.Send("ics_off");
            _pipeServer.Send("ics_clientconnected");
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentWindow != null)
                {
                    _currentWindow.Hide();
                }

                var notification = _container.Resolve<NotificationWindow>();
                notification.DataContext = new NotificationViewModel { Message = BitConverter.ToString(macAddress) };

                notification.Show();
            }));
        }
    }
}
