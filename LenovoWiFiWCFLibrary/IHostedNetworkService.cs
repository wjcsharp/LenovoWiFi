﻿using System.ServiceModel;

namespace Lenovo.WiFi
{
    [ServiceContract(CallbackContract = typeof(IHostedNetworkServiceCallback), SessionMode = SessionMode.Required)]
    public interface IHostedNetworkService
    {
        [OperationContract]
        string GetHostedNetworkName();

        [OperationContract]
        void SetHostedNetworkName(string name);

        [OperationContract]
        string GetHostedNetworkKey();

        [OperationContract]
        void SetHostedNetworkKey(string key);

        [OperationContract]
        string GetHostedNetworkAuthAlgorithm();

        [OperationContract]
        void StartHostedNetwork();

        [OperationContract]
        int GetHostedNetworkConnectedDeviceCount();

        [OperationContract]
        void RegisterForNewConnectedDevice();

        [OperationContract]
        void UnregisterForNewConnectedDevice();

        [OperationContract]
        void StopHostedNetwork();
    }
}
