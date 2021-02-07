using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

// lookat
// use realineasync instead of OutputDataReceived and ErrorDataReceived, because the current
// microsoft event based stdin/stderr way is buggy by default, ie do not use OutputDataReceived
// and ErrorDataReceived.

namespace WinFormExecute
{
    class Executer
    {
        public const int DefaultTimeout = 60000;
        public delegate void AtExitDelegate(object sender, EventArgs e);

        public string Path;
        public string Arguments;
        /// <summary>If AtExit is called when the program terminates or timeouts.</summary>
        AtExitDelegate AtExit;
        /// <summary>Timeout is in millisecs and specifies the maximum time to wait for the program to terminate. The defaul is 60 seconds.</summary>
        public int Timeout;
        public int ExitCode;
        public bool Exited = false;
        Process process = null;
        bool UseStdErr = true;
        public string StdOut = null;
        public string StdErr = null;
        Thread TStdOut;
        Thread TStdErr;

        public Executer(string path, string arguments, AtExitDelegate atexit = null, int timeout = DefaultTimeout)
        {
            Path = path;
            Arguments = arguments;
            AtExit = atexit;
            Timeout = timeout;
            ExitCode = 0;
            Debug.WriteLine($"timeout({Timeout})");
        }

        void ProcessExited(object sender, EventArgs e)
        {
            if (AtExit != null)
            {
                AtExit(sender, e);
            }
        }

        public bool Run()
        {
            // Setup the proccess
            var info = new ProcessStartInfo
            {
                FileName = Path,
                Arguments = Arguments,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            // TODO dispose of the previous process, if any.
            process = new Process { EnableRaisingEvents = true, StartInfo = info };

            // Run the process
            try
            {
                process.Start();
                TStdOut = new Thread(() => { StdOut = process.StandardOutput.ReadToEnd(); });
                TStdOut.Start();
                if (UseStdErr)
                {
                    TStdErr = new Thread(() => { StdErr = process.StandardError.ReadToEnd(); });
                    TStdErr.Start();
                }

                if (Timeout > 0)
                {
                    Exited = process.WaitForExit(Timeout);
                    // TODO kill the process on timeout ? prc.Kill();
                }
                else
                {
                    process.WaitForExit();
                    ExitCode = process.ExitCode;
                    Exited = true;
                }
                TStdOut.Join();
                if (UseStdErr)
                    TStdErr.Join();
            }
            catch(Exception e)
            {
                StdOut = e.Message;
            }
            finally
            {
                ProcessExited(null, null);
                process.Close();
            }
            return Exited;
        }
    } // classs
} // namespace
