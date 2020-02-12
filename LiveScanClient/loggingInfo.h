/**
 *  This class is developed to serve for redirecting output to a file
 *  and keep statistics regarding the client's operations.
 *
 *  Ioannis Selinis 2019 (5GIC, University of Surrey)
 */


#ifndef LOGGINGINFO_H
#define LOGGINGINFO_H

#include <fstream>
#include <iostream>
#include <string>

class LoggingInfo
{
public:
	LoggingInfo();
	~LoggingInfo();

	void RedOutput(std::string msg);
	void SetPath(std::string path);

private:
	bool flag; 		//!< Flag for setting the path to the output file
	std::string filePath;	//!< The path to the file
	std::fstream outputFile;
};

#endif

