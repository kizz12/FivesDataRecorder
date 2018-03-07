using System;
using System.Linq;
using System.IO;
using System.Threading;


namespace FivesDataRecorder {

    public static class RecordData { //this class will contain functions that handle sending messages to arduino, as well as parsing and storing the data



        public static bool listening;
        private static Thread readThread;
        private static bool exitOnLoop;
        public static string serialValue;
        public static bool boxDataReady;
        public static bool breakConnection;
        public static string lastValue;
        public static bool RecordingData;
        public static bool nameSet;
        public static int recordTime = 5;
        public static string filePath = "";//@"C:\Users\chrisk.CORP\Desktop\DATA\";///home/debian/Desktop/DATA/
        public static string fileName = "AccelDataCSV";//AccelDataCSV
        public static StreamWriter file;
        public static FileStream fs;


        public static void recordData(string sval) { //pushes data into a file
            if (RecordingData) {
                if (sval.Length > 20) { //check length to prevent too short data
                    using (fs = new FileStream(filePath+fileName, FileMode.Append, FileAccess.Write, FileShare.None)) { //create and append data to new file for this recording
                        using (file = new StreamWriter(fs, System.Text.Encoding.UTF8, 65536)) { //write to file
                            file.Write (sval + "\n"); //write each line
                        }
                    }
                }
            }
        }

        public static void formatFile() { //checks every line to make sure its not corrupt and removes any lines broken, then updates the file
            var tempFile = Path.GetTempFileName();
            var linesToKeep = File.ReadLines (filePath + fileName).Where (l => l.Length > 22);//checks length of line and removes anything that isnt min length
            File.WriteAllLines (tempFile, linesToKeep);
            File.Delete (filePath+fileName);
            File.Move (tempFile, filePath+fileName);
            Console.WriteLine ("Formatting data complete.");
        }

        public static void StartListener(bool exitOnRead) { //puts socket in thread to listen so that main thread doesn't freeze
            if (!listening) {
                readThread = new Thread(ConnectListen);//create a new thread
                readThread.IsBackground = true; //assign it to background
                listening = true;//mark it as listening
                readThread.Start();//launch the thread
                exitOnLoop=exitOnRead;
            }
        }
        static void ConnectListen() { //This modified loop listener allows for interrupt due to timeout delay
            if (COMManager.stream != null) {
                while (!breakConnection) {
                    try {
                        serialValue =COMManager.stream.ReadExisting();
                        boxDataReady = true;
                        lastValue = serialValue;
                        if (exitOnLoop) { //if only 1x read, return here after a good read
                            listening = false;
                            return;
                        }   
                    } catch (TimeoutException) { //catch our timeout but don't exit, keep looping
                        //do nothing
                    } catch (Exception e) {
                        Console.WriteLine("Error on read: " + e);
                        COMManager.stream.Close(); 
                        COMManager.stream=null;
                        listening = false;
                        return;
                    }
                } 
                listening = false;
                return;
            }
            listening = false;
            return;
        }

        public static void startArduino() { //start the arduino recording process 
            try {
                if (COMManager.stream!= null) {
                    if (nameSet) {
                        fileName = "AccelDataCSV";
                        nameSet = false;
                    }
                    if (!nameSet) {
                        fileName = fileName + DateTime.Now.ToString("_dd_MM_yyyy-HH_mm_ss")+ ".csv";
                        Console.WriteLine("File Written:"+fileName);
                        nameSet = true;
                    }
                    string s = "pushData;";
                    COMManager.stream.Write(s);
                    RecordingData = true;
                    StartListener(false); //NEED TO CREATE A THREAD AND LISTEN ON IT FOR THE DATA
                    Console.WriteLine("Data collection started.");

                } else {
                    Console.WriteLine("Failed to start data collection, stream is not connected.");
                    RecordingData = false;
                }
            } catch (Exception e) {
                Console.WriteLine("Error starting data collection: " + e);
                RecordingData = false;
            }
        }

        public static void stopArduino() { //stop the recording and push to file/format file
            try {
                if (COMManager.stream!= null) {
                    while (listening) {
                        breakConnection = true;
                    }

                    breakConnection = false;
                    string s = "stopPush;";
                    COMManager.stream.Write(s);
                    formatFile();
                    Console.WriteLine("Recording had ended! Check the desktop DATA folder for latest file.");
                    RecordingData = false;

                } else {
                    Console.WriteLine("Failed to stop recording because port not open!");
                    RecordingData = false;
                }
            } catch (Exception e) {
                Console.WriteLine("Error when stopping recording: " + e);
                RecordingData = false;

            }
        }
    }
}
