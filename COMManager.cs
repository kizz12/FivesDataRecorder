using System;
using System.IO.Ports;
using System.Diagnostics;

namespace FivesDataRecorder {

    public static class COMManager { //this class opens the serial com port and creates an instance of the stream 



        public static int baudRate = 115200;//180000 min to support 10byte/ms bit rate
        public static string comPort = "/dev/ttymxc3";///dev/ttymxc3
        public static SerialPort stream;
        public static bool streamOpen;

        public static void OpenCom() { //in an effort to make this script static accessible, am moving start function to here where I can control opening and closing serial
            try {
                Console.WriteLine("Opening Port!");
                stream = new SerialPort(comPort, baudRate);//must close and reopen com to update com or baud - MUST WRITE UPDATE IF STREAM COM OR BAUD CHANGED!
                stream.ReadBufferSize = 40960;
                stream.Open();
            } catch (Exception e) {
                Console.WriteLine("Error on Connect: " + e);
                stream = null;
            }
            if (stream!=null) {
                streamOpen = true;
                Console.WriteLine("Connection Established! Port: " +comPort);
            }
        }
        public static void CloseCom() { //in an effort to make this script static accessible, am moving start function to here where I can control opening and closing serial
            Console.WriteLine("Closing serial connection!");
            try {
                if (stream!=null) {
                    while (RecordData.listening) {
                        RecordData.breakConnection = true;
                    }
                    stream.Close();
                    RecordData.RecordingData = false;
                    streamOpen = false;   
                    stream=null;
                }
            } catch (Exception e) { 
                Console.WriteLine("Error closing Serial Connection: " + e);
            }
        }

    }
}
