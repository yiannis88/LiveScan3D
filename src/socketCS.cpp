/**
 *  This class is developed for non blocking sockets.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#define _WINSOCK_DEPRECATED_NO_WARNINGS

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif


#include "SocketCS.h"
#include <iostream>
#include <filesystem>
#include <sstream>      // std::stringstream
#include <chrono>
#include <thread>         // std::thread

#pragma comment(lib, "iphlpapi.lib") // Need to link with Iphlpapi.lib
#pragma comment(lib, "ws2_32.lib") // Need to link with Ws2_32.lib
#pragma warning (disable:6101) // Warning for the GetTcpRow() and the *clientConnectRow 

// An array of name for the TCP_ESTATS_TYPE enum values
// The names values must match the enum values
wchar_t* estatsTypeNames[] = {
	L"TcpConnectionEstatsSynOpts",
	L"TcpConnectionEstatsData",
	L"TcpConnectionEstatsSndCong",
	L"TcpConnectionEstatsPath",
	L"TcpConnectionEstatsSendBuff",
	L"TcpConnectionEstatsRec",
	L"TcpConnectionEstatsObsRec",
	L"TcpConnectionEstatsBandwidth",
	L"TcpConnectionEstatsFineRtt",
	L"TcpConnectionEstatsMaximum"
};

SocketClient::SocketClient(const std::string& host, int port, int clientId)
	: m_nofSockets(0),
	  m_sock(NULL),
	  m_clientId(0),
	  m_lPort(0),
	  m_hPort(0),
	  m_sockSuccess(FALSE)
{
	bool bWSAStartup = false;
	UINT winStatus;

	m_tsPointer = new Timestamp();
	m_logOutput = new LoggingInfo();

	//
	// Initialize Winsock.
	//
	if (!m_nofSockets) {
		WSADATA info;
		winStatus = WSAStartup(MAKEWORD(2, 0), &info);
		if (winStatus != ERROR_SUCCESS) {
			wprintf(L"\nFailed to open winsock. Error %d", winStatus);
			goto bail;
		}
	}

	bWSAStartup = true;

	//
	// Create TCP connection on which Estats information will be collected.
	// Obtain port numbers of created connections.
	//
	u_short clientPort;
	winStatus =
		CreateTcpConnection(host, port, &clientPort);
	if (winStatus != ERROR_SUCCESS) {
		wprintf(L"\nFailed to create TCP connection. Error %d", winStatus);
		goto bail;
	}
	m_lPort = clientPort;
	m_hPort = htons(port); // we need it in network byte order (to cast back then ntohs(m_hPort))
	//
	// Create TCP connection on which Estats information will be collected.
	// Obtain port numbers of created connections.
	//
	m_nofSockets++;
	m_clientId = clientId;
	
	if (m_sock == INVALID_SOCKET) {
		wprintf(L"\nsocket failed on the client");
		goto bail;
	}

	{
		std::stringstream ss;
		std::filesystem::path pathV1 = std::filesystem::current_path();
		std::string pth_str = pathV1.string() + "\\logging_output";
		std::filesystem::path pathV2 = (pth_str);

		long dateTs = m_tsPointer->GetDateTs();
		std::string secTs = m_tsPointer->GetSecondsTs();

		if (std::filesystem::exists(pathV2))
		{
			ss << pth_str << "\\OutputFileClientSocket_" << m_clientId << "_" << ntohs(m_lPort) << "_" << ntohs(m_hPort) << "_" << dateTs << "_" << secTs << ".txt";
			m_logOutput->SetPath(ss.str());
		}
		else
		{
			std::filesystem::create_directory(pathV2);
			ss << pth_str << "\\OutputFileClientSocket_" << m_clientId << "_" << ntohs(m_lPort) << "_" << ntohs(m_hPort) << "_" << dateTs << "_" << secTs << ".txt";
			m_logOutput->SetPath(ss.str());
			m_logOutput->RedOutput("Directory and File created now");
		}

		std::stringstream ssb;
		ssb << "LocalTime\tActiveOpen\tMssRcvd\tMssSent\tDataBytesOut\tDataSegsOut\tDataBytesIn\tDataSegsIn\tSegsOut\tSegsIn\tSoftErrors\tSoftErrorReason\tSndUna";
		ssb << "\tSndNxt\tSndMax\tThruBytesAcked\tRcvNxt\tThruBytesReceived\tSndLimTransRwin\tSndLimTimeRwin\tSndLimBytesRwin\tSndLimTransCwnd\tSndLimTimeCwnd\tSndLimBytesCwnd\tSndLimTransSnd";
		ssb << "\tSndLimTimeSnd\tSndLimBytesSnd\tSlowStart\tCongAvoid\tOtherReductions\tCurCwnd\tMaxSsCwnd\tMaxCaCwnd\tCurSsthresh\tMaxSsthresh\tMinSsthresh\tLimCwnd\tFastRetran\tTimeouts";
		ssb << "\tSubsequentTimeouts\tCurTimeoutCount\tAbruptTimeouts\tPktsRetrans\tBytesRetrans\tDupAcksIn\tSacksRcvd\tSackBlocksRcvd\tCongSignals\tPreCongSumCwnd\tPreCongSumRtt";
		ssb << "\tPostCongSumRtt\tPostCongCountRtt\tEcnSignals\tEceRcvd\tSendStall\tQuenchRcvd\tRetranThresh\tSndDupAckEpisodes\tSumBytesReordered\tNonRecovDa\tNonRecovDaEpisodes";
		ssb << "\tAckAfterFr\tDsackDups\tSampleRtt\tSmoothedRtt\tRttVar\tMaxRtt\tMinRtt\tSumRtt\tCountRtt\tCurRto\tMaxRto\tMinRto\tCurMss\tMaxMss\tMinMss\tSpuriousRtoDetections";
		ssb << "\tCurRetxQueue\tMaxRetxQueue\tCurAppWQueue\tMaxAppWQueue\tCurRwinSent\tMaxRwinSent\tMinRwinSent\tLimRwin\tDupAckEpisodes\tDupAcksOut\tCeRcvd\tEcnSent\tEcnNoncesRcvd";
		ssb << "\tCurReasmQueue\tMaxReasmQueue\tCurAppRQueue\tMaxAppRQueue\tWinScaleSent\tCurRwinRcvd\tMaxRwinRcvd\tMinRwinRcvd\tWinScaleRcvd\tOutboundBandwidth\tInboundBandwidth";
		ssb << "\tOutboundInstability\tInboundInstability\tOutboundBandwidthPeaked\tInboundBandwidthPeaked\tRttVar\tMaxRtt\tMinRtt\tSumRtt";
		m_logOutput->RedOutput(ssb.str());
	}
	m_sockSuccess = true;
	SetUpStatistics();	

	return;

bail:
  if (m_sock != INVALID_SOCKET)
	  closesocket(m_sock);
  if (bWSAStartup) {
	  WSACleanup();
  }
  return;
}

void
SocketClient::SetUpStatistics()
{
	MIB_TCPROW client4ConnectRow;
	void* clientConnectRow = NULL;
	clientConnectRow = &client4ConnectRow;

	//this is for the statistics
	UINT winStatus = GetTcpRow(m_lPort, m_hPort, MIB_TCP_STATE_ESTAB, (PMIB_TCPROW)clientConnectRow); //lPort & hPort in htons!
	if (winStatus != ERROR_SUCCESS) {
		wprintf(L"\nGetTcpRow failed on the client established connection with %d", winStatus);
		return;
	}	
	//
	// Enable Estats collection and dump current stats.
	//
	ToggleAllEstats(clientConnectRow, TRUE);
	TcpStatistics(clientConnectRow);
}

void
SocketClient::CloseUpStatistics()
{
	MIB_TCPROW client4ConnectRow;
	void* clientConnectRow = NULL;
	clientConnectRow = &client4ConnectRow;

	//this is for the statistics
	UINT winStatus = GetTcpRow(m_lPort, m_hPort, MIB_TCP_STATE_ESTAB, (PMIB_TCPROW)clientConnectRow);
	if (winStatus != ERROR_SUCCESS) {
		wprintf(L"\nGetTcpRow failed on the client established connection with %d", winStatus);
		return;
	}
	//
	// Enable Estats collection and dump current stats.
	//
	std::stringstream result;
	result << m_tsPointer->GetMillisecondsTs(0) << "\tShutting it down!";
	m_logOutput->RedOutput(result.str());
	ToggleAllEstats(clientConnectRow, TRUE); // get the latest stats before shutting it down
	TcpStatistics(clientConnectRow);
	ToggleAllEstats(clientConnectRow, FALSE);
}

void 
SocketClient::SendBytes(const char* buf, int len) {
	/**
	 * s_: the connected socket
	 * buf: pointer to the character buffer that contains the data
	 * len: the number of characters in the buffer to send
	 * 0: The 0 flag allows you to use a regular recv(), with a standard behavior (i.e. not options defined).
	 */
	int _bytesSent = 0;
	while (len > 0)
	{
		_bytesSent = send(m_sock, buf, len, 0);
		if (_bytesSent == SOCKET_ERROR)
			break;
		buf += _bytesSent;
		len -= _bytesSent;
	}
}

