using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSystem.Domain.Models
{
    public class BookingRequest
    {
        public long customerId { get; set; }
        public float sourceLatitude { get; set; }
        public float sourceLongitude { get; set; }
        public float destinationLatitude { get; set; }
        public float destinationLongitude { get; set; }
    }
}
