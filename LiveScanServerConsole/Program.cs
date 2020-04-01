using System;

namespace KinectServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerConsole console = new ServerConsole();
            console.Start();
        }
    }
}
