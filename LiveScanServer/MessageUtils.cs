/**
* This class has been created to hold the signalling information
* required for exchanging data between the devices.
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

namespace KinectServer
{
    public class MessageUtils
    {
        public enum SIGNAL_MESSAGE_TYPE
        {
            MSG_CAPTURE_FRAME,        // 0
            MSG_CALIBRATE,            // 1
            MSG_RECEIVE_SETTINGS,     // 2
            MSG_REQUEST_STORED_FRAME, // 3
            MSG_REQUEST_LAST_FRAME,   // 4
            MSG_RECEIVE_CALIBRATION,  // 5
            MSG_CLEAR_STORED_FRAMES,  // 6
            MSG_CONFIRM_CAPTURED,     // 7
            MSG_CONFIRM_CALIBRATED,   // 8
            MSG_SEND_STORED_FRAME,    // 9
            MSG_SEND_LAST_FRAME,      // 10
            MSG_NO_FRAME              // 11
        };
    }
}
