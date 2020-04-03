/**
 *  This class is developed to act as a buffer and store the frames
 *  produced by the sensors, before transmitting them to the Server. 
 *  Each client connects to a single server, hence we mark the frames
 *  produced and transmitted by timestamps, in order to indicate the 
 *  frames already transmitted and to avoid any duplicate transmissions. 
 *  For the time being, one thread is applied for en/de-queuing.
 *
 *  As a future step, I could make use of https://juanchopanzacpp.wordpress.com/2013/02/26/concurrent-queue-c11/
 *  to signal when a frame is in the queue and start dequeueing and transmitting the frame somehow...
 *  
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

// Although, a 3D array might be a better solutions, I used 3 vectors as the size of the buffer varies.
//TODO: queueing policy (if buffer full then choose if we want to drop the oldest or newest frame)

#include "stdafx.h"
#include "clientBuffer.h"
#include <chrono>
#include <vector>
#include <exception>      
#include <iostream>
#include <algorithm>    // for copy() and assign() 
#include <filesystem>
#include <string>       // std::string
#include <sstream>      // std::stringstream

std::mutex mtx;           // mutex for critical section

ClientBuffer::ClientBuffer():
	m_lastTsBuffered(0),
	m_lastTsBufferedDeq(0),
	m_flagOutput(true)	
{
	m_tsPointer = new Timestamp();
	m_logOutput = new LoggingInfo();

	m_stringRedirect.clear();

	std::stringstream ss;

	std::filesystem::path pathV1 = std::filesystem::current_path();
	std::string pth_str = pathV1.string() + "\\logging_output";
	std::filesystem::path pathV2 = (pth_str);

	long dateTs = m_tsPointer->GetDateTs();
	std::string secTs = m_tsPointer->GetSecondsTs();

	if (std::filesystem::exists(pathV2))
	{
		ss << pth_str << "\\OutputFileClientBuffer_" << dateTs << "_" << secTs << ".txt";
		m_logOutput->SetPath(ss.str());
	}
	else
	{
		std::filesystem::create_directory(pathV2);
		ss << pth_str << "\\OutputFileClientBuffer_" << dateTs << "_" << secTs << ".txt";
		m_logOutput->SetPath(ss.str());
		m_logOutput->RedOutput("Directory and File created now");
	}
	std::stringstream ssb;
	ssb << "Size\tBytesSoFar\tDeltaCreationEnqueue\tDeltaEnqueue\tDeltaStayedQueue\tDeltaDequeue" ;
	m_logOutput->RedOutput(ssb.str());

	m_thread = std::thread(&ClientBuffer::bufferStatistics, this);
}

ClientBuffer::~ClientBuffer()
{
	m_thread.join();
}

int 
ClientBuffer::deltaCorrection(int delta)
{
	if (delta > 4000000)
		delta -= 4040000; //e.g. 190000100 - 185959800 should give 300ms and not 4040300
	else if (delta > 30000)
		delta -= 40000; // e.g. 184000100 - 183959800 should give 300ms and not 40300

	return delta;
}

void
ClientBuffer::Enqueue(std::vector<char> finalVec, int tsCreation, int clockOffset)
{
	try {
		uint32_t ts = m_tsPointer->GetMillisecondsTs(clockOffset);
		if ((m_lBufferedFramesReadyForTx.size() < (MAX_CLIENT_BUFFER - (UINT)1)) && (ts > m_lastTsBuffered)) // just to make sure that (since fps is 30-60) we are not storing the same frame twice
		{
			uint32_t deltaEnq = 0;
			if (m_lastTsBuffered > 0)
				deltaEnq = deltaCorrection(ts - m_lastTsBuffered);

			m_lastTsBuffered = ts;

			// Enqueue packets in the buffer
			QueueObject queueObject;
			queueObject.m_enqueuedObjectV = finalVec;
			queueObject.m_tsEnqueue = ts;
			queueObject.m_bytesEnqueue = finalVec.size();
			queueObject.m_deltaCreation = deltaCorrection(ts - tsCreation);
			queueObject.m_deltaEnqueue = deltaEnq;

			m_lBufferedFramesReadyForTx.push(queueObject);
		}
	}
	catch (std::exception& e)
	{
		throw std::runtime_error(std::string("Enqueue client-buffer error: ") + e.what());
	}
}

int
ClientBuffer::Dequeue(std::vector<std::vector<char> >& outFrames, int clockOffset)
{
	try {
		uint32_t ts = m_tsPointer->GetMillisecondsTs(clockOffset);
		int size = 0;
		
		if (!m_lBufferedFramesReadyForTx.empty())
		{
			mtx.lock();
			size = m_lBufferedFramesReadyForTx.size();
			uint32_t deltaDeq = 0;
			if (m_lastTsBufferedDeq > 0)
				deltaDeq = deltaCorrection(ts - m_lastTsBufferedDeq);

			m_lastTsBufferedDeq = ts;
			uint32_t totalBytes = 0;
			while(!m_lBufferedFramesReadyForTx.empty())
			{
				QueueObject queueObject;
				queueObject = m_lBufferedFramesReadyForTx.front();
				m_lBufferedFramesReadyForTx.pop();

				outFrames.push_back(queueObject.m_enqueuedObjectV);
				uint32_t deltaStayedQueue = deltaCorrection(ts - queueObject.m_tsEnqueue);
				uint32_t deltaCreationEnq = queueObject.m_deltaCreation;
				uint32_t deltaEnq = queueObject.m_deltaEnqueue;
				totalBytes += queueObject.m_bytesEnqueue;

				m_stringRedirect += std::to_string(size) + "\t" + std::to_string(totalBytes) + "\t" + std::to_string(deltaCreationEnq) + "\t" + std::to_string(deltaEnq) + "\t" + std::to_string(deltaStayedQueue) + "\t" + std::to_string(deltaDeq) + "\n";
			}
			mtx.unlock();
		}
		return size;
	}
	catch (std::exception& e)
	{
		throw std::runtime_error(std::string("Dequeue client-buffer error: ") + e.what());
	} 
}

void
ClientBuffer::bufferStatistics()
{
	int interval = 1000; //[ms] consider interval of 1 sec for now...

	while (true)
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(interval));
		if (!m_stringRedirect.empty())
		{
			mtx.lock();
			m_logOutput->RedOutput(m_stringRedirect);
			m_stringRedirect.clear();
			mtx.unlock();
		}
	}
}

void
ClientBuffer::Clear()
{
	std::queue<QueueObject> empty;
	std::swap(m_lBufferedFramesReadyForTx, empty);

	m_lastTsBuffered = 0;
	m_lastTsBufferedDeq = 0;
}

int
ClientBuffer::size()
{
	return m_lBufferedFramesReadyForTx.size();
}