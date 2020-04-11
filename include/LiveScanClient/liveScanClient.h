//   Copyright (C) 2015  Marek Kowalski (M.Kowalski@ire.pw.edu.pl), Jacek Naruniec (J.Naruniec@ire.pw.edu.pl)
//   License: MIT Software License   See LICENSE.txt for the full license.

//   If you use this software in your research, then please use the following citation:

//    Kowalski, M.; Naruniec, J.; Daniluk, M.: "LiveScan3D: A Fast and Inexpensive 3D Data
//    Acquisition System for Multiple Kinect v2 Sensors". in 3D Vision (3DV), 2015 International Conference on, Lyon, France, 2015

//    @INPROCEEDINGS{Kowalski15,
//        author={Kowalski, M. and Naruniec, J. and Daniluk, M.},
//        booktitle={3D Vision (3DV), 2015 International Conference on},
//        title={LiveScan3D: A Fast and Inexpensive 3D Data Acquisition System for Multiple Kinect v2 Sensors},
//        year={2015},
//    }
//  The initial code has significantly modified in order to support multiple TCP connections, decoupled functions, buffers,
//  and other functionalities to correctly receive/tranmit the frames to the server.
//  Comments or modifications (major) are made by Ioannis Selinis (5GIC University of Surrey, 2019)

#pragma once

#include "resource.h"
#include "ImageRenderer.h"
#include "SocketCS.h"
#include "calibration.h"
#include "utils.h"
#include "KinectCapture.h"
#include "frameFileWriterReader.h"
#include "timestamp.h"
#include "clientBuffer.h"
#include "LoggingInfo.h"
#include <thread>
#include <mutex>
#include <chrono>

class LiveScanClient
{
public:
	LiveScanClient();
	~LiveScanClient();


	static LRESULT CALLBACK MessageRouter(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
	LRESULT CALLBACK        DlgProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
	int                     Run(HINSTANCE hInstance, int nCmdShow);

	static inline int const MaxTcp = 6;
private:
	Calibration calibration;

	bool m_bCalibrate;
	bool m_bFilter;
	bool m_bStreamOnlyBodies;

	ICapture* pCapture;

	int m_nFilterNeighbors;
	float m_fFilterThreshold;

	bool m_bCaptureFrame;
	bool m_bConnected;
	bool m_bConfirmCaptured;
	bool m_bConfirmCalibrated;
	bool m_bShowDepth;
	bool m_bFrameCompression;
	int m_iCompressionLevel;

	FrameFileWriterReader m_framesFileWriterReader;


	std::vector<SocketClient*> m_pClientSocket;
	std::vector<float> m_vBounds;
	int* m_portPool;
	int m_tcpConn;
	float m_randomLosses;

	std::vector<Point3s> m_vLastFrameVertices;
	std::vector<RGB> m_vLastFrameRGB;
	std::vector<Body> m_vLastFrameBody;

	HWND m_hWnd;
	INT64 m_nLastCounter;
	double m_fFreq;
	INT64 m_nNextStatusTime;
	DWORD m_nFramesSinceUpdate;

	Point3f* m_pCameraSpaceCoordinates;
	Point2f* m_pColorCoordinatesOfDepth;
	Point2f* m_pDepthCoordinatesOfColor;

	// Direct2D
	ImageRenderer* m_pDrawColor;
	ID2D1Factory* m_pD2DFactory;
	RGB* m_pDepthRGBX;

	void UpdateFrame();
	void ProcessColor(RGB* pBuffer, int nWidth, int nHeight);
	void ProcessDepth(const UINT16* pBuffer, int nHeight, int nWidth);

	bool SetStatusMessage(_In_z_ WCHAR* szMessage, DWORD nShowTimeMsec, bool bForce);
	void build_fd_sets(fd_set* read_fds, fd_set* write_fds);

	void HandleSocketReceived(string received, std::vector<int>& anythingToSend, int portId);
	void HandleSocketTransmit(std::vector<int> signalInfoForTx, fd_set write_fds);
	void SendFrame(vector<char> frameTx, int sockId);
	void CreateFramesReadyForTransmission(vector<Point3s> vertices, vector<RGB> RGB, vector<Body> body, vector<char>& finalVec, uint32_t& tsCreation);

	void SocketThreadFunction();
	void NtpThreadFunction();
	/**
	 * 15 bytes per point; [colour]: 3 bytes for R,G,B respectively, and [coordinates]: 12 bytes for x,y,z respectively.
	 * x,y,z, are floats --> 4 bytes per float 
	 * On top of that, there is some small "overhead" transmitted, which is for the bodies information and green skeleton.
	 * This information (for the bodies) occupies 880 bytes per body detected by the sensor and per frame.
	 * Of course, a header is also transmitted per frame, with a size of 13 bytes.
	 */
	void StoreFrame(Point3f* vertices, Point2f* mapping, RGB* color, vector<Body>& bodies, BYTE* bodyIndex);
	void ShowFPS();
	void ReadIPFromFile();
	float bytesToFloat(uchar b0, uchar b1, uchar b2, uchar b3);
	void WriteIPToFile();

	LoggingInfo* m_logOutputTimers;
	bool m_flagOutput;

	Timestamp* m_tsPointer;
	ClientBuffer* m_clBuffer;	

	bool m_bSocketThread;
	bool m_bNtpThread;
	int m_offsetUtcClock; // in ms

	LoggingInfo* m_logOutputFps;
	std::chrono::system_clock::time_point m_lastTs;
	std::chrono::system_clock::time_point m_lastTsFps;
	uint32_t m_pktCtr;
	uint32_t m_droppedFrame;
};

