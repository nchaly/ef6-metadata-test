using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Diagnostics.Runtime;
using RestSharp.Extensions;
using WebApplication2.Code;

namespace controller
{
    class Program
    {
        private static int _pid;
        private static ClrRuntime _runtime;
        private static DataTarget _target;
        private static ApiClient _client;

        static void Main(string[] args)
        {
            var url = "http://localhost:61075";
            _client = new ApiClient(url, "ctl", new RestSharpJsonNetSerializer());
            if (!Connect(_client))
            {
                Thread.Sleep(3000);
                if (!Connect(_client))
                {
                    return;
                }
            }

            try
            {
                Console.WriteLine("Trying to attach to the target process...");
                Attach(_pid);
                PrintTargetData();
                FetchGCStats("Warmup");
                Detach();
                Console.WriteLine($"Detached from process {_pid}.");

                InitDb();

                ReplLoop();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static Dictionary<string, Action> _actions = new Dictionary<string, Action>()
        {
            {"h", PrintHelp},
            {"q", ActionQuit},
            {"o", ActionQueryOrders},
            {"e", ActionExportStats},
            {"p", ActionQueryProducts},
            {"gc", ActionQueryGC},
            
        };

        [ActionDesc(Description = "Quit controller.")]
        private static void ActionQuit()
        {
            throw new NotImplementedException();
        }

        [ActionDesc(Description = "Query top 100 orders with lines")]
        private static void ActionQueryOrders()
        {
            Console.WriteLine("Query orders");
            var n = _client.Exec<int>("db/query-top/Order").Result;
            Console.WriteLine($"{n} orders in result set.");
        }

        [ActionDesc(Description = "Query products")]
        private static void ActionQueryProducts()
        {
            Console.WriteLine("Query top 100 products.");
            var n = _client.Exec<int>("db/query-top/Product").Result;
            Console.WriteLine($"{n} products in result set.");
        }


        [ActionDesc(Description = "Query GC")]
        private static void ActionQueryGC()
        {
        }

        [ActionDesc(Description = "Export stats")]
        private static void ActionExportStats()
        {
            using (var file = File.OpenWrite($"stats{DateTime.Now:s}.csv".Replace(":", "")))
            using (var w = new StreamWriter(file))
            {
                var sb = new StringBuilder("time,command");
                foreach (var f in _filters)
                {
                    sb.Append($",{f} Count, {f} Total Size");
                }
                w.WriteLine(sb.ToString());
                sb.Length = 0;

                foreach (var data in _clrData)
                {
                    sb.Append($"{data.Time:s},{data.Cmd,50}");
                    foreach (var stat in data.Stats)
                    {
                        sb.Append($",{stat.TotalCount,10},{stat.TotalSize,10}");
                    }
                    w.WriteLine(sb.ToString());
                    sb.Length = 0;
                }
            }
        }

        private static void ReplLoop()
        {

            PrintHelp();
            while (true)
            {
                Console.Write(">");
                var input = Console.ReadLine();
                if (input == null)
                {
                    break;
                }
                if (_actions.ContainsKey(input))
                {
                    var action = _actions[input];
                    if (action == ActionQuit)
                    {
                        Console.WriteLine("Bye-bye.");
                        return;
                    }

                    ExecAction(action);
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
        }

        private static void ExecAction(Action action)
        {
            try
            {

                var d = "";
                var at = action.Method.GetAttribute<ActionDescAttribute>();
                if (at != null)
                {
                    d = at.Description;
                }

                Console.WriteLine($"Start execute action {action}:\t{d}");

                action.Invoke();


                try
                {
                    Attach(_pid);
                    FetchGCStats(d);
                }
                finally
                {
                    Detach();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        class ClrData
        {
            public string Cmd;
            public DateTime Time;
            public List<ClrTypeData> Types = new List<ClrTypeData>();
            public List<FilterData> Stats = new List<FilterData>();
        }

        class ClrTypeData
        {
            public string Name;
            public ulong Size;
        }
        class FilterData
        {
            public string Filter;
            public ulong TotalSize;
            public int TotalCount;
        }

        static List<ClrData> _clrData = new List<ClrData>();
        private static string[] _filters = new []
        {
          //  "System.Data.Entity.Core.Metadata",
            "System.Data.Entity.Core.Metadata.Edm.TypeUsage",
            "WebApplication2.Code.Order",
           // "System.Data.Entity.Core.Metadata.Edm.MetadataProperty"
        };

        private static void FetchGCStats(string cmd)
        {
            Console.WriteLine("Fething GC stats...");
            var heap = _runtime.GetHeap();

            var data = new ClrData
            {
                Cmd = cmd,
                Time = DateTime.Now,
            };
            
            if (!heap.CanWalkHeap)
            {
                Console.WriteLine("Cannot walk the heap!");
            }
            else
            {
                foreach (ulong obj in heap.EnumerateObjectAddresses())
                {
                    var type = heap.GetObjectType(obj);

                    // If heap corruption, continue past this object.
                    if (type == null)
                        continue;

                    if (type.Name.StartsWith("System.Data.Entity") ||
                        type.Name.StartsWith("WebApplication2"))//.Core.Metadata.Edm"))
                    {
                        data.Types.Add(new ClrTypeData { Name = type.Name, Size = type.GetSize(obj)});
                    }
                }

                Console.WriteLine($"Stats for System.Data.Entity object. Total {data.Types.Count}");
                foreach (var f in _filters)
                {
                    ulong totalSize = 0;
                    int totalCount = 0;
                    var fd = new FilterData {Filter = f};
                    foreach (var type in data.Types)
                    {
                        if (type.Name.StartsWith(f))
                        {
                            totalSize += type.Size;
                            totalCount += 1;
                        }
                    }
                    fd.TotalCount = totalCount;
                    fd.TotalSize = totalSize;
                    data.Stats.Add(fd);

                    Console.WriteLine($"{fd.Filter}:\n\t Count:     {totalCount};\n\t Total size: {totalSize}\n");
                }
            }

            _clrData.Add(data);

            Console.WriteLine("--- End of GC stats.");
        }

        [ActionDesc(Description = "Print help")]
        private static void PrintHelp()
        {
            Console.WriteLine("** Choose an action:");
            foreach (var a in _actions)
            {
                var d = "";
                var at = a.Value.Method.GetAttribute<ActionDescAttribute>();
                if (at != null)
                {
                    d = at.Description;
                }
                Console.WriteLine($"{a.Key}\t{d}");
            }
        }

        private static void InitDb()
        {
            Console.WriteLine("Initializing database...");
            var s = _client.Exec<string>("db/startInit").Result;
            Console.WriteLine(s);

            dynamic status = _client.Exec<dynamic>("db/status").Result;
            if (status.Status != State.Completed)
            {
                while (true)
                {
                    Console.WriteLine($"Database: {status.Status}. Retry");
                    Thread.Sleep(1500);
                    status = _client.Exec<dynamic>("db/status").Result;
                    if (status.Status == State.Completed)
                    {
                        break;
                    }
                }
            }
            Console.WriteLine($"Database: {status.Status}. Done");
        }

        private static bool Connect(ApiClient client)
        {
            try
            {
                dynamic info = client.Exec<dynamic>("info").Result;
                if (info.id <= 0)
                {
                    throw new Exception("Invalid target pid");
                }
                Console.WriteLine("Target process: {0}/{1}", info.Id, info.ProcessName);
                _pid = info.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot find target webapi application at url {client.BaseUrl}. Exiting.");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        static void Detach()
        {
            if (_target != null)
            {
                _target.DebuggerInterface.DetachProcesses();
                _target.Dispose();
                _target = null;
                _runtime = null;
            }
        }
        private static void Attach(int pid)
        {
            if (_target != null)
            {
                throw new Exception("Already attached");
            }

            _target = DataTarget.AttachToProcess(pid, 10000, AttachFlag.NonInvasive);
            if (true) //target.ClrVersions.Count == 1)
            {
                ClrInfo runtimeInfo = _target.ClrVersions[0]; // just using the first runtime
                _runtime = runtimeInfo.CreateRuntime();
            }
        }

        private static void PrintTargetData()
        {
            foreach (var version in _target.ClrVersions)
            {
                Console.WriteLine("Found CLR Version:" + version.Version);
                ModuleInfo dacInfo = version.DacInfo;
                Console.WriteLine("Filesize:  {0:X}", dacInfo.FileSize);
                Console.WriteLine("Timestamp: {0:X}", dacInfo.TimeStamp);
                Console.WriteLine("Dac File:  {0}", dacInfo.FileName);

                string dacLocation = version.DacInfo.FileName;
                if (!string.IsNullOrEmpty(dacLocation))
                    Console.WriteLine("Local dac location: " + dacLocation);
            }

        }
    }
}
