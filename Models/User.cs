using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace auction.Models
{
    public class User
    {
        [Key]
        public int UserId{get;set;}

        [Required]
        [MinLength(3)]
        [MaxLength(20)] 
        public string UserName{get;set;}

        [Required]
        public string FirstName{get;set;}

        [Required]
        public string LastName{get;set;}

        [Required]
        public double Cash{get;set;}

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password{get;set;}

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt{get;set;}

        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt{get;set;}

        [NotMapped]
        [DataType(DataType.Password)]
        public string Confirm{get;set;}

        // public List<Link> Links {get;set;}
    }
}