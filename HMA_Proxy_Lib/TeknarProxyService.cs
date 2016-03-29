using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace Teknar_Proxy_Lib
{
    public partial class TeknarProxyService
    {
        #region Private
        private delegate Task sourceLoader();
        private static Dictionary<proxySource, sourceLoader> loaders = new Dictionary<proxySource, sourceLoader>();
        private static ConcurrentDictionary<string, TeknarProxy> proxyList = new ConcurrentDictionary<string, TeknarProxy>();
        private static System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(3).TotalMilliseconds);
        private static bool isStarted = false;
        private static int maxPageToParse = 1;
        private static TimeSpan requestTimeout = TimeSpan.FromSeconds(30);
        public static int MaxTryCount = 3;
        private static string CacheDir = Path.GetTempPath();
        private static proxySource source = proxySource.TPShma;
        private static SemaphoreSlim ssl = new SemaphoreSlim(10);
        private static SemaphoreSlim cacheFileWriter = new SemaphoreSlim(1);
        private static SynchronizationContext syncContext;
        private static string cacheFileName = "iabi_cache.ch";
        #endregion

        #region constructur destructor
        static TeknarProxyService()
        {
            // TODO: Burası çalışıyormu kontrol edilecek
            loaders.Add(proxySource.TPSfpln, getgetProxiesFromFPLN);
            loaders.Add(proxySource.TPShma, getProxiesFromHMA);
            syncContext = SynchronizationContext.Current;
        }
        ~TeknarProxyService()
        {
            proxyList.Clear();
            loaders.Clear();
            timer.Dispose();
        }
        #endregion

        #region Methods
        public static async Task<TeknarProxy> selectProxy()
        {
            try
            {
                for (; ; )
                {
                    if (proxyList.Count > 0) break;
                    await Task.Delay(100).ConfigureAwait(false);
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<TeknarProxy> list = proxyList.Values.Select(p => p.Clone()).ToList();
                double cumulativePheromone = 0;
                double totalPheromone = list.Sum(p => p._pheromone);
                list = new List<TeknarProxy>(list.OrderBy(p => p._pheromone).Select(p =>
                {
                    cumulativePheromone += (p._pheromone / totalPheromone);
                    p._pheromone = cumulativePheromone;
                    return p;
                }));
                sw.Stop();
                var seed = sw.ElapsedTicks;
                Random rnd = new Random((int)seed);
                var selected = rnd.NextDouble();
                var proxy = list.Where(p => p._pheromone >= selected).First();
                return proxy;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("selectProxy: {0}", e.Message));
            }
        }
        public static async Task<Dictionary<string, TeknarProxy>> selectDistinctProxies(byte count)
        {
            Dictionary<string, TeknarProxy> result = new Dictionary<string, TeknarProxy>();
            count = (byte)Math.Min(count, proxyList.Count);
            while (result.Count < count)
            {
                var proxy = await selectProxy().ConfigureAwait(false);
                result[proxy._key] = proxy;
            }
            return result;
        }

        private static string getSafeFleNameFromUrl(string url)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(url.Select(c => invalidChars.Contains(c) ? "_" : c.ToString()));
        }
        private async static Task cacheResult(string url, string content)
        {
            try
            {
                await cacheFileWriter.WaitAsync().ConfigureAwait(false);
                using (ZipArchive cacheZip = ZipFile.Open(Path.Combine(CacheDir, "iabi_cache.ch"), ZipArchiveMode.Update))
                {
                    var entry = cacheZip.CreateEntry(getSafeFleNameFromUrl(url));
                    using (var writer = new StreamWriter(entry.Open()))
                    {
                        await writer.WriteAsync(content).ConfigureAwait(false);
                    }
                }
                cacheFileWriter.Release();
            }
            catch (Exception ex)
            {
                cacheFileWriter.Release();
                throw new Exception(string.Format("cacheResult: {0}", ex.Message));
            }
        }
        public static void SetCacheDir(string path)
        {
            string cacheFileFullName = Path.Combine(path, cacheFileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //if (!File.Exists(cacheFileFullName))
            //    using (ZipArchive za = ZipFile.Open(Path.Combine(path, cacheFileName), ZipArchiveMode.Create)) ;
            CacheDir = path;
        }

        public static async Task<TeknarProxyResult<string>> getJSONString(string url, CancellationToken ct, string uploadData = null)
        {
            await ssl.WaitAsync().ConfigureAwait(false);
            TeknarProxyResult<string> result = new TeknarProxyResult<string>
            {
                Url = url
            };
            try
            {
                TeknarProxy proxy;
                if (!proxyList.TryGetValue((await selectProxy())._key, out proxy))
                    ct.ThrowIfCancellationRequested();
                result.Proxy = proxy;
                proxy.increaseCount();
                Stopwatch stw = new Stopwatch();
                using (WebClient wbc = new WebClient())
                {
                    wbc.Encoding = Encoding.UTF8;
                    wbc.Proxy = new WebProxy(proxy.ip, proxy.port);
                    try
                    {
                        stw.Start();
                        ct.Register(wbc.CancelAsync);
                        if (string.IsNullOrWhiteSpace(uploadData))
                            result.stringResult = await wbc.DownloadStringTaskAsync(url).ConfigureAwait(false);
                        else {
                            wbc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            result.stringResult = await wbc.UploadStringTaskAsync(new Uri(url), "POST", uploadData).ConfigureAwait(false);
                        }
                            if (ct.IsCancellationRequested && result.stringResult == null)
                            ct.ThrowIfCancellationRequested();
                        stw.Stop();
                        proxy.updatePheromone((result.stringResult ?? string.Empty).Length, (long)stw.Elapsed.TotalMilliseconds);
                        if (string.IsNullOrWhiteSpace(result.stringResult)) throw new Exception("Hiçbir sunuç dönmedi!");
                    }
                    catch (Exception e)
                    {
                        stw.Stop();
                        proxy.updatePheromone((result.stringResult ?? string.Empty).Length, (long)stw.Elapsed.TotalMilliseconds);
                        result.Error = e;
                    }
                    //if (result.stringResult != null)
                    //{
                    //    try
                    //    {
                    //        await cacheResult(url, result.stringResult).ConfigureAwait(false);
                    //    }
                    //    finally
                    //    {
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }
            ssl.Release();
            return result;
        }
        public static async Task<TeknarProxyResult<T>> DeserializeJSON<T>(string url, string uploadData = null, bool useProxyCache = true)
        {
            try
            {
                if (!isStarted) Start();
                #region Use Proxy Cache
                //if (useProxyCache)
                //{
                //    var cacheFileFullPath = Path.Combine(CacheDir, cacheFileName);
                //    var fileName = getSafeFleNameFromUrl(url);
                //    using (ZipArchive cacheZip = ZipFile.OpenRead(cacheFileFullPath))
                //    {
                //        var entry = cacheZip.GetEntry(fileName);
                //        if (entry != null)
                //        {
                //            try
                //            {
                //                using (var reader = new StreamReader(entry.Open()))
                //                {
                //                    return new TeknarProxyResult<T>
                //                    {
                //                        Url = url,
                //                        stringResult = "Proxy Cache Used",
                //                        Result = JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync().ConfigureAwait(false))
                //                    };
                //                }

                //            }
                //            finally
                //            {
                //            }
                //        }
                //    }
                //}
                #endregion
                int tryCount = 0;
                TeknarProxyResult<string> result = new TeknarProxyResult<string>
                {
                    Error = new Exception()
                };
                while (MaxTryCount > tryCount++ && result.hasException() && !result.isNotFoundError())
                {
                    result = await getJSONString(url, (new CancellationTokenSource(requestTimeout)).Token, uploadData).ConfigureAwait(false);
                }
                return new TeknarProxyResult<T>
                {
                    Error = (tryCount > MaxTryCount) ?
                    new TeknarMultipleConnectionError(
                        string.Format("Maksimum deneme sayısı [{0}] aşıldı.", MaxTryCount),
                        new TeknarProxyResult<string>[] { result }) :
                    result.Error,
                    Proxy = result.Proxy,
                    Url = url,
                    stringResult = result.stringResult,
                    Result = (tryCount > MaxTryCount) || result.Error != null ? default(T) : JsonConvert.DeserializeObject<T>(result.stringResult)
                };
            }
            catch (Exception ex)
            {
                return new TeknarProxyResult<T>
                {
                    Url = url,
                    Error = ex
                };
            }
        }
        private static void getProxiesFromSource()
        {
            loaders[source]();
        }
        public static List<TeknarProxy> ProxyList
        {
            get
            {
                if (!isStarted) throw new Exception("Servis başlatılmadı...");
                return proxyList.Values.ToList();
            }
        }
        public static string AverageSpeed
        {
            get
            {
                var totalkb = 4.0 * proxyList.Values.Sum(p => p._totalBytes) / 1024;
                var totalsn = proxyList.Values.Sum(p => p._totalTime) / 1000.0;
                return (totalkb / totalsn).ToString("N2") + " Kbps";
            }
        }
        public static long ActiveConnectionCount { get { return proxyList.Values.Sum(p => p.count); } }
        public static void Start()
        {
            if (!timer.Enabled)
            {
                isStarted = true;
                getProxiesFromSource();
                timer.Elapsed += (object sender, ElapsedEventArgs evtArgs) =>
                {
                    getProxiesFromSource();
                };
                timer.AutoReset = true;
                timer.Start();
            }
        }
        public static void Stop()
        {
            if (!isStarted) throw new Exception("Servis başlatılmadı...");
            timer.Stop();
            timer.Enabled = false;
            isStarted = false;
        }
        #endregion

        #region events
        public static EventHandler<TeknarProxy> onProxyListChange;
        internal static void doOnProxyListChange(TeknarProxy proxy)
        {
            if (onProxyListChange == null) return;
            var ilist = onProxyListChange.GetInvocationList();
            foreach (EventHandler<TeknarProxy> handler in ilist)
            {
                syncContext.Post(new SendOrPostCallback((o) => { handler(handler.Target, proxy); }), null);
            }
        }
        #endregion
    }
}