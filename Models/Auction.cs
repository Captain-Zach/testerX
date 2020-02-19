using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace auction.Models
{
    public class Auction
    {
        [Key]
        public int AuctionId {get;set;}

        [Required]
        public int UserId {get;set;}

        [Required]
        [MinLength(3)]
        public string Name {get;set;}
        
        [Required]
        [MinLength(10)]
        public string Description {get;set;}

        [Required]
        
        public double CurrentBid {get;set;}

        [Required]
        public DateTime EndDate {get;set;}

        [Required]
        public int HighBidderId {get;set;}

        [Required]
        public bool IsCurrent{get;set;}

        public User User{get;set;}
    }
}