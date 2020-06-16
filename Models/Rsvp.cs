using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Rsvp
    {

        [Key]
        public int RsvpId { get; set; }

        public int UserId { get; set; }

        public int WeddingId { get; set; }

        public bool IsRsvp { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }

        public Wedding Wedding { get; set; }

    }
}