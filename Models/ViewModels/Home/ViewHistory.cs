using CarrotSystem.Models.MPS;
using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewHistory
    {
        public EmailGroup emailGroup { get; set; }
        public List<EmailGroup> emailList { get; set; }
        public List<Event> eventList { get; set; }
    }
}
