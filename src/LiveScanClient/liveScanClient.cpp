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

#include "stdafx.h"
#include "resource.h"
#include "LiveScanClient.h"
#include "filter.h"
#include "NtpClient.h"
#include <strsafe.h>
#include <fstream>
#include "zstd.h"
#include <date.h>
// header file has been directly injected into codebase due to issues with vcpkg
#include <ctime>
#include <string>
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <iostream>
#include <filesystem>
#include <regex>
#include <array>
#include <algorithm>

/**
 * To use the date/date.h library (since it will be available with the c++ 20) we need to:
 *    1) Download https://github.com/Microsoft/vcpkg
 *    2) Administrator cmd (type cmd in the start menu, then right click run as administrator)
 *    3) bootstrap-vcpkg.bat in the root directory where the vcpkg exists (e.g. C:\Users\5gic\Downloads\vcpkg-master\vcpkg-master)
 *    4) vcpkg install boost:x86-windows (check with vcpkg list)
 *    5) vcpkg integrate install 
 *    6) vcpkg search date
 *    7) vcpkg install date 
 *    8) Now we need to link VS2019: 
 *            Open VS2019
 *            Go to Solution Explorer and then right click on the solution (e.g. LiveScanClient)
 *            Properties --> Linker --> System and change the Subsystem field to Console(/SUBSYSTEM:CONSOLE)
 * 
 *    If with the Console(/SUBSYSTEM:CONSOLE), an error like msvcrtd.lib error lnk2019 appears while rebuilding solution, 
 *    change the Console(/SUBSYSTEM:CONSOLE) to Windows(/SUBSYSTEM:WINDOWS)
 */

std::mutex m_mSocketThreadMutex;

int APIENTRY wWinMain(    
	_In_ HINSTANCE hInstance,
    _In_opt_ HINSTANCE hPrevInstance,
    _In_ LPWSTR lpCmdLine,
    _In_ int nShowCmd
    )
{
    UNREFERENCED_PARAMETER(hPrevInstance);
    UNREFERENCED_PARAMETER(lpCmdLine);

    LiveScanClient application;
    application.Run(hInstance, nShowCmd);
}

LiveScanClient::LiveScanClient() :
	m_hWnd(NULL),
	m_nLastCounter(0),
	m_nFramesSinceUpdate(0),
	m_fFreq(0),
	m_nNextStatusTime(0LL),
	m_pD2DFactory(NULL),
	m_pDrawColor(NULL),
	m_pDepthRGBX(NULL),
	m_pCameraSpaceCoordinates(NULL),
	m_pColorCoordinatesOfDepth(NULL),
	m_pDepthCoordinatesOfColor(NULL),
	m_bCalibrate(false),
	m_bFilter(false),
	m_bStreamOnlyBodies(false),
	m_bCaptureFrame(false),
	m_bConnected(false),
	m_bConfirmCaptured(false),
	m_bConfirmCalibrated(false),
	m_bShowDepth(false),
	m_bFrameCompression(false), //for some reason the library for compression wasn't working with the latest VS (further, we may encounter issues with the buffer size when creating the frames, as we copy the vectors)
	m_iCompressionLevel(2),
	m_pClientSocket(NULL),
	m_tcpConn(1),
	m_sourceID(1),
	m_randomLosses (0.0),
	m_nFilterNeighbors(10),
	m_fFilterThreshold(0.01f),
	m_logOutputTimers(NULL),
	m_flagOutput(false),
	m_bSocketThread(true),
	m_bNtpThread(true),
	m_offsetUtcClock(0),
	m_logOutputFps (NULL),
	m_lastTs(std::chrono::system_clock::now()),
	m_lastTsFps(std::chrono::system_clock::now()),
	m_pktCtr(0),
	m_droppedFrame(0)
{
	pCapture = new KinectCapture();
	m_tsPointer = new Timestamp();
	m_clBuffer = new ClientBuffer();

    LARGE_INTEGER qpf = {0};
    if (QueryPerformanceFrequency(&qpf))
    {
        m_fFreq = double(qpf.QuadPart);
    }

	m_vBounds.push_back(-0.5);
	m_vBounds.push_back(-0.5);
	m_vBounds.push_back(-0.5);
	m_vBounds.push_back(0.5);
	m_vBounds.push_back(0.5);
	m_vBounds.push_back(0.5);

	m_portPool = new int [MaxTcp] {48000,48001,48002,48003,48004,48005};

	calibration.LoadCalibration();
}
  
LiveScanClient::~LiveScanClient()
{
    // clean up Direct2D renderer
	if (m_hWnd)
	{
		delete m_hWnd;
		m_hWnd = NULL;
	}

    if (m_pDrawColor)
    {
        delete m_pDrawColor;
        m_pDrawColor = NULL;
    }

	if (m_pDepthRGBX)
	{
		delete m_pDepthRGBX;
		m_pDepthRGBX = NULL;
	}

	if (m_pCameraSpaceCoordinates)
	{
		delete m_pCameraSpaceCoordinates;
		m_pCameraSpaceCoordinates = NULL;
	}

	if (m_pColorCoordinatesOfDepth)
	{
		delete m_pColorCoordinatesOfDepth;
		m_pColorCoordinatesOfDepth = NULL;
	}

	if (m_pDepthCoordinatesOfColor)
	{
		delete m_pDepthCoordinatesOfColor;
		m_pDepthCoordinatesOfColor = NULL;
	}

	if (pCapture)
	{
		delete pCapture;
		pCapture = NULL;
	}

	if (m_logOutputTimers)
	{
		delete m_logOutputTimers;
		m_logOutputTimers = NULL;
	}

	if (m_logOutputFps)
	{
		delete m_logOutputFps;
		m_logOutputFps = NULL;
	}

	if (m_clBuffer)
	{
		m_clBuffer->~ClientBuffer();
		delete m_clBuffer;
		m_clBuffer = NULL;
	}

	if (m_pDepthRGBX)
	{
		delete[] m_pDepthRGBX;
		m_pDepthRGBX = NULL;
	}

	if (m_pCameraSpaceCoordinates)
	{
		delete[] m_pCameraSpaceCoordinates;
		m_pCameraSpaceCoordinates = NULL;
	}

	if (m_pColorCoordinatesOfDepth)
	{
		delete[] m_pColorCoordinatesOfDepth;
		m_pColorCoordinatesOfDepth = NULL;
	}

	if (m_pDepthCoordinatesOfColor)
	{
		delete[] m_pDepthCoordinatesOfColor;
		m_pDepthCoordinatesOfColor = NULL;
	}

	for (auto p : m_pClientSocket)
	{
		delete p;
	}
	m_pClientSocket.clear();

	if (m_portPool)
	{
		delete[] m_portPool;
	}

    // clean up Direct2D
    SafeRelease(m_pD2DFactory);
}

