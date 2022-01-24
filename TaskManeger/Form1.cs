using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;

namespace TaskManeger
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null;

        private ListViewItemComparer comparer = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void GetProcesses()
        {
            processes.Clear();

            processes = Process.GetProcesses().ToList<Process>();

        }
        private void RefreshProcessList()
        {
            listView2.Items.Clear();

            double memSize = 0;

            foreach (Process p in processes)
            {
                memSize = 0;

                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;

                memSize = (double)pc.NextValue() / (1000 * 1000);

                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };

                listView2.Items.Add(new ListViewItem(row));

                pc.Close();
                pc.Dispose();

            }
            Text = "Всего процессов:" + processes.Count.ToString();
        }
        private void RefreshProcessList(List<Process> processes, string keyword)
        {
            try
            {
                listView2.Items.Clear();

                double memSize = 0;

                foreach (Process p in processes)
                {
                    memSize = 0;

                    PerformanceCounter pc = new PerformanceCounter();
                    pc.CategoryName = "Process";
                    pc.CounterName = "Working Set - Private";
                    pc.InstanceName = p.ProcessName;

                    memSize = (double)pc.NextValue() / (1000 * 1000);

                    string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString() };

                    listView2.Items.Add(new ListViewItem(row));

                    pc.Close();
                    pc.Dispose();

                }
                Text = $"Всего процессов '{keyword}':" + processes.Count.ToString();
            }
            catch (Exception) { }
        }
        
            
            private void KillProcess(Process process)
        {
            process.Kill();

            process.WaitForExit();
        }

        private void KillProcessAndChildren(int pid)
        {
            if (pid == 0)
            {
                return;
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection objectCollection = searcher.Get();

            foreach (ManagementObject obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }

            try
            {
                Process p = Process.GetProcessById(pid);

                p.Kill();

                p.WaitForExit();
            }
            catch (ArgumentException) { }

        }

        private int GetParentProcessId(Process p)
        {
            int parentID = 0;
            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + p.Id + "'");

                managementObject.Get();

                parentID = Convert.ToInt32(managementObject["ParentProcessId"]);
            }
            catch (Exception) { }
            return parentID;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();

            GetProcesses();

            RefreshProcessList();

            comparer = new ListViewItemComparer();
            
            comparer.ColunmIndex = 0;   


        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            GetProcesses();

            RefreshProcessList();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView2.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView2.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcess(processToKill);

                    GetProcesses();

                    RefreshProcessList();
                }
            }
            catch (Exception) { }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView2.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView2.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));

                    GetProcesses();

                    RefreshProcessList();

                }
            }
            catch (Exception) { }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView2.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                    listView2.SelectedItems[0].SubItems[0].Text).ToList()[0];

                    KillProcessAndChildren(GetParentProcessId(processToKill));

                    GetProcesses();

                    RefreshProcessList();

                }
            }
            catch (Exception) { }
        }

        private void toolStripTextBox2_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();

            List<Process> filtredProcesses = processes.Where((x) => x.ProcessName.ToLower().Contains(toolStripTextBox2.Text.ToLower())).ToList<Process>();

            RefreshProcessList(filtredProcesses, toolStripTextBox2.Text);
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColunmIndex = e.Column;

            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            listView2.ListViewItemSorter = comparer;
            listView2.Sort();
        }

        private void ВыходtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
