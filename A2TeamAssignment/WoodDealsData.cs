using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2TeamAssignment
{
    public class Content
    {
        public string sellerName { get; set; }
        public string sellerInn { get; set; }
        public string buyerName { get; set; }
        public string buyerInn { get; set; }
        public string woodVolumeBuyer { get; set; }
        public string woodVolumeSeller { get; set; }
        public string dealDate { get; set; }
        public string dealNumber { get; set; }
        public string __typename { get; set; }
    }

    public class Data
    {
        public SearchReportWoodDeal searchReportWoodDeal { get; set; }
    }

    public class Root
    {
        public Data data { get; set; }
    }

    public class SearchReportWoodDeal
    {
        public List<Content> content { get; set; }
        public int total { get; set; }
        public int number { get; set; }
        public int size { get; set; }
        public string __typename { get; set; }
    }
}
