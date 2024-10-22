    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UberSytem.Dto.Responses
{
    public class CustomerDTO
    {
        public long Id { get; set; }

        public string Role { get; set; }

        public string? UserName { get; set; }

        public string Email { get; set; } = null!;

        public string? Password { get; set; }
        public bool? IsEmailConfirmed { get; set; }
    }
}
