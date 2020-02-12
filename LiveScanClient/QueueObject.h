/**
 *  This class is developed to create an object for the frames 
 *  enqueued in the client queue. Along with the frame to be transmitted,
 *  other information is also stored for statistical reasons.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#ifndef QUEUEOBJECT_H
#define QUEUEOBJECT_H

#include <vector>

class QueueObject
{
public:
	QueueObject();
	~QueueObject();

	std::vector<char> m_enqueuedObjectV;
	int m_tsEnqueue;
	int m_bytesEnqueue;
	int m_deltaCreation;
	int m_deltaEnqueue;
};

#endif