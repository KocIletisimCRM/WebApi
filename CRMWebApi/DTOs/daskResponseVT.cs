using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMWebApi.DTOs
{
    public class daskResponseVT
    {
        public int? value { get; set; }
        public string text { get; set; }
    }
    public class daskResponseJSON
    {
        private List<daskResponseVT> _yt = new List<daskResponseVT>();
        public List<daskResponseVT> yt { get { return _yt; } set { _yt.Clear(); yt.AddRange(value); } }
    }
}
