using mshtml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teknar_Proxy_Lib.HMAClasses;

namespace Teknar_Proxy_Lib
{
    public partial class TeknarProxyService
    {
        private static string HMA_Url = "http://proxylist.hidemyass.com";
        private static void HMA_ParseProxies(string table)
        {
            try
            {
                IHTMLDocument2 doc = (IHTMLDocument2)new HTMLDocument();
                doc.write(string.Format("<table>{0}</table>", table.Replace("\r\n", string.Empty)));
                IHTMLElementCollection rows = ((IHTMLDocument3)doc).getElementsByTagName("tr");
                for (int i = 0; i < rows.length; i++)
                {
                    try
                    {
                        HTMLTableRow row = rows.item(i);
                        var type = (protocol)Enum.Parse(typeof(protocol), rows.item(i).cells.item(6).innerText.Replace("/", string.Empty).ToUpperInvariant());
                        var security = (anonymity_level)Enum.Parse(typeof(anonymity_level), rows.item(i).cells.item(7).innerText.Replace(" +", string.Empty).ToUpperInvariant());
                        if (type == protocol.SOCKS45 || security < anonymity_level.HIGH) continue;

                        var ipChilds = ((IHTMLDOMNode)row.cells.item(1)).firstChild.childNodes;
                        string ipaddress = "";
                        for (int j = 0; j < ipChilds.length; j++)
                        {
                            var element = ipChilds.item(j) as IHTMLDOMNode;
                            string tagname = element.nodeName;
                            if (tagname == "#text") ipaddress += element.nodeValue;
                            else
                            {
                                var element2 = (IHTMLElement2)element;
                                string display = element2.currentStyle.display;
                                if (display != "none") ipaddress += ((IHTMLElement)element).innerText;
                            }
                        }
                        var port = ushort.Parse(((IHTMLElement)row.cells.item(2)).innerText);
                        var key = string.Format("{0}:{1}", ipaddress, port);
                        TeknarProxy newProxy = new TeknarProxy
                        {
                            ip = ipaddress,
                            port = ushort.Parse(((IHTMLElement)row.cells.item(2)).innerText),
                            country = ((IHTMLElement)row.cells.item(3)).innerText,
                            type = type,
                            security = security
                        };
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
            catch (Exception)
            {
            }
        }
        private static async Task HMA_LoadPageAndParseProxies(int pageNo)
        {
            using (WebClient wbc = new WebClient())
            {
                wbc.Encoding = Encoding.UTF8;
                wbc.Headers.Add("X-Requested-With", "XMLHttpRequest");
                try
                {
                    string s = await wbc.DownloadStringTaskAsync(string.Format("{0}/{1}", HMA_Url, pageNo)).ConfigureAwait(false);
                    HMA_ParseProxies(JsonConvert.DeserializeObject<HMA_pageResult>(s).table);
                }
                catch (Exception)
                {

                }
            }
        }
        private static async Task getProxiesFromHMA()
        {
            for (int i = 0; i < maxPageToParse; i++)
            {
                await HMA_LoadPageAndParseProxies(i + 1).ConfigureAwait(false);
            }

        }
    }
}
