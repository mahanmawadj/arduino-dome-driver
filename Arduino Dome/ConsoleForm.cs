using System;
using System.Windows.Forms;

namespace Arduino
{
    public partial class ConsoleForm : Form
    {
        public delegate void NewStringEventHandler(object sender, StringEntryEventArgs args);

        public event NewStringEventHandler NewStringEntry;

        protected virtual void OnNewListEntry(string test)
        {
            if (NewStringEntry != null)
            {
                NewStringEntry(this, new StringEntryEventArgs(test));
            }
        }

        public String ConsoleText
        {
            get { return this.textBox1.Text; }
            set
            {
                try
                {
                    if (this.IsDisposed == false) this.textBox1.AppendText(DateTime.Now.ToUniversalTime() + " " + value + Environment.NewLine);
                }
                catch (Exception) { };
            }
        }

        public ConsoleForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnNewListEntry(this.textBox2.Text);
            this.textBox2.Clear();
        }

        public class StringEntryEventArgs : EventArgs
        {
            private readonly string test;

            public StringEntryEventArgs(string test)
            {
                this.test = test;
            }

            public string Test
            {
                get { return this.test; }
            }
        }
    }
}