std::string
SocketClient::ReceiveBytes() {
	std::string ret;
	char buf[1024];
	while (1) {
		u_long arg = 0;
		if (ioctlsocket(m_sock, FIONREAD, &arg) != 0)
			break;

		if (arg == 0)
			break;

		if (arg > 1024) arg = 1024;

		int rv = recv(m_sock, buf, arg, 0);
		if (rv <= 0) break;

		std::string t;

		t.assign(buf, rv);
		ret += t;
	}
	return ret;
}

void 
SocketClient::CloseConnection()
{
	//Close the socket if it exists
	if (m_sock)
	{
		CloseUpStatistics();
		closesocket(m_sock);
	}
	m_nofSockets = 0;
	m_sockSuccess = false;
	WSACleanup(); //Clean up Winsock
}

SOCKET
SocketClient::GetSocket()
{
	return m_sock;
}

int 
SocketClient::CreateTcpConnection(const std::string& host, u_short serverPort, u_short* clientPort)
{
	INT status;
	ADDRINFOW hints, * localhost = NULL;
	std::wstring widestr = std::wstring(host.begin(), host.end());
	const wchar_t* loopback = widestr.c_str();
	wchar_t sPort[256];
	swprintf_s(sPort, L"%d", serverPort);
	int _addrFamily = AF_INET;
	int _socketType = SOCK_STREAM; // UDP: use SOCK_DGRAM instead of SOCK_STREAM
	int _protocolType = IPPROTO_TCP;

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = _addrFamily;
	hints.ai_socktype = _socketType;
	hints.ai_protocol = _protocolType;


	status = GetAddrInfoW(loopback, sPort, &hints, &localhost);
	if (status != ERROR_SUCCESS) {
		wprintf(L"\nFailed to open localhost. Error %d", status);
		goto bail;
	}

	m_sock = socket(_addrFamily, _socketType, _protocolType);
	if (m_sock == INVALID_SOCKET) {
		wprintf(L"\nFailed to create client socket. Error %d",
			WSAGetLastError());
		goto bail;
	}

	hostent* he;
	if ((he = gethostbyname(host.c_str())) == 0) {
		throw "GetHostByName error";
	}

	if (localhost != NULL) {
		FreeAddrInfoW(localhost);
		localhost = NULL;
	}

	sockaddr_in addr;
	addr.sin_family = _addrFamily;
	addr.sin_port = htons(serverPort);
	addr.sin_addr = *((in_addr*)he->h_addr);
	memset(&(addr.sin_zero), 0, 8);

	SOCKADDR_STORAGE clientSockName;
	int nameLen = sizeof(SOCKADDR_STORAGE);

	status = connect(m_sock, (sockaddr*)& addr, sizeof(sockaddr));
	if (status == SOCKET_ERROR) {
		wprintf(L"\nCould not connect client and server sockets %d",
			WSAGetLastError());
		goto bail;
	}

	status = getsockname(m_sock, (sockaddr*)& clientSockName, &nameLen);
	if (status == SOCKET_ERROR) {
		wprintf(L"\ngetsockname failed %d", WSAGetLastError());
		goto bail;
	}
	{
		*clientPort = ((sockaddr_in*)(&clientSockName))->sin_port;
		bool iOptVal = true;
		int iOptLen = sizeof(int);
		int iResult = getsockopt(m_sock, SOL_SOCKET, SO_KEEPALIVE, (char*)&iOptVal, &iOptLen);
		if (iResult == SOCKET_ERROR) {
			wprintf(L"getsockopt for SO_KEEPALIVE failed with error: %u\n", WSAGetLastError());
		}
		else
			wprintf(L"SO_KEEPALIVE Value: %ld\n", iOptVal);

		iResult = getsockopt(m_sock, SOL_SOCKET, TCP_NODELAY, (char*)&iOptVal, &iOptLen);
		if (iResult == SOCKET_ERROR) {
			wprintf(L"getsockopt for TCP_NODELAY failed with error: %u\n", WSAGetLastError());
		}
		else
			wprintf(L"TCP_NODELAY Value: %ld\n", iOptVal);
	}
	return ERROR_SUCCESS;

bail:
	if (localhost != NULL)
	{
		FreeAddrInfoW(localhost);
	}		
	if (m_sock != INVALID_SOCKET) {
		closesocket(m_sock);
		m_sock = INVALID_SOCKET;
	}
	return status;
}

