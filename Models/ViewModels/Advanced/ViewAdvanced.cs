
using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewAdvanced
    {
        public Period period { get; set; }
        public List<Period> periodList { get; set; }

        public List<Tax> taxrateList { get; set; }

        public List<string> endDateList { get; set; }

        public int focusId { get; set; }

    }
}
