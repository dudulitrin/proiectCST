using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace backend.Payloads
{
    public class LoginPayload
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