int LiveScanClient::Run(HINSTANCE hInstance, int nCmdShow)
{
    MSG       msg = {0};
    WNDCLASS  wc;

	// Dialog custom window class
    ZeroMemory(&wc, sizeof(wc));
    wc.style         = CS_HREDRAW | CS_VREDRAW;
    wc.cbWndExtra    = DLGWINDOWEXTRA;
    wc.hCursor       = LoadCursorW(NULL, IDC_ARROW);
    wc.hIcon         = LoadIconW(hInstance, MAKEINTRESOURCE(IDI_APP));
    wc.lpfnWndProc   = DefDlgProcW;
    wc.lpszClassName = L"LiveScanClientAppDlgWndClass";

    if (!RegisterClassW(&wc))
    {
        return 0;
    }

    // Create main application window
	HWND hWndApp = CreateDialogParamW(NULL, MAKEINTRESOURCE(IDD_APP), NULL, (DLGPROC)LiveScanClient::MessageRouter, reinterpret_cast<LPARAM>(this));

    // Show window
    ShowWindow(hWndApp, nCmdShow);

	std::thread t1(&LiveScanClient::SocketThreadFunction, this);
	std::thread t2(&LiveScanClient::NtpThreadFunction, this);
    // Main message loop
    while (WM_QUIT != msg.message)
    {
		UpdateFrame();
		auto start = std::chrono::system_clock::now();
        while (PeekMessageW(&msg, NULL, 0, 0, PM_REMOVE))
        {
            // If a dialog message will be taken care of by the dialog proc
            if (hWndApp && IsDialogMessageW(hWndApp, &msg))
            {
                continue;
            }

            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }
		auto end = std::chrono::system_clock::now();
		std::chrono::duration<double> elapsed_seconds = end - start;
		if (m_flagOutput)
		{
			stringstream ss;
			ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tPeekMessageW\t" << elapsed_seconds.count() * 1000.0; //in ms
			m_logOutputTimers->RedOutput(ss.str());
		}
    }
	m_bNtpThread = false;
	m_bSocketThread = false;
	t1.join();
	t2.join();
    return static_cast<int>(msg.wParam);
}

void LiveScanClient::UpdateFrame()
{
	auto start = std::chrono::system_clock::now();
	if (!pCapture->bInitialized)
	{
		auto endR = std::chrono::system_clock::now();
		std::chrono::duration<double> elapsed_secondsR = endR - start;
		if (m_flagOutput)
		{
			stringstream ss;
			ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tUpdateFrame\t" << elapsed_secondsR.count() * 1000.0; //in ms
			m_logOutputTimers->RedOutput(ss.str());
		}
		return;
	}

	auto start1 = std::chrono::system_clock::now();
	bool bNewFrameAcquired = pCapture->AcquireFrame();
	auto end1 = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds1 = end1 - start1;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tAcquireFrame\t" << elapsed_seconds1.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}

	if (!bNewFrameAcquired)
		return;

	auto start2 = std::chrono::system_clock::now();
	pCapture->MapDepthFrameToCameraSpace(m_pCameraSpaceCoordinates);
	auto end2 = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds2 = end2 - start2;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tMapDepthFrameToCameraSpace\t" << elapsed_seconds2.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
	auto start3 = std::chrono::system_clock::now();
	pCapture->MapDepthFrameToColorSpace(m_pColorCoordinatesOfDepth);
	auto end3 = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds3 = end3 - start3;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tMapDepthFrameToColorSpace\t" << elapsed_seconds3.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
	{
		StoreFrame(m_pCameraSpaceCoordinates, m_pColorCoordinatesOfDepth, pCapture->pColorRGBX, pCapture->vBodies, pCapture->pBodyIndex);

		if (m_bCaptureFrame)
		{
			auto start4 = std::chrono::system_clock::now();
			m_framesFileWriterReader.writeFrame(m_vLastFrameVertices, m_vLastFrameRGB);
			m_bConfirmCaptured = true;
			m_bCaptureFrame = false;
			auto end4 = std::chrono::system_clock::now();
			std::chrono::duration<double> elapsed_seconds4 = end4 - start4;
			if (m_flagOutput)
			{
				stringstream ss;
				ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\twriteFrame\t" << elapsed_seconds4.count() * 1000.0; //in ms
				m_logOutputTimers->RedOutput(ss.str());
			}
		}
	}

	if (m_bCalibrate)
	{		
		auto start5 = std::chrono::system_clock::now();
		Point3f *pCameraCoordinates = new Point3f[pCapture->nColorFrameWidth * pCapture->nColorFrameHeight];
		pCapture->MapColorFrameToCameraSpace(pCameraCoordinates);
		bool res = calibration.Calibrate(pCapture->pColorRGBX, pCameraCoordinates, pCapture->nColorFrameWidth, pCapture->nColorFrameHeight);

		delete[] pCameraCoordinates;

		if (res)
		{
			m_bConfirmCalibrated = true;
			m_bCalibrate = false;
		}
		auto end5 = std::chrono::system_clock::now();
		std::chrono::duration<double> elapsed_seconds5 = end5 - start5;
		if (m_flagOutput)
		{
			stringstream ss;
			ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tCalibrate\t" << elapsed_seconds5.count() * 1000.0; //in ms
			m_logOutputTimers->RedOutput(ss.str());
		}
	}

	if (!m_bShowDepth)
		ProcessColor(pCapture->pColorRGBX, pCapture->nColorFrameWidth, pCapture->nColorFrameHeight);
	else
		ProcessDepth(pCapture->pDepth, pCapture->nDepthFrameWidth, pCapture->nDepthFrameHeight);

	ShowFPS(); // FPS per Kinect sensor

	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tUpdateFrame\t" << elapsed_seconds.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
}

