/**
 *  This class is developed to serve for redirecting output to a file
 *  and keep statistics regarding the client's operations.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */

#include "loggingInfo.h"
#include <string>
#include <mutex>

std::mutex mu;

LoggingInfo::LoggingInfo()
	: flag(false)
{
}

LoggingInfo::~LoggingInfo()
{
}

void
LoggingInfo::RedOutput(std::string msg)
{
	if (flag)
	{
		try {
			std::lock_guard<std::mutex> locker(mu);
			if (!outputFile.is_open())
			{
				outputFile.open(filePath, std::fstream::in | std::fstream::out | std::fstream::app);
				outputFile << msg << std::endl;
				outputFile.close();
			}
		}
		catch (std::exception& e)
		{
			throw std::runtime_error(std::string("LoggingInfo client error: ") + e.what());
		}
	}
}

void
LoggingInfo::SetPath(std::string path)
{
	if (!flag)
	{
		filePath = path;
		flag = true;
	}
}
