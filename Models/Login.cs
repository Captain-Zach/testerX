using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace auction.Models
{
    public class Login
    {
        [Required]
        public string UserName {get;set;}
        
        [Required]
        public string Password {get;set;}
    }
}