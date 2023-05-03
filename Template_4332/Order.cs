using System;

namespace Template_4332
{
    public class Order
    {
        public int Id { get; set; }
        public string CodeOrder { get; set; }
        public DateTime CreateDate { get; set; }
        public TimeSpan CreateTime { get; set; }
        public string CodeClient { get; set; }
        public string Services { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }
        public TimeSpan ProkatTime { get; set; }
    }
}
