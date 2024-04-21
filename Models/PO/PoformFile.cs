using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.PO
{
    public partial class PoformFile
    {
        public int Aid { get; set; }
        public int FormId { get; set; }
        public int FileSetId { get; set; }
        public string? AttachedFileName { get; set; }
        public string? AttachedFileAddress { get; set; }
        public DateTime DateUploaded { get; set; }
    }
}