//
// Returns a MIB_TCPROW corresponding to the local port, remote port and state
// filter parameters.
//
DWORD
SocketClient::GetTcpRow(u_short localPort, u_short remotePort, MIB_TCP_STATE state, __out PMIB_TCPROW row)
{
	PMIB_TCPTABLE tcpTable = NULL;
	PMIB_TCPROW tcpRowIt = NULL;

	DWORD status, size = 0, i;
	bool connectionFound = FALSE;

	status = GetTcpTable(tcpTable, &size, TRUE);
	if (status != ERROR_INSUFFICIENT_BUFFER) {
		return status;
	}

	tcpTable = (PMIB_TCPTABLE)malloc(size);
	if (tcpTable == NULL) {
		return status;
	}

	status = GetTcpTable(tcpTable, &size, TRUE);
	if (status != ERROR_SUCCESS) {
		free(tcpTable);
		return status;
	}

	for (i = 0; i < tcpTable->dwNumEntries; i++) {
		tcpRowIt = &tcpTable->table[i];
		if (tcpRowIt->dwLocalPort == (DWORD)localPort &&
			tcpRowIt->dwRemotePort == (DWORD)remotePort &&
			tcpRowIt->State == state) {
			connectionFound = TRUE;
			*row = *tcpRowIt;
			break;
		}
	}

	free(tcpTable);

	if (connectionFound) {
		return ERROR_SUCCESS;
	}
	else {
		return ERROR_NOT_FOUND;
	}
}

