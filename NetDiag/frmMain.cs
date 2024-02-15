using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;

namespace NetDiag
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        readonly string[] domains = new string[] { "google.com", "bing.com", "duckduckgo.com", "youtube.com" };
        private void killmy_self()
        {
            Thread.Sleep(1000);
            Ping pingReq = new Ping();

            List<long> latencies = new List<long>();
            int lostPackets = 0;
            progressBar1.Invoke(new MethodInvoker(() => { progressBar1.Maximum = domains.Length * 4; }));

            for (int i = 0; i < domains.Length; ++i)
                for (int r = 0; r < 4; ++r)
                {
                    try
                    {
                        PingReply reply = pingReq.Send(domains[i]);
                        if (reply.Status != IPStatus.Success)
                            ++lostPackets;
                        else
                            latencies.Add(reply.RoundtripTime);
                    }
                    catch
                    {
                        ++lostPackets;
                    }

                    progressBar1.Invoke(new MethodInvoker(() => { ++progressBar1.Value; }));
                    Thread.Sleep(1000);
                }

            Invoke(new MethodInvoker(Hide));
            string networkCondition = "Good";
            double lossPercentage = ((double)lostPackets / (double)(latencies.Count + lostPackets)) * 100d;
            if (lossPercentage > 0d)
                networkCondition = "Poor";
            else if (lossPercentage > 49d)
                networkCondition = "Horrible";

            if (latencies.Count < 1)
            {
                MessageBox.Show("The diagnostic has completed. There is no internet connection.", "NetDiag",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Invoke(new MethodInvoker(Close));
            }
            else
            {
                double averageLatency = 0d;
                foreach (double dubble in latencies)
                    averageLatency += dubble;

                averageLatency = averageLatency / latencies.Count;
                if (networkCondition == "Good")
                {
                    if (averageLatency < 25d)
                        networkCondition = "Excellent";
                    else if (averageLatency > 45d)
                        networkCondition = "Decent";
                    else if (averageLatency > 70d)
                        networkCondition = "Poor";
                    else if (averageLatency > 120d)
                        networkCondition = "Horrible";
                }

                MessageBox.Show("The diagnostic has completed. The results are as follows:\n\n" +
                    $"Average Latency: {averageLatency}ms.\n" +
                    $"Loss Percentage: {lossPercentage}%.\n" +
                    $"Network Condition: {networkCondition}.", "NetDiag", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Invoke(new MethodInvoker(Close));
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            new Thread(killmy_self).Start();
        }
    }
}
