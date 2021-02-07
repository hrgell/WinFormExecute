using System;
using System.Windows.Forms;

namespace WinFormExecute
{
    public partial class Form1 : Form
    {
        long MyCount = 0;
        Executer executer = null;
        bool Exited;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TxtProgram.Text = @"C:\Users\Anonymous User\source\repos\BitType\Debug\BitType.exe";
            TxtArguments.Text = @"C:\Windows\notepad.exe";
            TxtTimeout.Text = "0";
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void BtnExecute_Click(object sender, EventArgs e)
        {
            ++MyCount;
            int timeout = 0;
            if(!int.TryParse(TxtTimeout.Text, out timeout))
            {
                TxtOutput.Text = "Error: Timeout must be an integer.";
                return;
            }
            BtnExecute.Enabled = false;
            // TODO Make sure a program that runs does not cause memory corruptiuon or leaks. if(executer != null) etc
            executer = new Executer(TxtProgram.Text, TxtArguments.Text, ExecuteStopped, timeout);
            Exited = executer.Run();
        }

        public void AddOutput(string txt)
        {
            TxtOutput.BeginInvoke((Action)delegate ()
            {
                TxtOutput.AppendText(txt);
            //TxtOutput.SelectionStart = TxtOutput.Text.Length;
            //TxtOutput.ScrollToCaret();
        });
        }

        void ExecuteStopped(object sender, EventArgs e)
        {
            string info = (Exited ? $"Program terminated with exit code {executer.ExitCode}." : "Program didn't terminate.");
            string msg = executer.StdOut + info + Environment.NewLine + $"Clicks {MyCount}." + Environment.NewLine;
            AddOutput(msg);
            BtnExecute.BeginInvoke((Action)delegate () { BtnExecute.Enabled = true; });
        }
    } // class
} // namespace
