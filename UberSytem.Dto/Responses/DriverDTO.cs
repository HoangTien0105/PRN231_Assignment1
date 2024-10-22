using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSytem.Dto.Responses
{
    public class DriverDTO
    {
        public long Id { get; set; }

        public long? CabId { get; set; }

        public DateTime? Dob { get; set; }

        public double? LocationLatitude { get; set; }

        public double? LocationLongitude { get; set; }

        public byte[] CreateAt { get; set; } = null!;

        public long? UserId { get; set; }
    }
}