//
// Toggle all Estats for a TCP connection.
//
void 
SocketClient::ToggleAllEstats(void* row, bool enable)
{
	ToggleEstat(row, TcpConnectionEstatsData, enable);
	ToggleEstat(row, TcpConnectionEstatsSndCong, enable);
	ToggleEstat(row, TcpConnectionEstatsPath, enable);
	ToggleEstat(row, TcpConnectionEstatsSendBuff, enable);
	ToggleEstat(row, TcpConnectionEstatsRec, enable);
	ToggleEstat(row, TcpConnectionEstatsObsRec, enable);
	ToggleEstat(row, TcpConnectionEstatsBandwidth, enable);
	ToggleEstat(row, TcpConnectionEstatsFineRtt, enable);
}

//
// Enable or disable the supplied Estat type on a TCP connection.
//
void 
SocketClient::ToggleEstat(PVOID row, TCP_ESTATS_TYPE type, bool enable)
{
	TCP_BOOLEAN_OPTIONAL operation =
		enable ? TcpBoolOptEnabled : TcpBoolOptDisabled;
	ULONG status, size = 0;
	PUCHAR rw = NULL;
	TCP_ESTATS_DATA_RW_v0 dataRw;
	TCP_ESTATS_SND_CONG_RW_v0 sndRw;
	TCP_ESTATS_PATH_RW_v0 pathRw;
	TCP_ESTATS_SEND_BUFF_RW_v0 sendBuffRw;
	TCP_ESTATS_REC_RW_v0 recRw;
	TCP_ESTATS_OBS_REC_RW_v0 obsRecRw;
	TCP_ESTATS_BANDWIDTH_RW_v0 bandwidthRw;
	TCP_ESTATS_FINE_RTT_RW_v0 fineRttRw;

	switch (type) {
	case TcpConnectionEstatsData:
		dataRw.EnableCollection = enable;
		rw = (PUCHAR)& dataRw;
		size = sizeof(TCP_ESTATS_DATA_RW_v0);
		break;

	case TcpConnectionEstatsSndCong:
		sndRw.EnableCollection = enable;
		rw = (PUCHAR)& sndRw;
		size = sizeof(TCP_ESTATS_SND_CONG_RW_v0);
		break;

	case TcpConnectionEstatsPath:
		pathRw.EnableCollection = enable;
		rw = (PUCHAR)& pathRw;
		size = sizeof(TCP_ESTATS_PATH_RW_v0);
		break;

	case TcpConnectionEstatsSendBuff:
		sendBuffRw.EnableCollection = enable;
		rw = (PUCHAR)& sendBuffRw;
		size = sizeof(TCP_ESTATS_SEND_BUFF_RW_v0);
		break;

	case TcpConnectionEstatsRec:
		recRw.EnableCollection = enable;
		rw = (PUCHAR)& recRw;
		size = sizeof(TCP_ESTATS_REC_RW_v0);
		break;

	case TcpConnectionEstatsObsRec:
		obsRecRw.EnableCollection = enable;
		rw = (PUCHAR)& obsRecRw;
		size = sizeof(TCP_ESTATS_OBS_REC_RW_v0);
		break;

	case TcpConnectionEstatsBandwidth:
		bandwidthRw.EnableCollectionInbound = operation;
		bandwidthRw.EnableCollectionOutbound = operation;
		rw = (PUCHAR)& bandwidthRw;
		size = sizeof(TCP_ESTATS_BANDWIDTH_RW_v0);
		break;

	case TcpConnectionEstatsFineRtt:
		fineRttRw.EnableCollection = enable;
		rw = (PUCHAR)& fineRttRw;
		size = sizeof(TCP_ESTATS_FINE_RTT_RW_v0);
		break;

	default:
		return;
		break;
	}

	status = SetPerTcpConnectionEStats((PMIB_TCPROW)row, type, rw, 0, size, 0);

	if (status != NO_ERROR) {
		wprintf(L"\nSetPerTcpConnectionEStats %s %s failed. status = %d",
				estatsTypeNames[type], enable ? L"enabled" : L"disabled",
				status);
	}
}

