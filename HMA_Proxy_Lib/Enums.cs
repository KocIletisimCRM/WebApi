using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknar_Proxy_Lib
{
    public enum protocol { 
        HTTP = 0,
        NO = HTTP,
        HTTPS = 1, 
        YES = HTTPS,
        SOCKS45 = 2
    }
    public enum anonymity_level { 
        NONE = 0,
        TRANSPARENT = NONE,
        LOW = 1, 
        ANONYMOUS = LOW,
        MEDIUM = 2, 
        HIGH = 4, 
        ELITEPROXY = HIGH,
        HIGHKA = 8
    }
    public enum proxySource { TPSfpln, TPShma }
}