LRESULT CALLBACK LiveScanClient::MessageRouter(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    LiveScanClient* pThis = NULL;
    
    if (WM_INITDIALOG == uMsg)
    {
        pThis = reinterpret_cast<LiveScanClient*>(lParam);
        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pThis));
    }
    else
    {
        pThis = reinterpret_cast<LiveScanClient*>(::GetWindowLongPtr(hWnd, GWLP_USERDATA));
    }

    if (pThis)
    {
        return pThis->DlgProc(hWnd, uMsg, wParam, lParam);
    }

    return 0;
}

LRESULT CALLBACK LiveScanClient::DlgProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    UNREFERENCED_PARAMETER(wParam);
    UNREFERENCED_PARAMETER(lParam);

    switch (message)
    {
        case WM_INITDIALOG:
        {
            // Bind application window handle
            m_hWnd = hWnd;

            // Init Direct2D
            D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &m_pD2DFactory);

            // Get and initialize the default Kinect sensor
			bool res = pCapture->Initialize();
			if (res)
			{
				m_pDepthRGBX = new RGB[pCapture->nColorFrameWidth * pCapture->nColorFrameHeight];

				m_pCameraSpaceCoordinates = new Point3f[pCapture->nDepthFrameWidth * pCapture->nDepthFrameHeight];
				m_pColorCoordinatesOfDepth = new Point2f[pCapture->nDepthFrameWidth * pCapture->nDepthFrameHeight];
				m_pDepthCoordinatesOfColor = new Point2f[pCapture->nColorFrameWidth * pCapture->nColorFrameHeight];
			}
			else
			{
				SetStatusMessage(L"Capture device failed to initialize!", 10000, true);
			}

			// Create and initialize a new Direct2D image renderer (take a look at ImageRenderer.h)
			// We'll use this to draw the data we receive from the Kinect to the screen
			HRESULT hr;
			m_pDrawColor = new ImageRenderer();
			hr = m_pDrawColor->Initialize(GetDlgItem(m_hWnd, IDC_VIDEOVIEW), m_pD2DFactory, pCapture->nColorFrameWidth, pCapture->nColorFrameHeight, pCapture->nColorFrameWidth * sizeof(RGB));
			if (FAILED(hr))
			{
				SetStatusMessage(L"Failed to initialize the Direct2D draw device.", 10000, true);
			}

			ReadIPFromFile();
        }
        break;

        // If the titlebar X is clicked, destroy app
		case WM_CLOSE:			
			WriteIPToFile();
			DestroyWindow(hWnd);	
			delete this;
			break;
        case WM_DESTROY:
            // Quit the main message pump
            PostQuitMessage(0);
            break;
			
        // Handle button press
        case WM_COMMAND:
			if (IDC_BUTTON_CONNECT == LOWORD(wParam) && BN_CLICKED == HIWORD(wParam))
			{
				lock_guard<mutex> lock(m_mSocketThreadMutex);
				if (m_bConnected)
				{
					for (auto p : m_pClientSocket)
					{
						p->CloseConnection();
						delete p;
					}
					m_pClientSocket.clear();

					m_bConnected = false;
					SetDlgItemTextA(m_hWnd, IDC_BUTTON_CONNECT, "Connect");
				}
				else
				{
					try
					{
						char tcpConnections[1];
						GetDlgItemTextA(m_hWnd, IDC_TCPCONNECTIONS, tcpConnections, 2); // if I set GetDlgItemTextA(m_hWnd, IDC_TCPCONNECTIONS, tcpConnections, 1) it does not return anything
						if (isdigit(tcpConnections[0]))
							m_tcpConn = std::min(atoi(tcpConnections), MaxTcp);
						else						
							m_tcpConn = 1;

						char address[20];
						GetDlgItemTextA(m_hWnd, IDC_IP, address, 20);
						for (int ii = 0; ii < m_tcpConn; ii++)
						{
							SocketClient *Sock = new SocketClient(address, m_portPool[ii], m_pClientSocket.size());
							if (Sock->GetSocketCreationFlag())
								m_pClientSocket.push_back(Sock); //port needs to be selected from a pool of ports based on the number of TCP connections
							else
							{
								SetStatusMessage(L"Failed to connect. Did you start the server?", 10000, true);
							}
						}

						// set source ID
						char sourceID[3];
						GetDlgItemTextA(m_hWnd, IDC_EDIT_SOURCEID, sourceID, 3);
						
						// TODO check sourceID is digit
						/*
						ofstream myfile;
						myfile.open("example.txt");
						for (char num : sourceID) {
							myfile << num << "\n";
						}
						myfile.close();

						bool sourceIDisDigit = true;
						for (char num : sourceID) {
							if (!isdigit(num) && num != ' ')
								sourceIDisDigit = false;
						}

						if (sourceIDisDigit)
							m_sourceID = std::max(std::min(std::atoi(sourceID), 254), 0);
						else
							m_sourceID = 1;
						*/

						m_sourceID = std::max(std::min(std::atoi(sourceID), 254), 0);

						//get the random losses (m_randomLosses)		
						char frameLosses[10];
						GetDlgItemTextA(m_hWnd, IDC_FIELD_FRAMEDROP, frameLosses, 10);
						m_randomLosses = std::max(std::min(std::atof(frameLosses), 1.0), 0.0);

						m_bConnected = true;
						if (calibration.bCalibrated)
							m_bConfirmCalibrated = true;

						SetDlgItemTextA(m_hWnd, IDC_BUTTON_CONNECT, "Disconnect");
						//Clear the status bar so that the "Failed to connect..." disappears.
						SetStatusMessage(L"", 1, true);

						m_logOutputFps = new LoggingInfo();
						stringstream ss;
						std::filesystem::path pathV1 = std::filesystem::current_path();
						string pth_str = pathV1.string() + "\\logging_output";
						std::filesystem::path pathV2 = (pth_str);

						long dateTs = m_tsPointer->GetDateTs();
						string secTs = m_tsPointer->GetSecondsTs();

						if (!std::filesystem::exists(pathV2))
						{
							std::filesystem::create_directory(pathV2);
						}	
					
						ss << pth_str << "\\OutputFileFpsClient_" << dateTs << "_" << secTs << ".txt";
						m_logOutputFps->SetPath(ss.str());
						
						stringstream ssB;
						ssB << "UTC\tInterval[ms]\tFPS\tRandomLosses(%)\tDroppedFrames"; //in ms
						m_logOutputFps->RedOutput(ssB.str());

					}
					catch (...)
					{
						SetStatusMessage(L"Failed to connect. Did you start the server?", 10000, true);
					}
				}
			}
			if (IDC_BUTTON_SWITCH == LOWORD(wParam) && BN_CLICKED == HIWORD(wParam))
			{
				m_bShowDepth = !m_bShowDepth;

				if (m_bShowDepth)
				{
					SetDlgItemTextA(m_hWnd, IDC_BUTTON_SWITCH, "Show color");
				}
				else
				{
					SetDlgItemTextA(m_hWnd, IDC_BUTTON_SWITCH, "Show depth");
				}
			}
			if (IDC_BUTTON_PROFILING == LOWORD(wParam) && BN_CLICKED == HIWORD(wParam))
			{

				if (!m_flagOutput)
				{
					SetDlgItemTextA(m_hWnd, IDC_BUTTON_PROFILING, "Disabling Profiling");

					m_logOutputTimers = new LoggingInfo();
					stringstream ssB;

					std::filesystem::path pathV1 = std::filesystem::current_path();
					string pth_str = pathV1.string() + "\\logging_output";
					std::filesystem::path pathV2 = (pth_str);

					long dateTs = m_tsPointer->GetDateTs();
					string secTs = m_tsPointer->GetSecondsTs();

					if (std::filesystem::exists(pathV2))
					{
						ssB << pth_str << "\\OutputFileProfilingClient_" << dateTs << "_" << secTs << ".txt";
						m_logOutputTimers->SetPath(ssB.str());
					}
					else
					{
						std::filesystem::create_directory(pathV2);
						ssB << pth_str << "\\OutputFileProfilingClient_" << dateTs << "_" << secTs << ".txt";
						m_logOutputTimers->SetPath(ssB.str());
					}

					stringstream ss;
					ss << "UTC\tFunction\tms\tDstPort"; //in ms
					m_logOutputTimers->RedOutput(ss.str());
					m_flagOutput = true;
				}
				else
				{
					SetDlgItemTextA(m_hWnd, IDC_BUTTON_PROFILING, "Enabling Profiling");
					m_flagOutput = false;
				}
			}
            break;
    }

    return FALSE;
}

