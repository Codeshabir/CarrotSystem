using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class EmailGroup
    {
        public int Id { get; set; }
        public string? GroupName { get; set; }
        public string? EmailAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
