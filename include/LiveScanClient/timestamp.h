/**
 *  This class is developed to get the local time or UTC time.
 *  It can return the time in seconds or milliseconds.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#ifndef TIMESTAMP_H
#define TIMESTAMP_H

#include <ctime>
#include <chrono>
#include <string>

class Timestamp
{
public:
	Timestamp();
	~Timestamp();

	void gettimeofday(struct timeval* tp);
	uint32_t GetMillisecondsTs(int offset);
	long GetDateTs();
	std::string GetSecondsTs();
	std::string GetSecondsString();
};

#endif