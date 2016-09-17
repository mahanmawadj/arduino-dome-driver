//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Dome driver for Arduino
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Dome interface version: 1.0
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	1.0.0	Initial edit, from ASCOM Dome Driver template
// --------------------------------------------------------------------------------
//
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.Interface;
using Arduino;
using Arduino.Properties;

namespace ASCOM.Arduino
{
    //
    // Your driver's ID is ASCOM.Arduino.Dome
    //
    // The Guid attribute sets the CLSID for ASCOM.Arduino.Dome
    // The ClassInterface/None addribute prevents an empty interface called
    // _Dome from being created and used as the [default] interface
    //
    [Guid("387409ed-6827-46d7-8b61-a61a724281e0")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Dome : IDome
    {
        //
        // Driver ID and descriptive string that shows in the Chooser
        //
        public static string s_csDriverID = "ASCOM.Arduino.Dome";
        public static string s_csDriverDescription = "Arduino Dome";

        private Settings props = new Settings();

        private ArduinoSerial SerialConnection;

        private Util HC = new Util();

        private Config Config = new Config();

        //
        // Constructor - Must be public for COM registration!
        //
        public Dome()
        {
            this.Config.ComPort = props.COMPORT;
            this.Config.DiagnosticConsole = props.DEBUGCONSOLE;
            // TODO Implement your additional construction here
        }

        #region ASCOM Registration
        //
        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        private static void RegUnregASCOM(bool bRegister)
        {
            Helper.Profile P = new Helper.Profile();
            P.DeviceTypeV = "Dome";					//  Requires Helper 5.0.3 or later
            if (bRegister)
                P.Register(s_csDriverID, s_csDriverDescription);
            else
                P.Unregister(s_csDriverID);
            try										// In case Helper becomes native .NET
            {
                Marshal.ReleaseComObject(P);
            }
            catch (Exception) { }
            P = null;
        }

        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }
        #endregion

        //
        // PUBLIC COM INTERFACE IDome IMPLEMENTATION
        //

        #region IDome Members

        public void AbortSlew()
        {
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.Abort);
        }

        public double Altitude
        {
            get { throw new PropertyNotImplementedException("Altitude", false); }
        }

        public bool AtHome
        {
            get { return this.Config.AtHome; }
        }

        public bool AtPark
        {
            get { return this.Config.Parked; }
        }

        public double Azimuth
        {
            get { return this.Config.Azimuth; }
        }

        public bool CanFindHome
        {
            get { return true; }
        }

        public bool CanPark
        {
            get { return false; }
        }

        public bool CanSetAltitude
        {
            get { return false; }
        }

        public bool CanSetAzimuth
        {
            get { return true; }
        }

        public bool CanSetPark
        {
            get { return false; }
        }

        public bool CanSetShutter
        {
            get { return true; }
        }

        public bool CanSlave
        {
            get { return true; }
        }

        public bool CanSyncAzimuth
        {
            get { return true; }
        }

        public void CloseShutter()
        {
            this.Config.ShutterStatus = ShutterState.shutterClosing;
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.CloseShutter);

