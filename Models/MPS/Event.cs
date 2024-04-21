using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class Event
    {
        public int EventId { get; set; }
        public string? EventBy { get; set; }
        public string? EventType { get; set; }
        public string? ActionType { get; set; }
        public string? EventDesc { get; set; }
        public DateTime? EventDate { get; set; }
    }
}
