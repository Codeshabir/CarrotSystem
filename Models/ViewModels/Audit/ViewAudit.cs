using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewAudit
    {
        public Event events { get; set; }
        public List<Event> eventList { get; set; }

        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        
        public string strDateFrom { get; set; }
        public string strDateTo { get; set; }

        public List<string> eventTypeList { get; set; }
        public string eventType { get; set; }
        public string actionType { get; set; }

    }
}
