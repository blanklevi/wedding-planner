using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set; }

        [Required(ErrorMessage = "is required")]
        [MinLength(2, ErrorMessage = "must be at least 2 characters")]
        [Display(Name = "Wedder One")]
        public string WedderOne { get; set; }

        [Required(ErrorMessage = "is required")]
        [MinLength(2, ErrorMessage = "must be at least 2 characters")]
        [Display(Name = "Wedder Two")]
        public string WedderTwo { get; set; }

        [Required(ErrorMessage = "is required")]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "is required")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int Creator { get; set; }

        public List<Rsvp> Users { get; set; }

        //methods
        public int TotalGuests()
        {
            if (Users.Count == 0)
            {
                return 0;
            }
            var total = Users.Count;
            return total;
        }

    }
}