void LiveScanClient::ProcessDepth(const UINT16* pBuffer, int nWidth, int nHeight)
{
	auto start = std::chrono::system_clock::now();
	// Make sure we've received valid data
	if (m_pDepthRGBX && m_pDepthCoordinatesOfColor && pBuffer && (nWidth == pCapture->nDepthFrameWidth) && (nHeight == pCapture->nDepthFrameHeight))
	{
		// end pixel is start + width*height - 1
		const UINT16* pBufferEnd = pBuffer + (nWidth * nHeight);

		pCapture->MapColorFrameToDepthSpace(m_pDepthCoordinatesOfColor);

		for (int i = 0; i < pCapture->nColorFrameWidth * pCapture->nColorFrameHeight; i++)
		{
			Point2f depthPoint = m_pDepthCoordinatesOfColor[i];
			BYTE intensity = 0;
			
			if (depthPoint.X >= 0 && depthPoint.Y >= 0)
			{
				int depthIdx = (int)(depthPoint.X + depthPoint.Y * pCapture->nDepthFrameWidth);
				USHORT depth = pBuffer[depthIdx];
				intensity = static_cast<BYTE>(depth % 256);
			}

			m_pDepthRGBX[i].rgbRed = intensity;
			m_pDepthRGBX[i].rgbGreen = intensity;
			m_pDepthRGBX[i].rgbBlue = intensity;
		}

		// Draw the data with Direct2D
		m_pDrawColor->Draw(reinterpret_cast<BYTE*>(m_pDepthRGBX), pCapture->nColorFrameWidth * pCapture->nColorFrameHeight * sizeof(RGB), pCapture->vBodies);
	}
	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tProcessDepth\t" << elapsed_seconds.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
}

void LiveScanClient::ProcessColor(RGB* pBuffer, int nWidth, int nHeight) 
{
	auto start = std::chrono::system_clock::now();
    // Make sure we've received valid data
	if (pBuffer && (nWidth == pCapture->nColorFrameWidth) && (nHeight == pCapture->nColorFrameHeight))
    {
        // Draw the data with Direct2D
		m_pDrawColor->Draw(reinterpret_cast<BYTE*>(pBuffer), pCapture->nColorFrameWidth * pCapture->nColorFrameHeight * sizeof(RGB), pCapture->vBodies);
    }
	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tProcessColor\t" << elapsed_seconds.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
}

bool LiveScanClient::SetStatusMessage(_In_z_ WCHAR* szMessage, DWORD nShowTimeMsec, bool bForce)
{
    INT64 now = GetTickCount64();

    if (m_hWnd && (bForce || (m_nNextStatusTime <= now)))
    {
        SetDlgItemText(m_hWnd, IDC_STATUS, szMessage);
        m_nNextStatusTime = now + nShowTimeMsec;

        return true;
    }

    return false;
}

void 
LiveScanClient::build_fd_sets(fd_set* read_fds, fd_set* write_fds)
{
	int max_sockets = m_pClientSocket.size();
	FD_ZERO(read_fds);
	for (int ii = 0; ii < max_sockets; ii++)
		if (m_pClientSocket.at(ii)->GetSocket() != INVALID_SOCKET)
			FD_SET(m_pClientSocket.at(ii)->GetSocket(), read_fds);

	FD_ZERO(write_fds);
	for (int ii = 0; ii < max_sockets; ii++) 
		if (m_pClientSocket.at(ii)->GetSocket() != INVALID_SOCKET) //here we could check whether there is anything in the buffer for us to send (?)
			FD_SET(m_pClientSocket.at(ii)->GetSocket(), write_fds);
}

