#include "stdafx.h"

LPCTSTR MONIKER = L"service:mexAddress=net.pipe://localhost/LenovoWiFi/HostedNetworkService/mex,"
				  L"address=net.pipe://localhost/LenovoWiFi/HostedNetworkService,"
				  L"binding=NetNamedPipeBinding_IHostedNetworkService, bindingNamespace=http://tempuri.org/,"
				  L"contract=IHostedNetworkService, contractNamespace=http://tempuri.org/";

CHostedNetworkClient::CHostedNetworkClient()
{
	Log.i(L"CHostedNetworkClient::CHostedNetworkClient", L"CoInitializeEx begin\n");

	DWORD dwError;
	HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);

	if (FAILED(hr))
	{
		Log.i(L"CHostedNetworkClient::CHostedNetworkClient", L"CoInitializeEx failed: error %d\n", GetLastError());
	}

	hr = CoGetObject(
		MONIKER,
		NULL,
		IID_IDispatch,
		reinterpret_cast<void **>(&m_pProxy));

	if (FAILED(hr))
	{
		Log.i(L"CHostedNetworkClient::CHostedNetworkClient", L"CoGetObject failed: error %d\n", GetLastError());

		BAIL_ON_HRESULT_ERROR(dwError, hr)
	}

	DISPID dispId;
	BSTR szFunc = SysAllocString(TEXT("StartHostedNetwork"));

	hr = m_pProxy->GetIDsOfNames(
		IID_NULL,
		&szFunc,
		1,
		GetUserDefaultLCID(),
		&dispId);

	if (FAILED(hr))
	{
		Log.i(L"CHostedNetworkClient::CHostedNetworkClient", L"GetIDsOfNames failed: error %d\n", GetLastError());

		BAIL_ON_HRESULT_ERROR(dwError, hr)
	}

	m_lStartFuncID = dispId;

	szFunc = SysAllocString(TEXT("StopHostedNetwork"));

	hr = m_pProxy->GetIDsOfNames(
		IID_NULL,
		&szFunc,
		1,
		GetUserDefaultLCID(),
		&dispId);

	if (FAILED(hr))
	{
		Log.i(L"CHostedNetworkClient::CHostedNetworkClient", L"GetIDsOfNames failed: error %d\n", GetLastError());

		BAIL_ON_HRESULT_ERROR(dwError, hr)
	}

	m_lStopFuncID = dispId;

	return;
ERROR_LABEL:

	CoUninitialize();
	throw dwError;
}

CHostedNetworkClient::~CHostedNetworkClient()
{
	CoUninitialize();
}

STDMETHODIMP CHostedNetworkClient::StartHostedNetwork()
{
	DISPPARAMS dispparamsNoArgs = { NULL, NULL, 0, 0 };
	EXCEPINFO excepInfo;
	memset(&excepInfo, 0, sizeof(EXCEPINFO));
	UINT uArgErr;

	VARIANTARG lpvVoid;
	VariantInit(&lpvVoid);

	return m_pProxy->Invoke(
		m_lStartFuncID,
		IID_NULL,
		GetUserDefaultLCID(),
		DISPATCH_METHOD,
		&dispparamsNoArgs, &lpvVoid, &excepInfo, &uArgErr);
}

STDMETHODIMP CHostedNetworkClient::StopHostedNetwork()
{
	DISPPARAMS dispparamsNoArgs = { NULL, NULL, 0, 0 };
	EXCEPINFO excepInfo;
	memset(&excepInfo, 0, sizeof(EXCEPINFO));
	UINT uArgErr;

	VARIANTARG lpvVoid;
	VariantInit(&lpvVoid);

	return m_pProxy->Invoke(
		m_lStopFuncID,
		IID_NULL,
		GetUserDefaultLCID(),
		DISPATCH_METHOD,
		&dispparamsNoArgs, &lpvVoid, &excepInfo, &uArgErr);
}

STDMETHODIMP CHostedNetworkClient::RestartHostedNetwork()
{
	HRESULT hResult = StopHostedNetwork();

	BAIL_ON_FAILURE(hResult)

	hResult = StartHostedNetwork();

ERROR_LABEL:
	return hResult;
}

