using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class RateShort
    {
        public RateShort() { }

        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public float Cur_OfficialRate { get; set; }
    }
}
