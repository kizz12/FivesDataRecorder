using System;
using System.Threading;


namespace FivesDataRecorder {

    class MainClass {

        public static bool endRecording;
        public static bool recording;
        public static Thread recordThread;

        public static void startCode() { //just here for clears to keep the menu up
            Console.WriteLine("Welcome to the Fives Data Recorder!");
            Console.WriteLine("1 to begin recording (W/Timer) \n2 to adjust settings \n3 to clear the screen \n4 to stop recording (EMERGENCY ONLY) \nAnything else to exit");
            return;
        }

        public static void settingsMenu() { //settings menu code
            Console.Clear();
            Console.WriteLine("Configuration Menu:");
            Console.WriteLine("1 to set recording time \n2 to change com port \n3 to change baud rate \nAnything else to return to the main menu");
            return;
        }

        public static string getInput() { //grabs input from user
            string userResult = Console.ReadLine();
            return userResult;
        }

        public static void createThreadRecording() { //creates a recording thread so we can still do things while getting data from user
            if (!recording) {
                recordThread = new Thread(checkForData);//create a new thread
                recordThread.IsBackground = true; //assign it to background
                recording = true;//mark it as listening
                recordThread.Start();//launch the thread
            }
        }

        public static void checkForData() { //called on createThreadRecording as a thread
            while (!endRecording) {
                if (RecordData.boxDataReady) {
                    RecordData.recordData(RecordData.serialValue);
                    //Console.WriteLine(RecordData.serialValue);
                    RecordData.boxDataReady = false;
                }
            }
        }

        public static void stopRecording(object source, System.Timers.ElapsedEventArgs e) { //stops the recording and checks & resets various values to normal
            Console.Clear();
            endRecording = true;
            recording = false;

            RecordData.stopArduino();
            RecordData.boxDataReady = false;
            if (COMManager.streamOpen) {
                COMManager.CloseCom();
            }
            if (RecordData.fs != null) {
                RecordData.fs.Close ();
            }
            if (RecordData.file != null) {
                RecordData.file.Close ();
            }
            startCode();
        }

        public static void startTimer() { //starts a timer to record data
            System.Timers.Timer timer = new System.Timers.Timer(((RecordData.recordTime*1000)+400));
            timer.Elapsed += stopRecording;
            timer.AutoReset = false;
            timer.Enabled = true;
        }

        public static void Main() { //primary program loop

            while (true) {
                startCode();
                string res = getInput();

                if (res == "1") {
                    Console.Clear();
                    COMManager.OpenCom();
                    RecordData.startArduino();
                    if (RecordData.RecordingData && COMManager.streamOpen) {
                        endRecording = false;
                        Console.WriteLine("Starting data recording, please wait "+RecordData.recordTime+"s.");
                        createThreadRecording();
                        startTimer();
                    } else {
                        Console.WriteLine("Failed to start recording. Were we unable to open a serial port?");
                    }

                } else if (res == "2") {
                    settingsMenu();
                    string newResult = getInput();
                    switch(newResult) { //contains various results for different inputs
                        case "1": 
                            Console.WriteLine("How long would you like to record (seconds) | Default is 5 seconds:");
                            string period = getInput();
                            try {
                                RecordData.recordTime = int.Parse(period);
                                Console.WriteLine("Recording time set to "+period+" seconds.");
                            } catch(Exception e) {
                                Console.WriteLine("Failed to set recording time. Did you type an integer? Error: "+ e);
                            }
                            //do recording
                            break;
                        case "2": 
                            Console.WriteLine("Please enter the new COM port (default is /dev/ttymxc3 | Current COM: "+COMManager.comPort+"):");
                            string newCom = getInput();
                            COMManager.comPort = newCom;
                            Console.WriteLine("COM port is now set to: " + newCom);  
                            break;
                        case "3":
                            Console.WriteLine("Please enter the new BAUD rate (default is 115200 | Current BAUD: "+COMManager.baudRate+"):");
                            string newBaud = getInput();
                            int newFinalBaud;
                            try {
                                newFinalBaud = int.Parse(newBaud);
                                COMManager.baudRate = newFinalBaud;
                                Console.WriteLine("BAUD rate is now set to: " + newFinalBaud);
                            } catch(Exception e){
                                Console.WriteLine("Failed to set BAUD. Did you type an integer? Error: "+ e);
                            }
                            break;
                        default:
                            Console.Clear();
                            break;
                    }
                } else if(res == "3") {
                    Console.Clear();
                    //return;

                } else if(res == "4") {
                    Console.Clear();
                    endRecording = true;
                    recording = false;
                    RecordData.stopArduino();
                    RecordData.boxDataReady = false;
                    if (COMManager.streamOpen) {
                        COMManager.CloseCom();
                    }
                } else {
                    Console.Clear();
                    Console.WriteLine("Exiting...");
                    if (!endRecording) {
                        endRecording = true;
                    }
                    if (recording) {
                        recording = false;
                    }
                    if (COMManager.streamOpen) {
                        COMManager.CloseCom();
                    }
                    break;
                }

            }

        }
    }
}

