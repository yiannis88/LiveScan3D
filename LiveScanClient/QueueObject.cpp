/**
 *  This class is developed to create an object for the frames
 *  enqueued in the client queue. Along with the frame to be transmitted,
 *  other information is also stored for statistical reasons.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#include "QueueObject.h"

QueueObject::QueueObject() :
	m_tsEnqueue(0),
	m_bytesEnqueue(0),
	m_deltaCreation(0),
	m_deltaEnqueue(0)
{
}

QueueObject::~QueueObject()
{
}