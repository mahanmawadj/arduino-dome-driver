using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Ports;
using Arduino;

namespace ASCOM.Arduino
{
    class ArduinoSerial : SerialPort, IDisposable
    {
        private bool debug;
        public ConsoleForm consoleForm;

        ASCOM.Utilities.Util HC = new ASCOM.Utilities.Util(); // Helper class
        public Stack CommandQueue = new Stack(); // Our command received stack

        public delegate void CommandQueueReadyEventHandler(object sender, EventArgs e); // Our Process stack callback
        public event CommandQueueReadyEventHandler CommandQueueReady;  

        // Serial commands look like ": M 100 #"
        public struct SerialCommand
        {
            public struct CommandSyntax
            {
                public static string Start = "997";
                public static string End = "998";
                public static string Spacer = ",";
            }

            public struct StatusSyntax
            {
                public static string Start = "996";
                public static string End = "998";
                public static string Spacer = ",";
            }

            public static string Slew = "8";
            public static string Abort = "999";
            public static string Park = "PARK";
            public static string Home = "878";
            public static string OpenShutter = "801";
            public static string CloseShutter = "800";
            public static string SlewCheck = "804";
            public static string SyncToAzimuth = "9";
            public static bool AtHome = true;


        }

        public ArduinoSerial(StopBits stopBits, int baud, string comPort, bool autostart, bool diagnosticConsole)
        {
            debug = diagnosticConsole;
            if (diagnosticConsole)
            {
                consoleForm = new ConsoleForm();
                consoleForm.Show();
            }
            if (debug) this.consoleForm.ConsoleText = "ASCOM Arduino Driver - Console Enabled.";
            this.Parity = Parity.None;
            this.PortName = comPort;
            this.StopBits = stopBits;
            this.BaudRate = baud;
            if (debug) this.consoleForm.ConsoleText = String.Format("Parity: {0} Port: {1} StopBits: {2} BaudRate: {3} Autostart: {4}", Parity, PortName, StopBits, BaudRate, autostart);
            this.DataReceived += new SerialDataReceivedEventHandler(ArduinoSerial_DataReceived);

            if (autostart)
                if (!this.IsOpen)
                {
                    if (debug) this.consoleForm.ConsoleText = "Opening connection...";
                    this.Open();
                    if (debug && this.IsOpen ) this.consoleForm.ConsoleText = "Connection opened.";
                }
        }

        public ArduinoSerial() : this(StopBits.One, 57600, "COM4", true, true) { }

        private void ArduinoSerial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string received = this.ReadLine().Trim("\r".ToCharArray());
            if (debug) this.consoleForm.ConsoleText = "Received: " + received;
            CommandQueue.Push(received); // Push latest command onto the stack
            CommandQueueReady(this, e);
        }

        public void SendCommand(string command, object arg1)
        {
            string serialCommand = this.BuildSerialCommand(command, arg1);
            if (debug) this.consoleForm.ConsoleText = "Command Sent: " + serialCommand;
            if (this.IsOpen) this.Write(serialCommand);
        }

        public void SendCommand(string command)
        {
            string serialCommand = this.BuildSerialCommand(command);
            if (debug) this.consoleForm.ConsoleText = "Command Sent: " + serialCommand;
            if (this.IsOpen) this.Write(serialCommand);
        }

        public void SendStatus(string command, object arg1)
        {
            string serialCommand = this.BuildSerialCommand(command, arg1);
            if (debug) this.consoleForm.ConsoleText = "Status Sent: " + serialCommand;
            if (this.IsOpen) this.Write(serialCommand);
        }

        public void SendStatus(string command)
        {
            string serialCommand = this.BuildSerialCommand(command);
            if (debug) this.consoleForm.ConsoleText = "Status Sent: " + serialCommand;
            if (this.IsOpen) this.Write(serialCommand);
        }

        private string BuildSerialCommand(string command)
        {
            return SerialCommand.CommandSyntax.Start + 
                SerialCommand.CommandSyntax.Spacer + 
                command + 
                SerialCommand.CommandSyntax.Spacer + 
                SerialCommand.CommandSyntax.End;
        }

        private string BuildSerialCommand(string command, object arg1)
        {
            return SerialCommand.CommandSyntax.Start + 
                SerialCommand.CommandSyntax.Spacer + 
                command +  arg1.ToString() + 
                SerialCommand.CommandSyntax.Spacer + 
                SerialCommand.CommandSyntax.End;
        }

        private string BuildStatusCommand(string command)
        {
            return SerialCommand.StatusSyntax.Start +
                SerialCommand.StatusSyntax.Spacer +
                command +
                SerialCommand.StatusSyntax.Spacer +
                SerialCommand.StatusSyntax.End;
        }

        private string BuildStatusCommand(string command, object arg1)
        {
            return SerialCommand.StatusSyntax.Start +
                SerialCommand.StatusSyntax.Spacer +
                command +
                SerialCommand.StatusSyntax.Spacer +
                arg1.ToString() +
                SerialCommand.StatusSyntax.Spacer +
                SerialCommand.StatusSyntax.End;
        }

        public void ResetConnection()
        {
            if (debug) this.consoleForm.ConsoleText = "Resetting connection...";
            this.Close();
            HC.WaitForMilliseconds(1000);
            this.Open();
            HC.WaitForMilliseconds(3000);
        }

        bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (debug) this.consoleForm.ConsoleText = "Disposing! Goodbye!";
            if (!disposed)
            {
                if (disposing)
                {
                    consoleForm.Close();
                    consoleForm = null;
                }
                base.Dispose(disposing);
            }
            //dispose unmanaged resources
            disposed = true;
        }
    }
}
