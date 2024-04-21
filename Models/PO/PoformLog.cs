using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.PO
{
    public partial class PoformLog
    {
        public int Pk { get; set; }
        public int PoformId { get; set; }
        public string? TimeLabelBgColor { get; set; }
        public string? TimeLabelHeader { get; set; }
        public string? TimeLabelBody { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
