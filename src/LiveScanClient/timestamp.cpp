/**
 *  This class is developed to get the local time or UTC time.
 *  It can return the time in seconds or milliseconds.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#include "timestamp.h"
#include <time.h>       /* time_t, struct tm, time, localtime, strftime */
#include <sstream>
#include <Windows.h>
#include <iomanip>

#include <filesystem>

Timestamp::Timestamp()
{
}

Timestamp::~Timestamp()
{
}

void
Timestamp::gettimeofday(struct timeval* tp)
{
	// Note: some broken versions only have 8 trailing zero's, the correct epoch has 9 trailing zero's
	// This magic number is the number of 100 nanosecond intervals since January 1, 1601 (UTC)
	// until 00:00:00 January 1, 1970 
	static const uint64_t EPOCH = ((uint64_t)116444736000000000ULL);

	SYSTEMTIME  system_time;
	FILETIME    file_time;
	uint64_t    time;

	GetSystemTime(&system_time);
	SystemTimeToFileTime(&system_time, &file_time);
	time = ((uint64_t)file_time.dwLowDateTime);
	time += ((uint64_t)file_time.dwHighDateTime) << 32;

	tp->tv_sec = (long)((time - EPOCH) / 10000000L);
	tp->tv_usec = (long)(system_time.wMilliseconds * 1000);
}

uint32_t
Timestamp::GetMillisecondsTs(int offset)
{
	struct timeval unix;
	gettimeofday(&unix);

	unix.tv_sec += offset / 1000;
	unix.tv_usec += (offset % 1000) * 1000;

	if (unix.tv_usec < 0) {		
		int _redFactor = unix.tv_usec / 1000000;
		unix.tv_sec = unix.tv_sec + (_redFactor - 1);
		unix.tv_usec += 1000000;
	}	
	if (unix.tv_usec >= 1000000) {
		int _incFactor = unix.tv_usec / 1000000;
		unix.tv_sec += _incFactor;
		unix.tv_usec -= 1000000;
	}

	int hour = (unix.tv_sec % 86400L) / 3600;
	int minute = (unix.tv_sec % 3600) / 60;
	int second = (unix.tv_sec % 60);
	int millisecond = unix.tv_usec / 1000;

	return hour * 10000000 + minute * 100000 + second * 1000 + millisecond;
}

std::string
Timestamp::GetSecondsTs()
{
	time_t rawtime;
	struct tm* timeinfo;
	char buffer[80];

	time(&rawtime);
	timeinfo = localtime(&rawtime);

	strftime(buffer, 80, "%H%M%S", timeinfo);
	std::string string_sec(buffer);

	return string_sec;
}

std::string
Timestamp::GetSecondsString()
{
	time_t rawTime = time(NULL);
	tm* tm_local = localtime(&rawTime);
	std::ostringstream ss;
	ss << tm_local->tm_hour << "." << tm_local->tm_min << "." << tm_local->tm_sec;

	return ss.str();
}

long
Timestamp::GetDateTs()
{
	time_t rawTime = time(NULL);
	tm* tm_local = localtime(&rawTime);
	std::ostringstream ss;
	ss << tm_local->tm_year + 1900 << tm_local->tm_mon + 1 << tm_local->tm_mday;
	std::istringstream iss(ss.str());
	long dateTs;
	iss >> dateTs;

	return dateTs;
}