void LiveScanClient::SocketThreadFunction()
{
	while (m_bSocketThread)
	{
		std::this_thread::sleep_for(std::chrono::milliseconds(2)); //seems that every 2 milliseconds checks about the message received from the server
		if (m_bConnected)
		{
			auto start = std::chrono::system_clock::now();
			lock_guard<mutex> lock(m_mSocketThreadMutex);
			fd_set read_fds;
			fd_set write_fds;
			int max_sockets = m_pClientSocket.size();

			timeval tv;
			tv.tv_sec = 0;
			tv.tv_usec = 1;

			build_fd_sets(&read_fds, &write_fds);
			//here we should be checking the status of the socket...
			//first if it is readable and then writeable...
			for (int ii = 0; ii < max_sockets; ii++)
			{
				int activity = select(m_pClientSocket.at(ii)->GetSocket() + 1, &read_fds, &write_fds, NULL, &tv);
				if (activity == 0 || activity == -1)
				{
					SetStatusMessage(L"Time to close connection with select: ", 10000, true);
					m_pClientSocket.at(ii)->CloseConnection(); // exit normally (?)
					m_pClientSocket.erase(m_pClientSocket.begin() + ii);
				}
				else
				{
					string received;
					std::vector<int> anythingToSend;
					if (m_pClientSocket.at(ii)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(ii)->GetSocket(), &read_fds))
					{
						received = m_pClientSocket.at(ii)->ReceiveBytes();
						if (received.length() > 0)
							HandleSocketReceived(received, anythingToSend, ii);
					}
					if (anythingToSend.size() > 0)
					{
						HandleSocketTransmit(anythingToSend, write_fds);
					}
				}
			}

			auto end = std::chrono::system_clock::now();
			std::chrono::duration<double> elapsed_seconds = end - start;
			if (m_flagOutput)
			{
				stringstream ssB;
				ssB << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tSocketThreadFunction\t" << elapsed_seconds.count() * 1000.0; //in ms
				m_logOutputTimers->RedOutput(ssB.str());
			}
		}		
	}
}

void
LiveScanClient::HandleSocketReceived(string received, std::vector<int>& anythingToSend, int portId)
{
	auto start = std::chrono::system_clock::now();

	for (unsigned int i = 0; i < received.length(); i++)
	{
		//capture a frame
		if (received[i] == MSG_CAPTURE_FRAME)
			m_bCaptureFrame = true;
		//calibrate
		else if (received[i] == MSG_CALIBRATE)
			m_bCalibrate = true;
		//receive settings
		//TODO: what if packet is split?
		else if (received[i] == MSG_RECEIVE_SETTINGS)
		{
			vector<float> bounds(6);
			i++;
			int nBytes = *(int*)(received.c_str() + i);
			i += sizeof(int);

			for (int j = 0; j < 6; j++)
			{
				bounds[j] = *(float*)(received.c_str() + i);
				i += sizeof(float);
			}
				
			m_bFilter = (received[i]!=0);
			i++;

			m_nFilterNeighbors = *(int*)(received.c_str() + i);
			i += sizeof(int);

			m_fFilterThreshold = *(float*)(received.c_str() + i);
			i += sizeof(float);

			m_vBounds = bounds;

			int nMarkers = *(int*)(received.c_str() + i);
			i += sizeof(int);

			calibration.markerPoses.resize(nMarkers);

			for (int j = 0; j < nMarkers; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					for (int l = 0; l < 3; l++)
					{
						calibration.markerPoses[j].R[k][l] = *(float*)(received.c_str() + i);
						i += sizeof(float);
					}
				}

				for (int k = 0; k < 3; k++)
				{
					calibration.markerPoses[j].t[k] = *(float*)(received.c_str() + i);
					i += sizeof(float);
				}

				calibration.markerPoses[j].markerId = *(int*)(received.c_str() + i);
				i += sizeof(int);
			}

			m_bStreamOnlyBodies = (received[i] != 0);
			i += 1;

			m_iCompressionLevel = *(int*)(received.c_str() + i);
			i += sizeof(int);
			if (m_iCompressionLevel > 0)
				m_bFrameCompression = true;
			else
				m_bFrameCompression = false;

			//so that we do not lose the next character in the stream
			i--;
		}
		//req stored frame
		else if (received[i] == MSG_REQUEST_STORED_FRAME)
		{
			anythingToSend.push_back(MSG_REQUEST_STORED_FRAME);
		}
		//req last frame
		else if (received[i] == MSG_REQUEST_LAST_FRAME)
		{
			anythingToSend.push_back(MSG_REQUEST_LAST_FRAME);
		}
		//receive calibration data
		else if (received[i] == MSG_RECEIVE_CALIBRATION)
		{
			i++;
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					calibration.worldR[j][k] = *(float*)(received.c_str() + i);
					i += sizeof(float);
				}
			}
			for (int j = 0; j < 3; j++)
			{
				calibration.worldT[j] = *(float*)(received.c_str() + i);
				i += sizeof(float);
			}

			//so that we do not lose the next character in the stream
			i--;
		}
		else if (received[i] == MSG_CLEAR_STORED_FRAMES)
		{
			m_framesFileWriterReader.closeFileIfOpened();
		}
	}

	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ssB;
		ssB << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tHandleSocketReceived\t" << elapsed_seconds.count() * 1000.0 << "\t" << m_portPool[portId]; //in ms
		m_logOutputTimers->RedOutput(ssB.str());
	}
}

