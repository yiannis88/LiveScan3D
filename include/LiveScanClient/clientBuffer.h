/**
 *  This class is developed to act as a buffer and store the frames
 *  produced by the sensors, before transmitting them to the Server.
 *  Each client connects to a single server, hence we mark the frames
 *  produced and transmitted by timestamps, in order to indicate the
 *  frames already transmitted and to avoid any duplicate transmissions.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#ifndef CLIENTBUFFER_H
#define CLIENTBUFFER_H

#include "utils.h"
#include "iCapture.h"
#include "timestamp.h"
#include "LoggingInfo.h"
#include "QueueObject.h"
#include <thread>
#include <mutex>          // std::mutex, std::lock_guard
#include <vector>
#include <queue> 

class ClientBuffer
{
public:
	ClientBuffer();
	~ClientBuffer();

	/**
	 * This function saves the frames into the buffer (if allowed)
	 */
	void Enqueue(std::vector<char> finalVec, int tsCreation, int clockOffset);
	/**
	 * This function returns the number of the frames to be transmitted
	 * TODO: We tried to change the TCP settings in Windows 10 and allow larger socket buffer, with no success!
	 *       So far is limited to 256 KB, which means if the client has many frames to send, the socket buffer will
	 *       not allow to transmit everything in one go --> hence, we kept it simple for now (as changes will be required
	 *       at the server also)
	 */
	int Dequeue(std::vector<std::vector<char> >& outFrames, int clockOffset);
	void Clear();
	int size();

private:
	/**
	 * This function returns the corrected delta time based on any discrepancies introduced due to clock offset
	 */
	int deltaCorrection(int delta);
	void bufferStatistics();

	const UINT MAX_CLIENT_BUFFER  = 256;

	std::queue<QueueObject> m_lBufferedFramesReadyForTx;

	Timestamp* m_tsPointer;
	long m_lastTsBuffered;
	long m_lastTsBufferedDeq;

	LoggingInfo* m_logOutput;
	bool m_flagOutput;
	std::string m_stringRedirect;
	std::thread m_thread;
};

#endif
