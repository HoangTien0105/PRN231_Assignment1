using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Enums;

namespace UberSytem.Dto.Responses
{
    public class TripDTO
    {
        public long Id { get; set; }

        public long? CustomerId { get; set; }

        public long? DriverId { get; set; }

        public long? PaymentId { get; set; }

        public TripStatus Status { get; set; }

        public double? SourceLatitude { get; set; }

        public double? SourceLongitude { get; set; }

        public double? DestinationLatitude { get; set; }

        public double? DestinationLongitude { get; set; }

        public byte[] CreateAt { get; set; } = null!;
    }
}