void
LiveScanClient::HandleSocketTransmit(std::vector<int> signalInfoForTx, fd_set write_fds)
{
	auto start = std::chrono::system_clock::now();
	int _hdrSize = 13; // including all information (Note: This variable although it is applied in all frames, in this function it is used only for the cases where no frames or a signal was sent to the server (just to construct the header easily))

	for (int ii = 0; ii < signalInfoForTx.size(); ii++)
	{

		if (signalInfoForTx.at(ii) == MSG_REQUEST_STORED_FRAME) 
		{			
			vector<Point3s> points;
			vector<RGB> colors;
			bool res = m_framesFileWriterReader.readFrame(points, colors);
			if (res == false)
			{
				for (int jj = m_pClientSocket.size() - 1; jj >= 0; jj--)
				{
					if (m_pClientSocket.at(jj)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(jj)->GetSocket(), &write_fds))
					{
						vector<char> _byteSend (_hdrSize, MSG_NO_FRAME);
						SendFrame(_byteSend, jj);
						break;
					}
				}
			}
			else
			{
				std::vector<char> finalVec;
				uint32_t tsCreation = 0;
				char byteToSend = MSG_SEND_STORED_FRAME;
				CreateFramesReadyForTransmission(points, colors, m_vLastFrameBody, finalVec, tsCreation);
				for (int jj = m_pClientSocket.size() - 1; jj >= 0; jj--)
				{
					if (m_pClientSocket.at(jj)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(jj)->GetSocket(), &write_fds))
					{
						finalVec.insert(finalVec.begin(), byteToSend);
						SendFrame(finalVec, jj);
						break;
					}
				}				
			}
		}
		else if (signalInfoForTx.at(ii) == MSG_REQUEST_LAST_FRAME) 
		{
			//here we check if new frames have been captured, before sending them			
			vector<vector<char> > bufferToTx;
			int numOfFrames = m_clBuffer->Dequeue(bufferToTx, m_offsetUtcClock);

			if (m_flagOutput)
			{
				stringstream ss;
				ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tHandleSocketTransmit START FRAMES \t" << numOfFrames; //in ms
				m_logOutputTimers->RedOutput(ss.str());
			}

			//todo: send everything in one go (watch socket overflow and need to create new headers to distinguish the aggregated packets at the receiver)
			//      and get rid of the byteToSend = MSG_LAST_FRAME; --> this requires changes at the server ( else if (buffer[0] == 3) and in the end it 
			//      is called again with the buffer = lClientSockets[i].Receive(1);)
			if (numOfFrames < 1)
			{
				for (int jj = m_pClientSocket.size() - 1; jj >= 0; jj--)
				{
					if (m_pClientSocket.at(jj)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(jj)->GetSocket(), &write_fds))
					{
						vector<char> _byteSend(_hdrSize, MSG_NO_FRAME);
						SendFrame(_byteSend, jj);
						break;
					}
				}
			}
			else
			{
				char byteToSend = MSG_SEND_LAST_FRAME;
				int socketInitial = m_pClientSocket.size() - 1;
				for (int ii = 0; ii < numOfFrames; ii++) //todo: in the future, concatenate all frames into one and send them in one go... of course, we need to modify header
				{
					bool flagTcpMultConSend = false;
					while (!flagTcpMultConSend)
					{
						for (int jj = socketInitial; jj >= 0; jj--)
						{
							if (m_pClientSocket.at(jj)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(jj)->GetSocket(), &write_fds))
							{								
								flagTcpMultConSend = true;
								vector<char> _bufferTx;
								_bufferTx = bufferToTx.at(ii);
								_bufferTx.insert(_bufferTx.begin(), byteToSend);
								SendFrame(_bufferTx, jj);
							}
							if (flagTcpMultConSend)
							{
								socketInitial = max(jj-1,0); // This is for Multiple TCP connections (try on the next available socket)
								break;
							}
							else
							{
								socketInitial = m_pClientSocket.size() - 1; // Start checking from the start again
							}
						}
					}
				}
			}
		}
	}

	if (m_bConfirmCaptured)
	{
		vector<char> _byteSend(_hdrSize, MSG_CONFIRM_CAPTURED);

		for (int jj = m_pClientSocket.size() - 1; jj >= 0; jj--)
		{
			if (m_pClientSocket.at(jj)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(jj)->GetSocket(), &write_fds))
			{
				SendFrame(_byteSend, jj);
				break;
			}
		}
		m_bConfirmCaptured = false;
	}

	if (m_bConfirmCalibrated)
	{
		int size = (9 + 3) * sizeof(float) + sizeof(int) + _hdrSize;
		char* buffer = new char[size];
		vector<char> _byteSend(_hdrSize, MSG_CONFIRM_CALIBRATED);
		memcpy(buffer, _byteSend.data(), _hdrSize);
		int i = _hdrSize;

		memcpy(buffer + i, &calibration.iUsedMarkerId, 1 * sizeof(int));
		i += 1 * sizeof(int);
		memcpy(buffer + i, calibration.worldR[0].data(), 3 * sizeof(float));
		i += 3 * sizeof(float);
		memcpy(buffer + i, calibration.worldR[1].data(), 3 * sizeof(float));
		i += 3 * sizeof(float);
		memcpy(buffer + i, calibration.worldR[2].data(), 3 * sizeof(float));
		i += 3 * sizeof(float);
		memcpy(buffer + i, calibration.worldT.data(), 3 * sizeof(float));
		i += 3 * sizeof(float);

		for (int jj = m_pClientSocket.size() - 1; jj >= 0; jj--)
		{
			if (m_pClientSocket.at(jj)->GetSocket() != INVALID_SOCKET && FD_ISSET(m_pClientSocket.at(jj)->GetSocket(), &write_fds))
			{
				m_pClientSocket.at(jj)->SendBytes(buffer, size);
				break;
			}
		}
		m_bConfirmCalibrated = false;
	}

	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tHandleSocketTransmit\t" << elapsed_seconds.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
}

void 
LiveScanClient::SendFrame(vector<char> frameTx, int sockId)
{	
	m_pClientSocket.at(sockId)->SendBytes(frameTx.data(), frameTx.size());
}

