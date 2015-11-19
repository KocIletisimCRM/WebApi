using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Teknar_Proxy_Lib
{
    public partial class TeknarProxyService
    {
        private static string FPLN_Url = "http://free-proxy-list.net/";
        private static void FPLN_ParseProxies(string html)
        {
            var trs = html.Replace("\r", string.Empty).Replace("\n", string.Empty).
                Split(new string[] { "table" }, StringSplitOptions.RemoveEmptyEntries)[6].
                Split(new string[] { "<tr>", "</tr>" }, StringSplitOptions.RemoveEmptyEntries).Where(item => item.StartsWith("<td>")).ToList();
            foreach (var tr in trs)
            {
                try
                {
                    var tds = tr.Split(new string[] { "<td>", "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                    var newProxy = new TeknarProxy
                    {
                        country = tds[3].Trim(),
                        ip = tds[0].Trim(),
                        port = ushort.Parse(tds[1].Trim()),
                        security = (anonymity_level)Enum.Parse(typeof(anonymity_level), tds[4].Replace(" ", string.Empty).Trim().ToUpperInvariant()),
                        type = (protocol)Enum.Parse(typeof(protocol), tds[6].Replace(" ", string.Empty).Trim().ToUpperInvariant()),
                    };
                    if (newProxy.security < anonymity_level.HIGH) continue;
                    var key = string.Format("{0}:{1}", newProxy.ip, newProxy.port);
                    TeknarProxy proxy;
                    if (proxyList.TryGetValue(key, out proxy))
                    {
                        proxy._pheromone = TeknarProxy.InitialPheromoneValue;
                        proxy.security = newProxy.security;
                        proxy.type = newProxy.type;
                        proxy.country = newProxy.country;
                    }
                    else
                    {
                        proxyList[key] = newProxy;
                    }
                    doOnProxyListChange(proxyList[key]);
                }
                catch (Exception)
                {
                }
            }
        }
        private static async Task getgetProxiesFromFPLN()
        {
            using (WebClient wbc = new WebClient())
            {
                try
                {
                    var s = await wbc.DownloadStringTaskAsync(FPLN_Url).ConfigureAwait(false);
                    FPLN_ParseProxies(s);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
