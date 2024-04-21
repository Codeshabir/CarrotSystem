using CarrotSystem.Models.ViewModel;
using Org.BouncyCastle.Asn1.Mozilla;

namespace CarrotSystem.Models.ViewModels
{
    public class PickingListViewModel
    {
        public DispatchView DispatchView { get; set; }
        public List<DispatchItemView> DispatchViewList { get; set; }
    }
}