void LiveScanClient::StoreFrame(Point3f *vertices, Point2f *mapping, RGB *color, vector<Body> &bodies, BYTE* bodyIndex)
{
	auto start = std::chrono::system_clock::now();
	vector<Point3f> goodVertices;
	vector<RGB> goodColorPoints;

	unsigned int nVertices = pCapture->nDepthFrameWidth * pCapture->nDepthFrameHeight;

	for (unsigned int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
	{
		if (m_bStreamOnlyBodies && bodyIndex[vertexIndex] >= bodies.size())
			continue;

		if (vertices[vertexIndex].Z >= 0 && mapping[vertexIndex].Y >= 0 && mapping[vertexIndex].Y < pCapture->nColorFrameHeight)
		{
			Point3f temp = vertices[vertexIndex];
			RGB tempColor = color[(int)mapping[vertexIndex].X + (int)mapping[vertexIndex].Y * pCapture->nColorFrameWidth];
			if (calibration.bCalibrated)
			{
				temp.X += calibration.worldT[0];
				temp.Y += calibration.worldT[1];
				temp.Z += calibration.worldT[2];
				temp = RotatePoint(temp, calibration.worldR);

				if (temp.X < m_vBounds[0] || temp.X > m_vBounds[3]
					|| temp.Y < m_vBounds[1] || temp.Y > m_vBounds[4]
					|| temp.Z < m_vBounds[2] || temp.Z > m_vBounds[5])
					continue;
			}

			goodVertices.push_back(temp);
			goodColorPoints.push_back(tempColor);
		}
	}

	vector<Body> tempBodies = bodies;

	for (unsigned int i = 0; i < tempBodies.size(); i++)
	{
		for (unsigned int j = 0; j < tempBodies[i].vJoints.size(); j++)
		{
			if (calibration.bCalibrated)
			{
				tempBodies[i].vJoints[j].Position.X += calibration.worldT[0];
				tempBodies[i].vJoints[j].Position.Y += calibration.worldT[1];
				tempBodies[i].vJoints[j].Position.Z += calibration.worldT[2];

				Point3f tempPoint(tempBodies[i].vJoints[j].Position.X, tempBodies[i].vJoints[j].Position.Y, tempBodies[i].vJoints[j].Position.Z);

				tempPoint = RotatePoint(tempPoint, calibration.worldR);

				tempBodies[i].vJoints[j].Position.X = tempPoint.X;
				tempBodies[i].vJoints[j].Position.Y = tempPoint.Y;
				tempBodies[i].vJoints[j].Position.Z = tempPoint.Z;
			}
		}
	}

	if (m_bFilter)
		filter(goodVertices, goodColorPoints, m_nFilterNeighbors, m_fFilterThreshold);

	vector<Point3s> goodVerticesShort(goodVertices.size());

	for (unsigned int i = 0; i < goodVertices.size(); i++)
	{
		goodVerticesShort[i] = goodVertices[i];
	}

	m_vLastFrameBody = tempBodies;
	m_vLastFrameVertices = goodVerticesShort;
	m_vLastFrameRGB = goodColorPoints;
	std::vector<char> finalVec;

	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tStoreFrame\t" << elapsed_seconds.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}

	if (m_bConnected && goodVerticesShort.size() > 0 && m_pClientSocket.size() > 0)
	{
		uint32_t tsCreation = 0;
		double randomDrop = rand();
		double probRand = randomDrop / RAND_MAX;
		if (probRand > m_randomLosses)
		{
			CreateFramesReadyForTransmission(goodVerticesShort, goodColorPoints, tempBodies, finalVec, tsCreation);
			m_clBuffer->Enqueue(finalVec, tsCreation, m_offsetUtcClock);
			m_pktCtr++;
		}
		else
			m_droppedFrame++;
	}
}

void 
LiveScanClient::CreateFramesReadyForTransmission(vector<Point3s> vertices, vector<RGB> RGB, vector<Body> body, vector<char>& finalVec, uint32_t& tsCreation)
{
	auto start = std::chrono::system_clock::now();
	unsigned int size = sizeof(char) + sizeof(int) + (unsigned int)RGB.size() * (3 + 3 * sizeof(short));
					//	sourceID		nVerts			RGB								Verts

	vector<char> buffer(size);
	char* ptr2 = (char*)vertices.data();
	int pos = 0;

	// SOURCEID
	char sourceID = (char) m_sourceID;
	memcpy(buffer.data() + pos, &sourceID, sizeof(sourceID));
	pos += sizeof(sourceID);

	// nVERTS
	unsigned int nVertices = (unsigned int)RGB.size();
	memcpy(buffer.data() + pos, &nVertices, sizeof(nVertices));
	pos += sizeof(nVertices);
	for (size_t i = 0; i < RGB.size(); i++)
	{
		buffer[pos++] = RGB[i].rgbRed;
		buffer[pos++] = RGB[i].rgbGreen;
		buffer[pos++] = RGB[i].rgbBlue;

		memcpy(buffer.data() + pos, ptr2, sizeof(short) * 3);
		ptr2 += sizeof(short) * 3;
		pos += sizeof(short) * 3;
	}

	unsigned int nBodies = (unsigned int)body.size();
	size += sizeof(nBodies);

	unsigned int nBodiesTemp = 0;
	std::vector<unsigned int> nBodiesPos;
	for (unsigned int i = 0; i < nBodies; i++)
	{
		if (body[i].bTracked)
		{
			nBodiesTemp++;
			size += sizeof(body[i].bTracked);
			unsigned int nJoints = (unsigned int)body[i].vJoints.size();
			size += sizeof(nJoints);
			size += nJoints * (3 * sizeof(float) + 2 * sizeof(int));
			size += nJoints * (2 * sizeof(float));
			nBodiesPos.push_back(i);
		}
	}
	nBodies = nBodiesTemp;
	buffer.resize(size);
	memcpy(buffer.data() + pos, &nBodies, sizeof(nBodies));
	pos += sizeof(nBodies);

	for (unsigned int ii = 0; ii < nBodies; ii++)
	{
		unsigned int i = nBodiesPos.at(ii);
		memcpy(buffer.data() + pos, &body[i].bTracked, sizeof(body[i].bTracked));
		pos += sizeof(body[i].bTracked);

		unsigned int nJoints = (unsigned int)body[i].vJoints.size();
		memcpy(buffer.data() + pos, &nJoints, sizeof(nJoints));
		pos += sizeof(nJoints);

		for (unsigned int j = 0; j < nJoints; j++)
		{
			//Joint
			memcpy(buffer.data() + pos, &body[i].vJoints[j].JointType, sizeof(JointType));
			pos += sizeof(JointType);
			memcpy(buffer.data() + pos, &body[i].vJoints[j].TrackingState, sizeof(TrackingState));
			pos += sizeof(TrackingState);
			//Joint position
			memcpy(buffer.data() + pos, &body[i].vJoints[j].Position.X, sizeof(float));
			pos += sizeof(float);
			memcpy(buffer.data() + pos, &body[i].vJoints[j].Position.Y, sizeof(float));
			pos += sizeof(float);
			memcpy(buffer.data() + pos, &body[i].vJoints[j].Position.Z, sizeof(float));
			pos += sizeof(float);

			//JointInColorSpace
			memcpy(buffer.data() + pos, &body[i].vJointsInColorSpace[j].X, sizeof(float));
			pos += sizeof(float);
			memcpy(buffer.data() + pos, &body[i].vJointsInColorSpace[j].Y, sizeof(float));
			pos += sizeof(float);
		}
	}

	int iCompression = static_cast<int>(m_bFrameCompression);

	if (m_bFrameCompression)
	{
		// *2, because according to zstd documentation, increasing the size of the output buffer above a 
		// bound should speed up the compression.
		unsigned int cBuffSize = (unsigned int)ZSTD_compressBound(size) * 2;
		vector<char> compressedBuffer(cBuffSize);
		unsigned int cSize = (unsigned int)ZSTD_compress(compressedBuffer.data(), cBuffSize, buffer.data(), size, m_iCompressionLevel);
		size = cSize;
		buffer = compressedBuffer;
	}

	uint32_t ts_field = m_tsPointer->GetMillisecondsTs(m_offsetUtcClock); // 32-bit for carrying the timestamp
	tsCreation = ts_field;
	const int hdr_size = sizeof(size) + sizeof(iCompression) + sizeof(ts_field); // hdr = information about i) size ii) compression iii) timestamp
	char header[hdr_size];
	memcpy(header, (char*)& size, sizeof(size));
	memcpy(header + sizeof(size), (char*)& iCompression, sizeof(iCompression));
	memcpy(header + sizeof(size) + sizeof(iCompression), (char*)& ts_field, sizeof(ts_field));

	std::vector<char> headerVec(header, header + hdr_size);
	finalVec.insert(finalVec.begin(), headerVec.begin(), headerVec.end());
	finalVec.insert(finalVec.end(), buffer.begin(), buffer.end());	

	auto end = std::chrono::system_clock::now();
	std::chrono::duration<double> elapsed_seconds = end - start;
	if (m_flagOutput)
	{
		stringstream ss;
		ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tCreateFramesReadyForTransmission\t" << elapsed_seconds.count() * 1000.0; //in ms
		m_logOutputTimers->RedOutput(ss.str());
	}
}

void LiveScanClient::ShowFPS()
{
	if (m_hWnd)
	{
		auto start = std::chrono::system_clock::now();
		double fps = 0.0;

		LARGE_INTEGER qpcNow = { 0 };
		if (m_fFreq)
		{
			if (QueryPerformanceCounter(&qpcNow))
			{
				if (m_nLastCounter)
				{
					m_nFramesSinceUpdate++;
					fps = m_fFreq * m_nFramesSinceUpdate / double(qpcNow.QuadPart - m_nLastCounter);
				}
			}
		}

		WCHAR szStatusMessage[64];
		StringCchPrintf(szStatusMessage, _countof(szStatusMessage), L" FPS = %0.2f", fps);

		if (SetStatusMessage(szStatusMessage, 1000, false))
		{
			m_nLastCounter = qpcNow.QuadPart;
			m_nFramesSinceUpdate = 0;
		}

		uint32_t ts = m_tsPointer->GetMillisecondsTs(m_offsetUtcClock);
		auto _timeNow = std::chrono::system_clock::now();
		int _intervalMs = std::chrono::duration_cast<std::chrono::milliseconds>(_timeNow - m_lastTs).count();
		if (_intervalMs > 10000) //get statistics every 10sec (no need to add complexity in per second)
		{
			m_lastTs = _timeNow;
			for (int ii = 0; ii < m_pClientSocket.size(); ii++)
			{
				m_pClientSocket.at(ii)->SetUpStatistics();
			}
		}

		_intervalMs = std::chrono::duration_cast<std::chrono::milliseconds>(_timeNow - m_lastTsFps).count();
		//save fps to a file! (dude high complexity with all read/write)
		if (_intervalMs > 1000 && m_logOutputFps != NULL)
		{
			double _fps = 1000.0 * ((double)m_pktCtr / (_intervalMs));
			uint32_t _droppedFrames = m_droppedFrame;
			m_droppedFrame = 0;
			stringstream ss;
			ss << ts << "\t" << _intervalMs << "\t" << _fps << "\t" << m_randomLosses << "\t" << _droppedFrames; //in ms
			m_logOutputFps->RedOutput(ss.str());
			m_lastTsFps = _timeNow;
			m_pktCtr = 0;
		}

		auto end = std::chrono::system_clock::now();
		std::chrono::duration<double> elapsed_seconds = end - start;
		if (m_flagOutput)
		{
			stringstream ss;
			ss << date::format(" %T", floor<std::chrono::milliseconds>(std::chrono::system_clock::now())) << "\tFPS\t" << elapsed_seconds.count() * 1000.0; //in ms
			m_logOutputTimers->RedOutput(ss.str());
		}
	}
}

void 
LiveScanClient::NtpThreadFunction()
{
	while (m_bNtpThread)
	{
		NtpClient* _ntpPtr = new NtpClient();
		bool _timeNtp = _ntpPtr->Connect();
		if (_timeNtp)
			m_offsetUtcClock = _ntpPtr->GetClockOffset();
		this_thread::sleep_for(std::chrono::minutes(10));
	}
}

void LiveScanClient::ReadIPFromFile()
{
	ifstream file;
	file.open("lastIP.txt");
	if (file.is_open())
	{
		char lastUsedIPAddress[20];
		file.getline(lastUsedIPAddress, 20);
		file.close();
		SetDlgItemTextA(m_hWnd, IDC_IP, lastUsedIPAddress);
	}
}

void LiveScanClient::WriteIPToFile()
{
	ofstream file;
	file.open("lastIP.txt");
	char lastUsedIPAddress[20];
	GetDlgItemTextA(m_hWnd, IDC_IP, lastUsedIPAddress, 20);
	file << lastUsedIPAddress;
	file.close();
}
