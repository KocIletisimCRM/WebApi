using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Teknar_Proxy_Lib
{
    public class TeknarProxy
    {
        internal static long InitialPheromoneValue = 1000;
        internal static double PunishmentRate = .5;
        internal static double ConfermentRate = 2;

        private readonly object _threadLock = new object();

        public string ip { get; set; }
        public ushort port { get; set; }
        public string country { get; set; }
        public protocol type { get; set; }
        public anonymity_level security { get; set; }
        internal string _key
        {
            get
            {
                return string.Format("{0}:{1}", ip, port);
            }
        }
        internal double _pheromone = InitialPheromoneValue;
        public double pheromone { get { return _pheromone; } }
        internal void updatePheromone(long bytesRecieved, long timeElapsed)
        {
            lock (_threadLock)
            {
                _totalBytes += bytesRecieved;
                _totalTime += timeElapsed;
                if (bytesRecieved == 0)
                {
                    _pheromone = Convert.ToInt64(_pheromone * PunishmentRate);
                }
                else
                {
                    var rate = ConfermentRate * (4.0 * _totalBytes / 1024) / (_totalTime / 1000.0);
                    _pheromone = Convert.ToInt64(Math.Max(Math.Min(rate * InitialPheromoneValue, long.MaxValue), 0));
                }
                decreaseCount();
            }
        }

        internal long _totalBytes = 0;
        public long totalBytes { get { return Interlocked.Read(ref _totalBytes) / 1024; } }

        internal long _totalTime = 0;
        public long totalTime { get { return Interlocked.Read(ref _totalTime) / 1000; } }

        public string speed { get { return ((4.0 * _totalBytes / 1024) / (_totalTime / 1000.0)).ToString("N2") + " Kbps"; } }

        internal long _count = 0;
        public long count { get { return Interlocked.Read(ref _count); } }
        internal void increaseCount()
        {
            Interlocked.Increment(ref _count);
            TeknarProxyService.doOnProxyListChange(this);
        }
        internal void decreaseCount()
        {
            Interlocked.Decrement(ref _count);
            TeknarProxyService.doOnProxyListChange(this);
        }

        public TeknarProxy Clone()
        {
            return new TeknarProxy
            {
                _count = _count,
                _totalBytes = _totalBytes,
                _totalTime = _totalTime,
                country = country,
                ip = ip,
                port = port,
                security = security,
                type = type,
                _pheromone = _pheromone
            };
        }
    }
}
