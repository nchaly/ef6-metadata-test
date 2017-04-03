using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApplication2.Code
{
    [RoutePrefix("ctl")]
    public class CtlController : ApiController
    {
        private static State _dbInitStatus = State.NonStarted;
        private string _dbInitMessage;
        private object _sync = new object();

        [HttpGet, Route("info")]
        public object Get()
        {
            var p = Process.GetCurrentProcess();
            return new
            {
                p.Id,
                p.ProcessName,
                p.MachineName,
                p.StartTime,
                //Modules = p.Modules.Cast<ProcessModule>().Select(x => new {x.FileName} ),
                //Threads = p.Threads.Cast<ProcessThread>().Select(x => new { x.Id, x.StartTime})
            };
        }


        [HttpGet, Route("db/query-top/Order")]
        public int QueryTop100Orders()
        {

            using (var ctx = new Ctx())
            {
                var q = ctx.Orders.Include("Lines").Take(100);
                var r = q.ToArray();
                return r.Length;
            }
        }


        [HttpGet, Route("db/query-top/Product")]
        public int QueryTop100Product()
        {

            using (var ctx = new Ctx())
            {
                var q = ctx.Products.Include("Category").Take(100);
                var r = q.ToArray();
                return r.Length;
            }
        }

        [HttpGet, Route("db/status")]
        public object DbStatus()
        {
            lock (_sync)
            {
                if (_dbInitStatus != State.Completed)
                {
                    int percent;
                    string msg;

                    Ctx.Initializer.GetProgress(out percent, out msg);

                    return new
                    {
                        Status = _dbInitStatus,
                        Message = _dbInitMessage,
                        Message2 = msg,
                        Percent = percent
                    };
                }
                else
                {
                    using (var ctx = new Ctx())
                    {
                        return new
                        {
                            Status = _dbInitStatus,
                            CategoriesCount = ctx.Categories.Count()
                        };
                    }
                }
            }
        }

        [HttpGet, Route("db/startInit")]
        public string StartInitDb()
        {
            lock (_sync)
            {
                if (_dbInitStatus != State.InProgress)
                {
                    _dbInitStatus = State.InProgress;
                    var noMigrate = ConfigurationManager.AppSettings["db.noupdate"] == "true";
                    Task.Run(() => Ctx.Initializer.InitializeDb(noMigrate)).ContinueWith((t) =>
                    {
                        lock (_sync)
                        {
                            if (t.Exception == null && t.IsCompleted)
                            {
                                _dbInitStatus = State.Completed;
                            }
                            else if (t.Exception != null || t.IsCanceled || t.IsFaulted)
                            {
                                _dbInitStatus = State.Failed;
                                _dbInitMessage =
                                    $"Exception: {t.Exception}; Cancelled: {t.IsCanceled}; Failed: {t.IsFaulted}";
                            }
                        }
                    });
                    return "Started db initialization";
                }
                else
                {
                    return "Already in progress";
                }
            }
        }
    }
}
