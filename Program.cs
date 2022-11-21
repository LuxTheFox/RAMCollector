using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;

namespace RAMCollector
{
    class Program
    {

        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static int GetProcess(string processName)
        {
            int ProcessID = -1;
            foreach (Process Process in Process.GetProcessesByName(processName))
            {
                if (Process.WorkingSet64 > 0)
                {
                    ProcessID = Process.Id;
                }
            }
            return ProcessID;
        }

        public static void HandleProcesses(string ConfPath)
        {
            List<string> processes = System.IO.File.ReadAllLines(ConfPath).ToList();
            processes.ForEach(ProcessName =>
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    while (GetProcess(ProcessName) != -1)
                    {
                        if (GetProcess(ProcessName) != -1)
                        {
                            GC.Collect();

                            GC.WaitForPendingFinalizers();

                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                SetProcessWorkingSetSize(Process.GetProcessById(GetProcess(ProcessName)).Handle, -1, -1);
                            }

                            Thread.Sleep(1);
                        }
                    }
                }).Start();
                Console.WriteLine("Managing RAM Usage for " + ProcessName);
            });
        }

        static void Main(string[] args)
        {
            Console.Title = "RAM Collector By Lux";
            string ConfPath = System.IO.Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).ToString() + "\\RAMCollector.conf";
            if (!System.IO.File.Exists(ConfPath))
            {
                System.IO.File.WriteAllLines(ConfPath, new String[] { "discord", "chrome", "obs64" });
                Console.WriteLine("Created a config file with default executables.");
                Console.ReadLine();
                Environment.Exit(0);
            }
            HandleProcesses(ConfPath);
            Console.WriteLine("Press enter to hide window");
            Console.ReadLine();
            ShowWindow(GetConsoleWindow(), 0);
            Console.ReadLine();
        }
    }
}
