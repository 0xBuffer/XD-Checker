using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace XDCheckerRecode
{
    public static class Telemetry
    {
        // System Info Collector
        public static async Task<Dictionary<string, string>> CollectSystemInfo()
        {
            var r = new Dictionary<string, string>();
            r["Username"] = Environment.UserName;
            r["MachineName"] = Environment.MachineName;
            r["OS"] = Environment.OSVersion.ToString();

            // CPU
            try
            {
                using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        r["CPU"] = item["Name"].ToString();
                        break;
                    }
                }
            }
            catch { r["CPU"] = "Unknown"; }

            // RAM
            try
            {
                using (var searcher = new ManagementObjectSearcher("select Capacity from Win32_PhysicalMemory"))
                {
                    long t = 0;
                    foreach (var item in searcher.Get()) t += Convert.ToInt64(item["Capacity"]);
                    r["RAM"] = $"{(t / 1024 / 1024 / 1024)} GB";
                }
            }
            catch { r["RAM"] = "Unknown"; }

            // GPU
            try
            {
                using (var searcher = new ManagementObjectSearcher("select Name, AdapterRAM from Win32_VideoController"))
                {
                    foreach (var i in searcher.Get())
                    {
                        r["GPU"] = i["Name"].ToString();
                        try
                        {
                            long v = Convert.ToInt64(i["AdapterRAM"]);
                            r["GPU_RAM"] = $"{(v / 1024 / 1024)} MB";
                        }
                        catch { r["GPU_RAM"] = "Unknown"; }
                        break;
                    }
                }
            }
            catch { r["GPU"] = "Unknown"; }

            // IP collection removed from client side, will be handled by the server (Cloudflare Worker)
            return r;
        }

        public static async Task SendReport(Dictionary<string, string> data)
        {
            try 
            {
                string url = "https://apixd.zxcswamper.workers.dev/report"; 
                
                // Manual JSON serialization to avoid dependency on System.Text.Json (missing in .NET 4.8 default)
                var sb = new StringBuilder();
                sb.Append("{");
                if (data != null)
                {
                    bool first = true;
                    foreach (var kvp in data)
                    {
                        if (!first) sb.Append(",");
                        first = false;
                        
                        string key = kvp.Key;
                        // Basic escaping for JSON values
                        string val = (kvp.Value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                        sb.Append($"\"{key}\":\"{val}\"");
                    }
                }
                sb.Append("}");
                string json = sb.ToString();

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "XDChecker-Client/1.0");
                    await client.PostAsync(url, content);
                }
            }
            catch { /* Logging could be added here */ }
        }
    }
}
