/**
 *  This class is developed for non blocking sockets.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#pragma once

#ifndef SOCKET_H
#define SOCKET_H


#include <WinSock2.h>
#include <Ws2tcpip.h>
#include <iphlpapi.h>
#include <Tcpestats.h>
#include <stdlib.h>
#include <string>
#include "timestamp.h"
#include "LoggingInfo.h"

class SocketClient {
public:
	/**
	 * This function is called by the LRESULT CALLBACK LiveScanClient::DlgProc
	 * when the connect button is pressed  (m_pClientSocket = new SocketClient(address, 48001);)
	 * parameters:
	 *		\host: the address to connect to
	 *		\port: the port to connect to
	 *
	 * The SocketClient () function is responsible for the following:
	 *
	 * Starting up the Winsock API (WSAStartup(WORD,LPWSADATA))
     *
     *  WORD MAKEWORD(
     *	BYTE bLow,
     *	BYTE bHigh
     *  );
     * It's defined in Windef.h as :
     *        #define MAKEWORD(a,b)   ((WORD)(((BYTE)(a))|(((WORD)((BYTE)(b)))<<8)))
     * It basically builds a 16 bits words from two 1 bytes word (and doesn't look very portable)
     * The binary representation of the number 2 with 1 byte (a WORD) is : | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 |
     * If we take the concatenate two of those bytes as in MAKEWORD(2,2) , we get:
     * | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 |
     * Which is 512 + 2 = 514 : live demo.
     * The only real life example of this particular macro is in the Initialization of Winsock, 
     * to generate the versioning word expected by WSAStartup.

	 * to initilise a socket by calling the s_ = socket(_addrFamily, _socketType, _protocolType);
     * -----------------------------------------------------------------------------------------------------------
     * _addrFamily:
     * AF_INET:       For interprocess communications between processes on the same system or different systems in the
     *                Internet domain using the Internet Protocol (IPv4).
     * AF_INET6:	  For interprocess communications between processes on the same system or different systems in the
     *                Internet domain using the Internet Protocol (IPv6 or IPv4).
     * AF_NS:   	  For interprocess communications between processes on the same system or different systems in the
     *                domain defined by the Novell or Xerox protocol definitions.
     *                Note: The AF_NS address family is no longer supported as of V5R2.
     * AF_UNIX:	      For interprocess communications between processes on the same system in the UNIX® domain.
     * AF_UNIX_CCSID: For interprocess communications between processes on the same system in the UNIX domain using
     *                the Qlg_Path_Name_T structure.
     * AF_TELEPHONY:  For interprocess communications between processes on the same system in the telephony domain.
     *                Note: The AF_TELEPHONY address family is no longer supported as of V5R3.
     *
     * _socketType:
     * SOCK_DGRAM:     Indicates a datagram socket is desired.
     * SOCK_SEQPACKET: Indicates a full-duplex sequenced packet socket is desired. Each input and output operation
     *                 consists of exactly one record.
     * SOCK_STREAM:  	 Indicates a full-duplex stream socket is desired.
     * SOCK_RAW:   	 Indicates communication is directly to the network protocols. A process must have the appropriate
     *                 privilege *ALLOBJ to use this type of socket. Used by users who want to access the lower-level
     *                 protocols directly.
     *
     * _protocolType:
     * 0:	              Indicates that the default protocol for the type selected is to be used. For example, IPPROTO_TCP is chosen for
     *                  the protocol if the type was set to SOCK_STREAM and the address family is AF_INET.
     * IPPROTO_IP:      Equivalent to specifying the value zero (0).
     * IPPROTO_TCP:     Indicates that the TCP protocol is to be used.
     * IPPROTO_UDP:     Indicates that the UDP protocol is to be used.
     * IPPROTO_RAW:     Indicates that communications is to the IP layer.
     * IPPROTO_ICMP:    Indicates that the Internet Control Message Protocol (ICMP) is to be used.
     * IPPROTO_ICMPV6:  Indicates that the Internet Control Message Protocol (ICMPv6) is to be used.
     * TELPROTO_TEL: 	  Equivalent to specifying the value zero (0).
     *------------------------------------------------------------------------------------------------------
     * For the error conditions see https://www.ibm.com/support/knowledgecenter/en/ssw_ibm_i_74/apis/socket.htm
	 *
	 * The gethostbyname function (which lies inside this SocketClient) returns a pointer to a hostent structure
	 * —a structure allocated by Windows Sockets. The hostent structure contains the results of a successful search 
	 * for the host specified in the name parameter.
	 * Note: The gethostbyname function has been deprecated by the introduction of the getaddrinfo function. 
	 * Developers creating Windows Sockets 2 applications are urged to use the getaddrinfo function instead of gethostbyname.
	 *
	 * The htons function (which lies inside the SocketClient) is for Host to network short (whilst htonl is host to network long)
	 * u_short htons (u_short). The "host" computer is the computer that listens for and invites connections to it, and the "network" 
	 * computer is the visitor that connects to the host. So, for example, before we specify which port we are going to listen on 
	 * or connect to, we'll have to use the htons() function to convert the number to network byte order. Note that after using inet_addr() 
	 * to convert a string IP address to the required form, we will be returned the address in the correct network order, eliminating the 
	 * need to evoke htonl(). An easy way to differentiate between htons() and htonl() is to think of the port number as the shorter number, 
	 * and the IP as the longer number (which is true – an IP address consists of four sets of up to three digits separated by periods, versus 
	 * a single port number).
	 *
	 * The connect() function (which lies inside this SocketClient) is responsible for connecting to a Remote Host (Acting as the Client).
	 * The structure of connect() is:
	 * struct sockaddr_in 
	 * {
     *   short      sin_family;
     *   u_short    sin_port;
     *   struct     in_addr sin_addr;
     *   char       sin_zero[8]; // uneeded parameter, hence left blank
     * };
     * int PASCAL connect(SOCKET,const struct sockaddr*,int);
	 */
  SocketClient(const std::string& host, int port, int clientId);
  /**
   * CLOSECONNECTION – shuts down the socket and closes any connection on it
   * closing down the Winsock API (WSACleanup(void))
   */
  void CloseConnection();
  /**
   * Returns the socket
   */
  SOCKET GetSocket();
  /**
   * The parameter of SendBytes is a const reference
   * because SendBytes does not modify the std::string passed 
   * (in contrast to SendLine).
   * The void Socket::SendBytes(const char *buf, int len) function is called by the LiveScanClient::HandleSocket() and it calls in turn the send () of the WinSock2.h
   * which is defined as follows:
   * int send(
   *  SOCKET s,
   *  const char FAR * buf,
   *  int len,
   *  int flags
   * );
   *
   * The SOCKET parameter is the connected socket to send the data on.
   * The second parameter, buf, is a pointer to the character buffer that contains the data to be sent.
   * The third parameter, len, specifies the number of characters in the buffer to send.
   * Finally, the flags parameter can be either 0, MSG_DONTROUTE, or MSG_OOB. The 0 flag allows you to use a regular recv(), with a standard behavior.
   * Alternatively, the flags parameter can be a bitwise OR any of those flags. The MSG_DONTROUTE flag tells the transport not to route the packets it sends.
   * It is up to the underlying transport to honor this request (for example, if the transport protocol doesn't support this option, it will be ignored).
   * The MSG_OOB flag signifies that the data should be sent out of band.
   * For example, if you want to have the 'MSG_DONTWAIT' and the 'MSG_MORE' behaviour (as described by the man page) you can use MSG_DONTWAIT | MSG_MORE.
   * Another example, if you want to use a custom recv(), you need to separate your flags (thoses who're listed in the man page) with the OR operator
   * like that recv(sockfd, buf, buflen, FLAG | FLAG | FLAG);
   *
   * On a good return, send returns the number of bytes sent; otherwise, if an error occurs, SOCKET_ERROR will be returned.
   * A common error is WSAECO-NNABORTED, which occurs when the virtual circuit terminates because of a timeout failure or a protocol error.
   * When this occurs, the socket should be closed, as it is no longer usable. The error WSAECONNRESET occurs when the application on the
   * remote host resets the virtual circuit by executing a hard close or terminating unexpectedly, or when the remote host is rebooted.
   * Again, the socket should be closed after this error occurs. The last common error is WSAETIMEDOUT, which occurs when the connection is
   * dropped because of a network failure or the remote connected system going down without notice.
   */
  void SendBytes(const char* buf, int len);
  /**
 * This function is called by the liveScanClient::HandleSocket()
 *
 * int PASCAL FAR ioctlsocket ( SOCKET s, long cmd, u_long FAR * argp);
 *
 * s:    A descriptor identifying a socket.
 * cmd:  The command to perform on the socket s.
 * argp: A pointer to a parameter for cmd.
 *
 * Upon successful completion, the ioctlsocket returns zero. Otherwise, a value of SOCKET_ERROR is returned
 *
 * Remarks -------
 * This routine may be used on any socket in any state. It is used to get or retrieve operating parameters associated
 * with the socket, independent of the protocol and communications subsystem. The following commands are supported:
 * FIONBIO
 *    Enable or disable non-blocking mode on the socket s. argp points at an unsigned long, which is non-zero if non-blocking mode
 *	is to be enabled and zero if it is to be disabled. When a socket is created, it operates in blocking mode (i.e. non-blocking
 *    mode is disabled). This is consistent with BSD sockets. The WSAAsyncSelect() routine automatically sets a socket to nonblocking mode.
 *    If WSAAsyncSelect() has been issued on a socket, then any attempt to use ioctlsocket() to set the socket back to blocking mode will
 *    fail with WSAEINVAL. To set the socket back to blocking mode, an application must first disable WSAAsyncSelect() by calling WSAAsyncSelect()
 *    with the lEvent parameter equal to 0.
 * FIONREAD
 *    Determine the amount of data which can be read atomically from socket s. argp points at an unsigned long in which ioctlsocket() stores
 *    the result. If s is of type SOCK_STREAM, FIONREAD returns the total amount of data which may be read in a single recv(); this is normally
 *    the same as the total amount of data queued on the socket. If s is of type SOCK_DGRAM, FIONREAD returns the size of the first datagram queued
 *    on the socket. FIONREAD returns (without blocking) how many how many bytes is available to be read, although not quite accurate (but we don't
 *    care about that, because we are using it to know if there is data to be read and not to know exactly how many bytes are there).
 * SIOCATMARK
 *    Determine whether or not all out-of-band data has been read. This applies only to a socket of type SOCK_STREAM which has been configured for
 *    in-line reception of any out-of-band data (SO_OOBINLINE). If no out-of-band data is waiting to be read, the operation returns TRUE. Otherwise
 *    it returns FALSE, and the next recv() or recvfrom() performed on the socket will retrieve some or all of the data preceding the "mark"; the
 *    application should use the SIOCATMARK operation to determine whether any remains. If there is any normal data preceding the "urgent" (out of band)
 *    data, it will be received in order. (Note that a recv() or recvfrom() will never mix out-of-band and normal data in the same call.) argp points at
 *    a BOOL in which ioctlsocket() stores the result.
 */
  std::string ReceiveBytes();
  bool GetSocketCreationFlag();
  void TcpStatistics(void* row);
  void SetUpStatistics();

private:
	  DWORD GetTcpRow(u_short localPort, u_short remotePort, MIB_TCP_STATE state, __out PMIB_TCPROW row);
	  void ToggleAllEstats(void* row, bool enable);
	  void ToggleEstat(PVOID row, TCP_ESTATS_TYPE type, bool enable);	  
	  std::string GetAndOutputEstats(void* row, TCP_ESTATS_TYPE type);
	  int CreateTcpConnection(const std::string& host, u_short serverPort, u_short* clientPort);
	  void CloseUpStatistics();

	  int  m_nofSockets; //number of sockets created
	  SOCKET m_sock; //the initialised socket for sending
	  int m_clientId; //the client id of the socket (this is needed for the statistics 
	  int m_lPort; //the local port of the socket
	  int m_hPort; //the host port that this socket is connected to
	  Timestamp* m_tsPointer;
	  LoggingInfo* m_logOutput;
	  bool m_sockSuccess;
};
#endif