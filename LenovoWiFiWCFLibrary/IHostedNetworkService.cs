﻿using System.ServiceModel;

namespace Lenovo.WiFi
{
    [ServiceContract]
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
        void StartHostedNetwork();

        [OperationContract]
        void StopHostedNetwork();
    }
}
