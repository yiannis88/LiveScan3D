using System.Collections.Generic;

namespace LiveScanPlayer
{
    interface IFrameFileReader
    {
        int frameIdx
        {
            get;
            set;
        }

        void ReadFrame(List<float> vertices, List<byte> colors);

        void JumpToFrame(int frameIdx);

        void Rewind();
    }
}
