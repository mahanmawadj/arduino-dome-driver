using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Arduino.Properties;

namespace ASCOM.Arduino
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        public string ComPort { get; internal set; }
        public bool ShowDebugConsole { get; private set; }

        private Settings props = new Settings();
        public SetupDialogForm()
        {
            InitializeComponent();
            foreach (string item in System.IO.Ports.SerialPort.GetPortNames())
            {
                //store the each retrieved available prot names into the Listbox...
                comboBox1.Items.Add(item);
            }
        }

        public SetupDialogForm(string comPort, bool diagnosticConsole)
        {
            InitializeComponent();
            foreach (string item in System.IO.Ports.SerialPort.GetPortNames())
            {
                //store the each retrieved available prot names into the Listbox...
                comboBox1.Items.Add(item);
                
            }
            comboBox1.SelectedItem = comPort;
            checkBox1.Checked = diagnosticConsole;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            props.COMPORT = comboBox1.SelectedItem.ToString();
            props.DEBUGCONSOLE = checkBox1.Checked;
            props.Save();
            Dispose();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            this.ComPort = comboBox1.SelectedItem.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
            {
                this.ShowDebugConsole = true;
            }
            else this.ShowDebugConsole = false;
        }
    }
}