void
SocketClient::TcpStatistics(void* row)
{
	if (m_nofSockets > 0 && m_sockSuccess == true)
	{						
		std::stringstream result;
		result << m_tsPointer->GetMillisecondsTs(0) << "\t" << GetAndOutputEstats(row, TcpConnectionEstatsSynOpts) << "\t" << GetAndOutputEstats(row, TcpConnectionEstatsData)
			<< "\t" << GetAndOutputEstats(row, TcpConnectionEstatsSndCong) << "\t" << GetAndOutputEstats(row, TcpConnectionEstatsPath)
			<< "\t" << GetAndOutputEstats(row, TcpConnectionEstatsSendBuff) << "\t" << GetAndOutputEstats(row, TcpConnectionEstatsRec)
			<< "\t" << GetAndOutputEstats(row, TcpConnectionEstatsObsRec) << "\t" << GetAndOutputEstats(row, TcpConnectionEstatsBandwidth)
			<< "\t" << GetAndOutputEstats(row, TcpConnectionEstatsFineRtt);
		m_logOutput->RedOutput(result.str());
	}
}

//
// Dump the supplied Estate type on the given TCP connection row.
//
std::string
SocketClient::GetAndOutputEstats(void* row, TCP_ESTATS_TYPE type)
{
	std::string output;
	ULONG rosSize = 0, rodSize = 0;
	ULONG winStatus;
	PUCHAR ros = NULL, rod = NULL;

	PTCP_ESTATS_SYN_OPTS_ROS_v0 synOptsRos = { 0 };
	PTCP_ESTATS_DATA_ROD_v0 dataRod = { 0 };
	PTCP_ESTATS_SND_CONG_ROD_v0 sndCongRod = { 0 };
	PTCP_ESTATS_SND_CONG_ROS_v0 sndCongRos = { 0 };
	PTCP_ESTATS_PATH_ROD_v0 pathRod = { 0 };
	PTCP_ESTATS_SEND_BUFF_ROD_v0 sndBuffRod = { 0 };
	PTCP_ESTATS_REC_ROD_v0 recRod = { 0 };
	PTCP_ESTATS_OBS_REC_ROD_v0 obsRecRod = { 0 };
	PTCP_ESTATS_BANDWIDTH_ROD_v0 bandwidthRod = { 0 };
	PTCP_ESTATS_FINE_RTT_ROD_v0 fineRttRod = { 0 };

	switch (type) {
	case TcpConnectionEstatsSynOpts:
		rosSize = sizeof(TCP_ESTATS_SYN_OPTS_ROS_v0);
		break;

	case TcpConnectionEstatsData:
		rodSize = sizeof(TCP_ESTATS_DATA_ROD_v0);
		break;

	case TcpConnectionEstatsSndCong:
		rodSize = sizeof(TCP_ESTATS_SND_CONG_ROD_v0);
		rosSize = sizeof(TCP_ESTATS_SND_CONG_ROS_v0);
		break;

	case TcpConnectionEstatsPath:
		rodSize = sizeof(TCP_ESTATS_PATH_ROD_v0);
		break;

	case TcpConnectionEstatsSendBuff:
		rodSize = sizeof(TCP_ESTATS_SEND_BUFF_ROD_v0);
		break;

	case TcpConnectionEstatsRec:
		rodSize = sizeof(TCP_ESTATS_REC_ROD_v0);
		break;

	case TcpConnectionEstatsObsRec:
		rodSize = sizeof(TCP_ESTATS_OBS_REC_ROD_v0);
		break;

	case TcpConnectionEstatsBandwidth:
		rodSize = sizeof(TCP_ESTATS_BANDWIDTH_ROD_v0);
		break;

	case TcpConnectionEstatsFineRtt:
		rodSize = sizeof(TCP_ESTATS_FINE_RTT_ROD_v0);
		break;

	default:
		wprintf(L"\nCannot get type %d", (int)type);
		output = "CannotGetType_" + (int) type;
		return output;
		break;
	}

	if (rosSize != 0) {
		ros = (PUCHAR)malloc(rosSize);
		if (ros == NULL) {
			wprintf(L"\nOut of memory");
			output = "OutOfMemory";
			return output;
		}
		else
			memset(ros, 0, rosSize); // zero the buffer
	}
	if (rodSize != 0) {
		rod = (PUCHAR)malloc(rodSize);
		if (rod == NULL) {
			free(ros);
			wprintf(L"\nOut of memory");
			output = "OutOfMemoryRod";
			return output;
		}
		else
			memset(rod, 0, rodSize); // zero the buffer
	}

	winStatus = GetPerTcpConnectionEStats((PMIB_TCPROW)row,	type, NULL, 0, 0, ros, 0, rosSize, rod, 0, rodSize);

	/*char* data = static_cast<char*>(row);
	size_t len = *static_cast<int*>(row);
	std::string sName(data, len);

	std::string sName2(reinterpret_cast<char*>(&ros));
	std::string sName3(reinterpret_cast<char*>(&rod));*/ //for printing only...

	if (winStatus != NO_ERROR) {
		wprintf(L"\nGetPerTcpConnectionEStats %s failed. status = %d", estatsTypeNames[type], winStatus);
		output = "FailedGetTcpConn";
	}
	else {
		switch (type) {
		case TcpConnectionEstatsSynOpts:
			synOptsRos = (PTCP_ESTATS_SYN_OPTS_ROS_v0)ros;
			wprintf(L"\nSyn Opts");
			output = std::to_string(synOptsRos->ActiveOpen ? 1 : 0) + '\t' + std::to_string(synOptsRos->MssRcvd) + '\t' + std::to_string(synOptsRos->MssSent);
			break;

		case TcpConnectionEstatsData:
			dataRod = (PTCP_ESTATS_DATA_ROD_v0)rod;
			wprintf(L"\n\nData");
			output = std::to_string(dataRod->DataBytesOut) + '\t' + std::to_string(dataRod->DataSegsOut) + '\t' + std::to_string(dataRod->DataBytesIn) + '\t' + std::to_string(dataRod->DataSegsIn) + '\t' + std::to_string(dataRod->SegsOut) + '\t' + std::to_string(dataRod->SegsIn)
				+ '\t' + std::to_string(dataRod->SoftErrors) + '\t' + std::to_string(dataRod->SoftErrorReason) + '\t' + std::to_string(dataRod->SndUna) + '\t' + std::to_string(dataRod->SndNxt) + '\t' + std::to_string(dataRod->SndMax) + '\t' + std::to_string(dataRod->ThruBytesAcked)
				+ '\t' + std::to_string(dataRod->RcvNxt) + '\t' + std::to_string(dataRod->ThruBytesReceived);
			break;

		case TcpConnectionEstatsSndCong:
			sndCongRod = (PTCP_ESTATS_SND_CONG_ROD_v0)rod;
			sndCongRos = (PTCP_ESTATS_SND_CONG_ROS_v0)ros;
			wprintf(L"\n\nSnd Cong");
			output = std::to_string(sndCongRod->SndLimTransRwin) + '\t' + std::to_string(sndCongRod->SndLimTimeRwin) + '\t' + std::to_string(sndCongRod->SndLimBytesRwin) + '\t' + std::to_string(sndCongRod->SndLimTransCwnd) + '\t' + std::to_string(sndCongRod->SndLimTimeCwnd)
				+ '\t' + std::to_string(sndCongRod->SndLimBytesCwnd) + '\t' + std::to_string(sndCongRod->SndLimTransSnd) + '\t' + std::to_string(sndCongRod->SndLimTimeSnd) + '\t' + std::to_string(sndCongRod->SndLimBytesSnd) + '\t' + std::to_string(sndCongRod->SlowStart)
				+ '\t' + std::to_string(sndCongRod->CongAvoid) + '\t' + std::to_string(sndCongRod->OtherReductions) + '\t' + std::to_string(sndCongRod->CurCwnd) + '\t' + std::to_string(sndCongRod->MaxSsCwnd) + '\t' + std::to_string(sndCongRod->MaxCaCwnd)
				+ '\t' + std::to_string(sndCongRod->CurSsthresh) + '\t' + std::to_string(sndCongRod->MaxSsthresh) + '\t' + std::to_string(sndCongRod->MinSsthresh) + '\t' + std::to_string(sndCongRos->LimCwnd);
			break;

		case TcpConnectionEstatsPath:
			pathRod = (PTCP_ESTATS_PATH_ROD_v0)rod;
			wprintf(L"\n\nPath");
			output = std::to_string(pathRod->FastRetran) + '\t' + std::to_string(pathRod->Timeouts) + '\t' + std::to_string(pathRod->SubsequentTimeouts) + '\t' + std::to_string(pathRod->CurTimeoutCount) + '\t' + std::to_string(pathRod->AbruptTimeouts)
				+ '\t' + std::to_string(pathRod->PktsRetrans) + '\t' + std::to_string(pathRod->BytesRetrans) + '\t' + std::to_string(pathRod->DupAcksIn) + '\t' + std::to_string(pathRod->SacksRcvd) + '\t' + std::to_string(pathRod->SackBlocksRcvd)
				+ '\t' + std::to_string(pathRod->CongSignals) + '\t' + std::to_string(pathRod->PreCongSumCwnd) + '\t' + std::to_string(pathRod->PreCongSumRtt) + '\t' + std::to_string(pathRod->PostCongSumRtt) + '\t' + std::to_string(pathRod->PostCongCountRtt)
				+ '\t' + std::to_string(pathRod->EcnSignals) + '\t' + std::to_string(pathRod->EceRcvd) + '\t' + std::to_string(pathRod->SendStall) + '\t' + std::to_string(pathRod->QuenchRcvd) + '\t' + std::to_string(pathRod->RetranThresh)
				+ '\t' + std::to_string(pathRod->SndDupAckEpisodes) + '\t' + std::to_string(pathRod->SumBytesReordered) + '\t' + std::to_string(pathRod->NonRecovDa) + '\t' + std::to_string(pathRod->NonRecovDaEpisodes)
				+ '\t' + std::to_string(pathRod->AckAfterFr) + '\t' + std::to_string(pathRod->DsackDups) + '\t' + std::to_string(pathRod->SampleRtt) + '\t' + std::to_string(pathRod->SmoothedRtt) + '\t' + std::to_string(pathRod->RttVar)
				+ '\t' + std::to_string(pathRod->MaxRtt) + '\t' + std::to_string(pathRod->MinRtt) + '\t' + std::to_string(pathRod->SumRtt) + '\t' + std::to_string(pathRod->CountRtt) + '\t' + std::to_string(pathRod->CurRto) + '\t' + std::to_string(pathRod->MaxRto)
				+ '\t' + std::to_string(pathRod->MinRto) + '\t' + std::to_string(pathRod->CurMss) + '\t' + std::to_string(pathRod->MaxMss) + '\t' + std::to_string(pathRod->MinMss) + '\t' + std::to_string(pathRod->SpuriousRtoDetections);
			break;

		case TcpConnectionEstatsSendBuff:
			sndBuffRod = (PTCP_ESTATS_SEND_BUFF_ROD_v0)rod;
			wprintf(L"\n\nSend Buff");
			output = std::to_string(sndBuffRod->CurRetxQueue) + '\t' + std::to_string(sndBuffRod->MaxRetxQueue) + '\t' + std::to_string(sndBuffRod->CurAppWQueue) + '\t' + std::to_string(sndBuffRod->MaxAppWQueue);
			break;

		case TcpConnectionEstatsRec:
			recRod = (PTCP_ESTATS_REC_ROD_v0)rod;
			wprintf(L"\n\nRec");
			output = std::to_string(recRod->CurRwinSent) + '\t' + std::to_string(recRod->MaxRwinSent) + '\t' + std::to_string(recRod->MinRwinSent) + '\t' + std::to_string(recRod->LimRwin) + '\t' + std::to_string(recRod->DupAckEpisodes) + '\t' + std::to_string(recRod->DupAcksOut)
				+ '\t' + std::to_string(recRod->CeRcvd) + '\t' + std::to_string(recRod->EcnSent) + '\t' + std::to_string(recRod->EcnNoncesRcvd) + '\t' + std::to_string(recRod->CurReasmQueue) + '\t' + std::to_string(recRod->MaxReasmQueue) + '\t' + std::to_string(recRod->CurAppRQueue)
				+ '\t' + std::to_string(recRod->MaxAppRQueue) + '\t' + std::to_string(recRod->WinScaleSent);
			break;

		case TcpConnectionEstatsObsRec:
			obsRecRod = (PTCP_ESTATS_OBS_REC_ROD_v0)rod;
			wprintf(L"\n\nObs Rec");
			output = std::to_string(obsRecRod->CurRwinRcvd) + '\t' + std::to_string(obsRecRod->MaxRwinRcvd) + '\t' + std::to_string(obsRecRod->MinRwinRcvd) + '\t' + std::to_string(obsRecRod->WinScaleRcvd);
			break;

		case TcpConnectionEstatsBandwidth:
			bandwidthRod = (PTCP_ESTATS_BANDWIDTH_ROD_v0)rod;
			wprintf(L"\n\nBandwidth");
			output = std::to_string(bandwidthRod->OutboundBandwidth) + '\t' + std::to_string(bandwidthRod->InboundBandwidth) + '\t' + std::to_string(bandwidthRod->OutboundInstability) + '\t' + std::to_string(bandwidthRod->InboundInstability)
				+ '\t' + std::to_string(bandwidthRod->OutboundBandwidthPeaked) + '\t' + std::to_string(bandwidthRod->InboundBandwidthPeaked);
			break;

		case TcpConnectionEstatsFineRtt:
			fineRttRod = (PTCP_ESTATS_FINE_RTT_ROD_v0)rod;
			wprintf(L"\n\nFine RTT");
			output = std::to_string(fineRttRod->RttVar) + '\t' + std::to_string(fineRttRod->MaxRtt) + '\t' + std::to_string(fineRttRod->MinRtt) + '\t' + std::to_string(fineRttRod->SumRtt);
			break;

		default:
			wprintf(L"\nCannot get type %d", type);
			break;
		}
	}

	free(ros);
	free(rod);

	return output;
}

bool
SocketClient::GetSocketCreationFlag()
{
	return m_sockSuccess;
}