            while (this.Config.ShutterStatus == ShutterState.shutterClosed)
                HC.WaitForMilliseconds(100);
        }

        public void CommandBlind(string Command)
        {
            // TODO Replace this with your implementation
            throw new MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string Command)
        {
            // TODO Replace this with your implementation
            throw new MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string Command)
        {
            // TODO Replace this with your implementation
            throw new MethodNotImplementedException("CommandString");
        }

        public bool Connected
        {
            get { return this.Config.Link; }
            set 
            {
                switch(value)
                {
                    case true:
                        this.Config.Link = this.ConnectDome();
                        break;
                    case false:
                        this.Config.Link = !this.DisconnectDome();
                        break;
                }
            }
        }

        private bool ConnectDome()
        {
            SerialConnection = new ArduinoSerial(StopBits.One, 57600, this.Config.ComPort, true, this.Config.DiagnosticConsole);
            SerialConnection.CommandQueueReady += new ArduinoSerial.CommandQueueReadyEventHandler(SerialConnection_CommandQueueReady);
            HC.WaitForMilliseconds(2000);

            return true;
        }

        void SerialConnection_CommandQueueReady(object sender, EventArgs e)
        {
            while (SerialConnection.CommandQueue.Count > 0)
            {
                string[] com_args = ((string)SerialConnection.CommandQueue.Pop()).Split(' ');

                string command = com_args[0];
                switch (command)
                {

                    case "P":
                        this.Config.Azimuth = Int32.Parse(com_args[1]);
                        this.Config.IsSlewing = false;
                        break;
                    case ":807#":
                        this.Config.ShutterStatus = ShutterState.shutterOpen;
                        break;
                    case ":808#":
                        this.Config.ShutterStatus = ShutterState.shutterClosed;
                        break;
                    case ":SYNC":
                        this.Config.Synced = true;
                        this.Config.Azimuth = Int32.Parse(com_args[1].TrimEnd('#'));
                        break;
                    case ":824":
                        this.Config.AtHome = false;
                        this.Config.Azimuth = Int32.Parse(com_args[1].TrimEnd('#'));
                        this.Config.IsSlewing = false;
                        break;
                    case "PARKED 1":
                        this.Config.Parked = true;
                        break;
                    case ":815#":
                        this.Config.AtHome = true;
                        break;
                    case ":999#":
                        this.Config.IsSlewing = false;
                        break;
                    default:
                        break;
                }
            }
        }

        private bool DisconnectDome()
        {
            SerialConnection.Close();
            SerialConnection.Dispose();
            SerialConnection = null;
            return true;
        }

        public string Description
        {
            get { return "Arduino Dome Driver."; }
        }

        public string DriverInfo
        {
            get { return "Driver for connecting an Arduino to ASCOM applications."; }
        }

        public void FindHome()
        {
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.Home);

            while (!this.Config.AtHome)
                HC.WaitForMilliseconds(100);
        }

        public short InterfaceVersion
        {
            get { return 1; }
        }

        public string Name
        {
            get { return "Arduino Dome"; }
        }

        public void OpenShutter()
        {
            this.Config.ShutterStatus = ShutterState.shutterOpening;
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.OpenShutter);

            while (this.Config.ShutterStatus == ShutterState.shutterOpening)
                HC.WaitForMilliseconds(100);
        }

        public void Park()
        {
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.Park);

            while (!this.Config.Parked)
                HC.WaitForMilliseconds(100);
        }

        public void SetPark()
        {
            this.Config.ParkPosition = this.Config.Azimuth;
        }

        public void SetupDialog()
        {
            using (var form = new SetupDialogForm(this.Config.ComPort, this.Config.DiagnosticConsole))
            {
                var result = form.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.Config.ComPort = form.ComPort;
                    this.Config.DiagnosticConsole = form.ShowDebugConsole;
                }
            }
        }

        public ShutterState ShutterStatus
        {
            get { return this.Config.ShutterStatus; }
        }

        public bool Slaved
        {
            get { return this.Config.Slaved; }
            set { this.Config.Slaved = value; }
        }

        public void SlewToAltitude(double Altitude)
        {
            throw new MethodNotImplementedException("SlewToAltitude");
        }

        public void SlewToAzimuth(double Azimuth)
        {
            if (Azimuth > 360 || Azimuth < 0)
                throw new Exception("Out of range");
            this.Config.IsSlewing = true;
            string fmt = "000";
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.Slew, Azimuth.ToString(fmt));

            while (this.Config.IsSlewing)
            {
                HC.WaitForMilliseconds(100);
            //    SerialConnection.SendStatus(ArduinoSerial.SerialCommand.SlewCheck);
            }
        }

        public bool Slewing
        {
            get { return this.Config.IsSlewing; }
        }

        public void SyncToAzimuth(double Azimuth)
        {
            this.Config.Synced = false;
            if (Azimuth > 360 || Azimuth < 0)
                throw new Exception("Out of range");
            string fmt = "000";
            SerialConnection.SendCommand(ArduinoSerial.SerialCommand.SyncToAzimuth, Azimuth.ToString(fmt));

            while (!this.Config.Synced)
                HC.WaitForMilliseconds(100);
        }

        #endregion
    }
}
