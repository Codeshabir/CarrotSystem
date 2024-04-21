using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class EventSetting
    {
        public int Id { get; set; }
        public string? EventType { get; set; }
        public string? ActionType { get; set; }
        public bool? IsShowingLog { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
