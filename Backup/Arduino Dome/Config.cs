using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using ASCOM.Utilities;
using ASCOM.Interface;

namespace ASCOM.Arduino
{
    [ComVisible(false)]
    public class Config
    {

        private bool _Link                  = false;
        private bool _IsSlewing             = false;
        private bool _Parked                = false;
        private bool _Slaved                = false;
        private bool _Synced                = false;

        private double _Azimuth             = 0;
        private double _ParkPosition        = 0;

        private string _ComPort             = "COM4";     // Com port

        private ShutterState _ShutterStatus = ShutterState.shutterClosed;

        private Profile _Profile            = new Profile();

        public Config()
        {
            this._Profile.DeviceType = "Dome";

            try
            {
                this._ParkPosition = this.GetDouble("ParkPosition");
            }
            catch { }
        }




        private void WriteValue(string key, string value)
        {
            this._Profile.WriteValue(ASCOM.Arduino.Dome.s_csDriverID, key, value);
        }

        private void WriteValue(string key, int value)
        {
            this.WriteValue(key, value.ToString());
        }

        private void WriteValue(string key, double value)
        {
            this.WriteValue(key, value.ToString());
        }

        private void WriteValue(string key, bool value)
        {
            this.WriteValue(key, ((value == true) ? 1 : 0).ToString());
        }

        private string GetString(string key)
        {
            return this._Profile.GetValue(ASCOM.Arduino.Dome.s_csDriverID, key);
        }

        private int GetInt(string key)
        {
            return Int32.Parse(this.GetString(key));
        }

        private double GetDouble(string key)
        {
            return double.Parse(this.GetString(key));
        }

        private bool GetBool(string key)
        {
            return (this.GetInt(key) == 0) ? false : true;
        }




        public Profile Profile
        {
            get { return this._Profile; }
            set { this._Profile = value; }
        }

        public string ComPort
        {
            get { return this._ComPort; }
            set
            {
                this.WriteValue("ComPort", value);
                this._ComPort = value;
            }
        }

        public bool Link
        {
            get { return this._Link; }
            set { this._Link = value; }
        }

        public bool IsSlewing
        {
            get { return this._IsSlewing; }
            set { this._IsSlewing = value; }
        }

        public bool Parked
        {
            get { return this._Parked; }
            set { this._Parked = value; }
        }

        public bool Slaved
        {
            get { return this._Slaved; }
            set { this._Slaved = value; }
        }

        public bool Synced
        {
            get { return this._Synced; }
            set { this._Synced = value; }
        }

        public double Azimuth
        {
            get { return this._Azimuth; }
            set { this._Azimuth = value; }
        }

        public double ParkPosition
        {
            get { return this._ParkPosition; }
            set 
            {
                this.WriteValue("ParkPosition", value);
                this._ParkPosition = value; 
            }
        }

        public ShutterState ShutterStatus
        {
            get { return this._ShutterStatus; }
            set { this._ShutterStatus = value; }
        }
    }
}
