using CarrotSystem.Models.MPS;
using CarrotSystem.Models.PO;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewPOF
    {
        public ViewPOF()
        {   
        }

        public User user { get; set; }
        
        public string? userFullName { get; set; }
        public string? supplierName { get; set; }

        public PurchaseView purchase { get; set; } = new PurchaseView();
        public List<PurchaseItemView> purchaseItems { get; set; }  = new List<PurchaseItemView>();
        public PurchaseTotalView purchaseTotal { get; set; } = new PurchaseTotalView();

        public int purchaseId { get; set; }

        public DateTime? dateFrom { get; set; }
        public DateTime? dateTo { get; set; }

        public string? stringDateFrom { get; set; }
        public string? stringDateTo { get; set; }
        public string? sender { get; set; }

        public IFormFile? attachFile { get; set; }

        public string? formStatus { get; set; }
        public string? formAction { get; set; }
        public string? poNumber { get; set; }

        public string? userGroup { get; set; }
        public string? userName { get; set; }
        
        public string? templateName { get; set; }

        public string? appStatus { get; set; }
        public string? appComment { get; set; }
        public string? appBackPage { get; set; }
        
        public Poform poForm { get; set; }
        public Poform savedForm { get; set; }

        public List<Poform> formList { get; set; }
        public List<PoformLog> formLogList { get; set; }

        public List<PoformFile> fileList { get; set;}
        public List<PoformFile> savedFileList { get; set; }
    }
}
