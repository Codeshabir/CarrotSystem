using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;

namespace CarrotSystem.Services
{
    public interface IReportsService
    {
        List<RawProduceView> GetRawProduceList(DateTime dateFrom, DateTime dateTo);
        List<RawProduceView> GetIPProduceList(DateTime dateFrom, DateTime dateTo);
        List<RawProduceView> GetIFProduceList(DateTime dateFrom, DateTime dateTo);
    }

    public class ReportsService : IReportsService
    {
        // Constructor
        private readonly IEventWriter _logger;
        private readonly IContextService _context;
        private readonly ISystemService _common;

        public ReportsService(IEventWriter logger, IContextService context, ISystemService common)
        {
            _logger = logger;
            _context = context;
            _common = common;
        }

        public ReportsService()
        {
        }

        public List<Period> GetPeriodListByDate(DateTime dateFrom, DateTime dateTo)
        {
            List<Period> returnPeriod = new List<Period>();

            if (_context.GetMPSContext().Period.Any(w => w.StartDate.HasValue && w.EndDate.HasValue && dateFrom.Date.CompareTo(w.StartDate.Value.Date) >= 0 && dateTo.Date.CompareTo(w.EndDate.Value.Date) <= 0))
            {
                returnPeriod = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue &&dateFrom.Date.CompareTo(w.StartDate.Value.Date) >= 0 &&dateTo.Date.CompareTo(w.EndDate.Value.Date) <= 0).ToList();
            }

            return returnPeriod;
        }

        public Period GetTccPeriodByDate(DateTime baseDate)
        {
            Period returnPeriod = new Period();

            if (_context.GetMPSContext().Period.Any(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0))
            {
                returnPeriod = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();
            }
            else
            {
                returnPeriod = _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First();
            }

            return returnPeriod;
        }

        public List<RawProduceView> GetRawProduceList(DateTime dateFrom, DateTime dateTo)
        {
            List<RawProduceView> rawProduceList = new List<RawProduceView>();

            var periodList = GetPeriodListByDate(dateFrom, dateTo);

            var cosingUnitProductCode = "IRCARROT";

            var inventoryList = new List<ProductInventory>();
            var ytdInventoryList = new List<ProductInventoryYtd>();

            foreach (var period in periodList)
            {
                inventoryList.AddRange(_context.GetMPSContext().ProductInventory.Where(w => w.PeriodId.HasValue && w.PeriodId.Value.Equals(period.Id) && (!string.IsNullOrEmpty(w.ProductCode)) && w.ProductCode.Substring(0, 2).Equals("IR")).ToList());
                ytdInventoryList.AddRange(_context.GetMPSContext().ProductInventoryYtd.Where(w => w.PeriodId.HasValue && w.PeriodId.Value.Equals(period.Id) && (!string.IsNullOrEmpty(w.ProductCode)) && w.ProductCode.Substring(0, 2).Equals("IR")).ToList());
            }

            var inventoryGroup = inventoryList
                .GroupBy(g => new { ProductCode = g.ProductCode}, (key, group) =>
                new {
                    ReportId = group.Sum(s=>s.PeriodId),
                    ProductCode = key.ProductCode,
                    OpeningStock = group.Sum(s => s.Opening),
                    OpeningValue = group.Sum(s => s.OpeningValue),
                    Purchased = group.Sum(s => s.Purchased),
                    PClaimed = group.Sum(s => s.PClaimed),
                    RClaimed = group.Sum(s => s.RClaimed),
                    Sold = group.Sum(s => s.Sold),
                    SClaimed = group.Sum(s => s.SClaimed),
                    Wasted = group.Sum(s => s.UcWasted),
                    TransFrom = group.Sum(s => s.TransFrom),
                    TransTo = group.Sum(s => s.TransTo),
                    PackUsed = group.Sum(s => s.PackUsed),
                    Packed = group.Sum(s => s.Packed),
                    Calculated = group.Sum(s => s.Calculated),
                    StockCount = group.Sum(s => s.StockCount),
                    Variance = group.Sum(s => s.Variance),
                    Closing = group.Sum(s => s.Closing),
                    ClosingValue = group.Sum(s => s.ClosingValue),
                    Result = group.ToList()
                }).ToList();


            foreach (var inventory in inventoryGroup)
            {
                RawProduceView rawProduceView = new RawProduceView();

                rawProduceView.ProductCode = inventory.ProductCode;
                rawProduceView.Description = _context.GetMPSContext().Product.Where(w => (!string.IsNullOrEmpty(w.Code)) && w.Code.Equals(inventory.ProductCode)).First().Desc;

                rawProduceView.OpeningStock = inventory.OpeningStock.Value;
                rawProduceView.OpeningValue = inventory.OpeningValue.Value;

                rawProduceView.Purchased = inventory.Purchased.Value;
                rawProduceView.Packed = inventory.Packed.Value;
                rawProduceView.PClaimed = inventory.PClaimed.Value;
                rawProduceView.TransTo = inventory.TransTo.Value;

                rawProduceView.Added = (rawProduceView.Purchased + rawProduceView.Packed + rawProduceView.PClaimed + rawProduceView.TransTo);
                    
                rawProduceView.Sold = inventory.Sold.Value;
                rawProduceView.SClaimed = inventory.SClaimed.Value;
                rawProduceView.Wasted = inventory.Wasted.Value;
                rawProduceView.TransFrom = inventory.TransFrom.Value;
                rawProduceView.PackUsed = inventory.PackUsed.Value;

                rawProduceView.Taken = (rawProduceView.Sold + rawProduceView.SClaimed + rawProduceView.Wasted + rawProduceView.TransFrom + rawProduceView.PackUsed);

                rawProduceView.StockCalc = rawProduceView.OpeningStock + rawProduceView.Added - rawProduceView.Taken;

                rawProduceView.StockCount = inventory.StockCount.Value;

                rawProduceView.VarUnits = rawProduceView.StockCount - rawProduceView.StockCalc;
                     
                if (rawProduceView.ProductCode.Equals(cosingUnitProductCode))
                {
                    //Here
                    var kgsOpening = rawProduceView.OpeningStock;
                    var valueAmount = rawProduceView.OpeningValue;

                    var costPerKgs = 0.0;

                    if (kgsOpening != 0)
                    {
                        costPerKgs = valueAmount / kgsOpening;
                    }

                    var trnsferList = new List<ProductTransfer>();
                    trnsferList = _context.GetMPSContext().ProductTransfer.Where(w => w.TransferDate.HasValue && w.TransferDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.TransferDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                    var transferGroup = trnsferList
                            .GroupBy(g => new { FromCode = g.FromProduct, ToCode = g.ToProduct }, (key, group) =>
                            new {
                                FromCode = key.FromCode,
                                ToCode = key.ToCode,
                                FromQty = group.Sum(s => s.FromQty),
                                ToQty = group.Sum(s => s.ToQty),
                                Result = group.ToList()
                            }).ToList();

                    var transferInKgs = 0.0;

                    if (transferGroup.Any(w => w.FromCode.Equals(cosingUnitProductCode)))
                    {
                        transferInKgs = transferGroup.Where(w => w.FromCode.Equals(cosingUnitProductCode)).First().ToQty.Value;
                    }

                    var transferOutVal = 0.0;

                    if (transferGroup.Any(w => w.ToCode.Equals(cosingUnitProductCode)))
                    {
                        //transferOutVal = transferGroup.Where(w => w.ToCode.Equals(cosingUnitProductCode)).First().Price;
                    }

                    var purchaseList = new List<Purchase>();
                    var purchaseItemList = new List<PurchaseItem>();

                    purchaseList = _context.GetMPSContext().Purchase.Where(w => w.DeliveryDate.HasValue && w.DeliveryDate.Value.Date.CompareTo(dateFrom.Date) >= 0 && w.DeliveryDate.Value.Date.CompareTo(dateTo.Date) <= 0).ToList();

                    foreach (var pItem in purchaseList)
                    {
                        if (_context.GetMPSContext().PurchaseItem.Any(x => x.InvoiceId.Equals(pItem.InvoiceId)))
                        {
                            purchaseItemList.AddRange(_context.GetMPSContext().PurchaseItem.Where(x => x.InvoiceId.Equals(pItem.InvoiceId) && x.ProductCode.Substring(0,2).Equals("IR")).ToList());
                        }
                    }

                    var purchaseGroup = purchaseItemList
                            .GroupBy(g => new { ProductType = g.ProductCode.Substring(0, 2) }, (key, group) =>
                            new {
                                ProductType = key.ProductType,
                                Qty = group.Sum(s => s.InvoicedQty.Value),
                                UnitAmount = group.Sum(s => s.Price.Value),
                                TotalAmount = group.Sum(s => s.InvoicedQty.Value * s.Price.Value),
                                Result = group.ToList()
                            }).ToList();

                    var purchaseKgs = 0.0;
                    var purchaseVal = 0.0;
                    var purchaseCost = 0.0;

                    if (purchaseGroup.Any(w => w.ProductType.Equals(cosingUnitProductCode.Substring(0, 2))))
                    {
                        var pGroupItem = purchaseGroup.Where(w => w.ProductType.Equals(cosingUnitProductCode.Substring(0, 2))).First();

                        purchaseKgs = pGroupItem.Qty;
                        purchaseVal = pGroupItem.TotalAmount;

                        if (purchaseKgs != 0)
                        {
                            purchaseCost = purchaseVal / purchaseKgs;
                        }

                    }

                    var wasteList = new List<Waste>();
                    wasteList = _context.GetMPSContext().Waste.Where(w => w.WasteDate.HasValue && dateFrom.Date.CompareTo(w.WasteDate.Value.Date) >= 0 && dateTo.Date.CompareTo(w.WasteDate.Value.Date) <= 0).ToList();

                    var wasteCost = 0.351345172102932;

                    var wasteGroup = wasteList
                            .GroupBy(g => new { ProductCode = g.ProductCode }, (key, group) =>
                            new {
                                ProductCode = key.ProductCode,
                                WastedQty = group.Sum(s => s.Qty),
                                WastedAmount = group.Sum(s => s.Qty) * wasteCost,
                                Result = group.ToList()
                            }).ToList();

                    var wasteValue = 0.0;

                    if (wasteGroup.Any(x => x.ProductCode.Equals("IRCARROT")))
                    {
                        wasteValue = wasteGroup.Where(x => x.ProductCode.Equals("IRCARROT")).First().WastedAmount.Value;
                    }

                    var totalAdditionKgs = 0.0;
                    var totalAdditionVal = 0.0;
                    var totalAdditionCost = 0.0;

                    totalAdditionKgs = transferInKgs + purchaseKgs;
                    totalAdditionVal = transferOutVal + purchaseVal + wasteValue;

                    if (totalAdditionKgs != 0)
                    {
                        totalAdditionCost = totalAdditionVal / totalAdditionKgs;
                    }

                    var sohKgs = kgsOpening + totalAdditionKgs;
                    var sohVal = valueAmount + totalAdditionVal;
                    var sohCost = 0.0;

                    if (sohKgs != 0)
                    {
                        sohCost = sohVal / sohKgs;
                    }

                    var wasteDisposedProductCode = "IRCARWASTE";
                    var wasteDisposedPurchase = 0.0;

                    if (inventoryGroup.Any(x => x.ProductCode.Equals(wasteDisposedProductCode)))
                    {
                        wasteDisposedPurchase = inventoryGroup.Where(x => x.ProductCode.Equals(wasteDisposedProductCode)).First().Wasted.Value;
                    }

                    var transferOuttoIWProduct = "IWCARROT";
                    var transferOuttoIW = 0.0;

                    if (inventoryGroup.Any(x => x.ProductCode.Equals(transferOuttoIWProduct)))
                    {
                        transferOuttoIW = inventoryGroup.Where(x => x.ProductCode.Equals(transferOuttoIWProduct)).First().TransTo.Value;
                    }

                    var secondPackedProduct = "IRCARSECONDS";
                    var secondPacked = 0.0;

                    if (inventoryGroup.Any(x => x.ProductCode.Equals(secondPackedProduct)))
                    {
                        secondPacked = inventoryGroup.Where(x => x.ProductCode.Equals(secondPackedProduct)).First().PackUsed.Value;
                    }

                    var rawCarrotWastedProduct = "IRCARROT";
                    var rawCarrotWasted = 0.0;

                    if (inventoryGroup.Any(x => x.ProductCode.Equals(rawCarrotWastedProduct)))
                    {
                        rawCarrotWasted = inventoryGroup.Where(x => x.ProductCode.Equals(rawCarrotWastedProduct)).First().Wasted.Value;
                    }

                    var rawCarrotPackedProduct = "IRCARROT";
                    var rawCarrotPacked = 0.0;

                    if (inventoryGroup.Any(x => x.ProductCode.Equals(rawCarrotPackedProduct)))
                    {
                        rawCarrotPacked = inventoryGroup.Where(x => x.ProductCode.Equals(rawCarrotPackedProduct)).First().PackUsed.Value;
                    }

                    var valAmount = 0.0;

                    if (rawCarrotPacked < kgsOpening)
                    {
                        valAmount = rawCarrotPacked * kgsOpening;
                    }
                    else
                    {
                        valAmount = rawCarrotPacked - kgsOpening * totalAdditionCost;
                    }

                    var rawCarrotPackedCost = 0.0;

                    if (rawCarrotPacked != 0)
                    {
                        rawCarrotPackedCost = valAmount / rawCarrotPacked;
                    }

                    var irKGs = wasteDisposedPurchase + transferOuttoIW + secondPacked + rawCarrotWasted + rawCarrotPacked;

                    var costUnit = 0.0;

                    if (irKGs != 0)
                    {
                        costUnit = valAmount / irKGs;
                    }

                    rawProduceView.CostingUnit = costUnit;
                }
                else
                {
                    rawProduceView.CostingUnit = 0;
                }

                if (rawProduceView.StockCount == 0)
                {
                    //Check
                    rawProduceView.VarCost = rawProduceView.VarUnits * rawProduceView.CostingUnit;
                }
                else
                {
                    rawProduceView.VarCost = rawProduceView.VarUnits * rawProduceView.CostingUnit;
                }

                rawProduceView.ClosingStock = rawProduceView.StockCount * rawProduceView.CostingUnit;

                rawProduceView.RClaimed = inventory.RClaimed.Value;

                rawProduceView.Calculated = inventory.Calculated.Value;

                rawProduceView.Variance = inventory.Variance.Value;
                rawProduceView.Closing = inventory.Closing.Value;
                rawProduceView.ClosingValue = inventory.ClosingValue.Value;

                var checkNill = rawProduceView.Purchased + rawProduceView.Packed + rawProduceView.PClaimed + rawProduceView.TransTo + rawProduceView.Added +
                    rawProduceView.Sold + rawProduceView.Wasted + rawProduceView.SClaimed + rawProduceView.TransFrom + rawProduceView.PackUsed +
                    rawProduceView.Taken + rawProduceView.StockCalc + rawProduceView.StockCount + rawProduceView.VarUnits;

                var checkYtd = rawProduceView.Purchased + rawProduceView.Packed + rawProduceView.PClaimed + rawProduceView.TransTo + rawProduceView.Added +
                    rawProduceView.Sold + rawProduceView.Wasted + rawProduceView.SClaimed + rawProduceView.TransFrom + rawProduceView.PackUsed +
                    rawProduceView.Taken + rawProduceView.StockCalc + rawProduceView.StockCount + rawProduceView.VarUnits;

                if(checkNill > 0 && checkYtd > 0)
                {
                    rawProduceList.Add(rawProduceView);
                }
            }

            var allToTalGroup = inventoryGroup
            .GroupBy(g => new { ReportId = g.ReportId }, (key, group) =>
                new {
                    ReportId = key.ReportId,
                    OpeningStock = group.Sum(s => s.OpeningStock),
                    OpeningValue = group.Sum(s => s.OpeningValue),
                    Purchased = group.Sum(s => s.Purchased),
                    PClaimed = group.Sum(s => s.PClaimed),
                    RClaimed = group.Sum(s => s.RClaimed),
                    Sold = group.Sum(s => s.Sold),
                    SClaimed = group.Sum(s => s.SClaimed),
                    Wasted = group.Sum(s => s.Wasted),
                    TransFrom = group.Sum(s => s.TransFrom),
                    TransTo = group.Sum(s => s.TransTo),
                    PackUsed = group.Sum(s => s.PackUsed),
                    Packed = group.Sum(s => s.Packed),
                    Calculated = group.Sum(s => s.Calculated),
                    StockCount = group.Sum(s => s.StockCount),
                    Variance = group.Sum(s => s.Variance),
                    Closing = group.Sum(s => s.Closing),
                    ClosingValue = group.Sum(s => s.ClosingValue),
                    Result = group.ToList()
                }).ToList();

            foreach (var total in allToTalGroup)
            {
                RawProduceView totalProduceView = new RawProduceView();

                totalProduceView.ProductCode = "Total";
                totalProduceView.Description = "Total";

                totalProduceView.OpeningStock = total.OpeningStock.Value;

                totalProduceView.Purchased = total.Purchased.Value;
                totalProduceView.Packed = total.Packed.Value;
                totalProduceView.PClaimed = total.PClaimed.Value;
                totalProduceView.TransTo = total.TransTo.Value;

                totalProduceView.Added = (totalProduceView.Purchased + totalProduceView.Packed + totalProduceView.PClaimed + totalProduceView.TransTo);

                totalProduceView.Sold = total.Sold.Value;
                totalProduceView.SClaimed = total.SClaimed.Value;
                totalProduceView.Wasted = total.Wasted.Value;
                totalProduceView.TransFrom = total.TransFrom.Value;
                totalProduceView.PackUsed = total.PackUsed.Value;

                totalProduceView.Taken = (totalProduceView.Sold + totalProduceView.SClaimed + totalProduceView.Wasted + totalProduceView.TransFrom + totalProduceView.PackUsed);

                totalProduceView.StockCalc = totalProduceView.OpeningStock + totalProduceView.Added - totalProduceView.Taken;

                totalProduceView.StockCount = total.StockCount.Value;

                totalProduceView.VarUnits = totalProduceView.StockCount - totalProduceView.StockCalc;

                if (totalProduceView.ProductCode.Equals(cosingUnitProductCode))
                {
                    totalProduceView.CostingUnit = 0;
                }
                else
                {
                    totalProduceView.CostingUnit = 0;
                }

                if (totalProduceView.StockCount == 0)
                {
                    //Check
                    totalProduceView.VarCost = totalProduceView.VarUnits * totalProduceView.CostingUnit;
                }
                else
                {
                    totalProduceView.VarCost = totalProduceView.VarUnits * totalProduceView.CostingUnit;
                }

                totalProduceView.ClosingStock = totalProduceView.StockCount * totalProduceView.CostingUnit;

                totalProduceView.OpeningValue = total.OpeningValue.Value;

                totalProduceView.RClaimed = total.RClaimed.Value;

                totalProduceView.Calculated = total.Calculated.Value;

                totalProduceView.Variance = total.Variance.Value;
                totalProduceView.Closing = total.Closing.Value;
                totalProduceView.ClosingValue = total.ClosingValue.Value;

                rawProduceList.Add(totalProduceView);
            }

            return rawProduceList;
        }

        public List<RawProduceView> GetIPProduceList(DateTime dateFrom, DateTime dateTo)
        {
            List<RawProduceView> rawProduceList = new List<RawProduceView>();

            var periodList = GetPeriodListByDate(dateFrom, dateTo);

            var inventoryList = new List<ProductInventory>();
            var ytdInventoryList = new List<ProductInventoryYtd>();

            foreach (var period in periodList)
            {
                inventoryList.AddRange(_context.GetMPSContext().ProductInventory.Where(w => w.PeriodId.HasValue && w.PeriodId.Value.Equals(period.Id) && (!string.IsNullOrEmpty(w.ProductCode)) && w.ProductCode.Substring(0, 2).Equals("IP")).ToList());
                ytdInventoryList.AddRange(_context.GetMPSContext().ProductInventoryYtd.Where(w => w.PeriodId.HasValue && w.PeriodId.Value.Equals(period.Id) && (!string.IsNullOrEmpty(w.ProductCode)) && w.ProductCode.Substring(0, 2).Equals("IP")).ToList());
            }

            var inventoryGroup = inventoryList
                    .GroupBy(g => new { ProductCode = g.ProductCode }, (key, group) =>
                    new {
                        ReportId = group.Sum(s=>s.PeriodId),
                        ProductCode = key.ProductCode,
                        OpeningStock = group.Sum(s => s.Opening),
                        OpeningValue = group.Sum(s => s.OpeningValue),
                        Purchased = group.Sum(s => s.Purchased),
                        PClaimed = group.Sum(s => s.PClaimed),
                        RClaimed = group.Sum(s => s.RClaimed),
                        Sold = group.Sum(s => s.Sold),
                        SClaimed = group.Sum(s => s.SClaimed),
                        Wasted = group.Sum(s => s.UcWasted),
                        TransFrom = group.Sum(s => s.TransFrom),
                        TransTo = group.Sum(s => s.TransTo),
                        PackUsed = group.Sum(s => s.PackUsed),
                        Packed = group.Sum(s => s.Packed),
                        Calculated = group.Sum(s => s.Calculated),
                        StockCount = group.Sum(s => s.StockCount),
                        Variance = group.Sum(s => s.Variance),
                        Closing = group.Sum(s => s.Closing),
                        ClosingValue = group.Sum(s => s.ClosingValue),
                        Result = group.ToList()
                    }).ToList();


            foreach (var inventory in inventoryGroup)
            {
                RawProduceView rawProduceView = new RawProduceView();

                rawProduceView.ProductCode = inventory.ProductCode;
                rawProduceView.Description = _context.GetMPSContext().Product.Where(w => (!string.IsNullOrEmpty(w.Code)) && w.Code.Equals(inventory.ProductCode)).First().Desc;

                rawProduceView.OpeningStock = inventory.OpeningStock.Value;

                rawProduceView.Purchased = inventory.Purchased.Value;
                rawProduceView.Packed = inventory.Packed.Value;
                rawProduceView.PClaimed = inventory.PClaimed.Value;
                rawProduceView.TransTo = inventory.TransTo.Value;

                rawProduceView.Added = (rawProduceView.Purchased + rawProduceView.Packed + rawProduceView.PClaimed + rawProduceView.TransTo);

                rawProduceView.Sold = inventory.Sold.Value;
                rawProduceView.SClaimed = inventory.SClaimed.Value;
                rawProduceView.Wasted = inventory.Wasted.Value;
                rawProduceView.TransFrom = inventory.TransFrom.Value;
                rawProduceView.PackUsed = inventory.PackUsed.Value;

                rawProduceView.Taken = (rawProduceView.Sold + rawProduceView.SClaimed + rawProduceView.Wasted + rawProduceView.TransFrom + rawProduceView.PackUsed);

                rawProduceView.StockCalc = rawProduceView.OpeningStock + rawProduceView.Added - rawProduceView.Taken;

                rawProduceView.StockCount = inventory.StockCount.Value;

                rawProduceView.VarUnits = rawProduceView.StockCount - rawProduceView.StockCalc;

                if (rawProduceView.ProductCode.Equals("IRCARROT"))
                {
                    rawProduceView.CostingUnit = 0;
                }
                else
                {
                    rawProduceView.CostingUnit = 0;
                }

                if (rawProduceView.StockCount == 0)
                {
                    //Check
                    rawProduceView.VarCost = rawProduceView.VarUnits * rawProduceView.CostingUnit;
                }
                else
                {
                    rawProduceView.VarCost = rawProduceView.VarUnits * rawProduceView.CostingUnit;
                }

                rawProduceView.ClosingStock = rawProduceView.StockCount * rawProduceView.CostingUnit;

                rawProduceView.OpeningValue = inventory.OpeningValue.Value;

                rawProduceView.RClaimed = inventory.RClaimed.Value;

                rawProduceView.Calculated = inventory.Calculated.Value;

                rawProduceView.Variance = inventory.Variance.Value;
                rawProduceView.Closing = inventory.Closing.Value;
                rawProduceView.ClosingValue = inventory.ClosingValue.Value;

                rawProduceList.Add(rawProduceView);
            }

            var allToTalGroup = inventoryGroup
            .GroupBy(g => new { ReportId = g.ReportId }, (key, group) =>
                new {
                    ReportId = key.ReportId,
                    OpeningStock = group.Sum(s => s.OpeningStock),
                    OpeningValue = group.Sum(s => s.OpeningValue),
                    Purchased = group.Sum(s => s.Purchased),
                    PClaimed = group.Sum(s => s.PClaimed),
                    RClaimed = group.Sum(s => s.RClaimed),
                    Sold = group.Sum(s => s.Sold),
                    SClaimed = group.Sum(s => s.SClaimed),
                    Wasted = group.Sum(s => s.Wasted),
                    TransFrom = group.Sum(s => s.TransFrom),
                    TransTo = group.Sum(s => s.TransTo),
                    PackUsed = group.Sum(s => s.PackUsed),
                    Packed = group.Sum(s => s.Packed),
                    Calculated = group.Sum(s => s.Calculated),
                    StockCount = group.Sum(s => s.StockCount),
                    Variance = group.Sum(s => s.Variance),
                    Closing = group.Sum(s => s.Closing),
                    ClosingValue = group.Sum(s => s.ClosingValue),
                    Result = group.ToList()
                }).ToList();

            foreach (var total in allToTalGroup)
            {
                RawProduceView totalProduceView = new RawProduceView();

                totalProduceView.ProductCode = "Total";
                totalProduceView.Description = "Total";

                totalProduceView.OpeningStock = total.OpeningStock.Value;

                totalProduceView.Purchased = total.Purchased.Value;
                totalProduceView.Packed = total.Packed.Value;
                totalProduceView.PClaimed = total.PClaimed.Value;
                totalProduceView.TransTo = total.TransTo.Value;

                totalProduceView.Added = (totalProduceView.Purchased + totalProduceView.Packed + totalProduceView.PClaimed + totalProduceView.TransTo);

                totalProduceView.Sold = total.Sold.Value;
                totalProduceView.SClaimed = total.SClaimed.Value;
                totalProduceView.Wasted = total.Wasted.Value;
                totalProduceView.TransFrom = total.TransFrom.Value;
                totalProduceView.PackUsed = total.PackUsed.Value;

                totalProduceView.Taken = (totalProduceView.Sold + totalProduceView.SClaimed + totalProduceView.Wasted + totalProduceView.TransFrom + totalProduceView.PackUsed);

                totalProduceView.StockCalc = totalProduceView.OpeningStock + totalProduceView.Added - totalProduceView.Taken;

                totalProduceView.StockCount = total.StockCount.Value;

                totalProduceView.VarUnits = totalProduceView.StockCount - totalProduceView.StockCalc;

                if (totalProduceView.ProductCode.Equals("IRCARROT"))
                {
                    totalProduceView.CostingUnit = 0;
                }
                else
                {
                    totalProduceView.CostingUnit = 0;
                }

                if (totalProduceView.StockCount == 0)
                {
                    //Check
                    totalProduceView.VarCost = totalProduceView.VarUnits * totalProduceView.CostingUnit;
                }
                else
                {
                    totalProduceView.VarCost = totalProduceView.VarUnits * totalProduceView.CostingUnit;
                }

                totalProduceView.ClosingStock = totalProduceView.StockCount * totalProduceView.CostingUnit;

                totalProduceView.OpeningValue = total.OpeningValue.Value;

                totalProduceView.RClaimed = total.RClaimed.Value;

                totalProduceView.Calculated = total.Calculated.Value;

                totalProduceView.Variance = total.Variance.Value;
                totalProduceView.Closing = total.Closing.Value;
                totalProduceView.ClosingValue = total.ClosingValue.Value;

                rawProduceList.Add(totalProduceView);
            }

            return rawProduceList;
        }

        public List<RawProduceView> GetIFProduceList(DateTime dateFrom, DateTime dateTo)
        {
            List<RawProduceView> rawProduceList = new List<RawProduceView>();

            var periodList = GetPeriodListByDate(dateFrom, dateTo);

            var inventoryList = new List<ProductInventory>();
            var ytdInventoryList = new List<ProductInventoryYtd>();

            foreach (var period in periodList)
            {
                inventoryList.AddRange(_context.GetMPSContext().ProductInventory.Where(w => w.PeriodId.HasValue && w.PeriodId.Value.Equals(period.Id) && (!string.IsNullOrEmpty(w.ProductCode)) && w.ProductCode.Substring(0, 2).Equals("IF")).ToList());
                ytdInventoryList.AddRange(_context.GetMPSContext().ProductInventoryYtd.Where(w => w.PeriodId.HasValue && w.PeriodId.Value.Equals(period.Id) && (!string.IsNullOrEmpty(w.ProductCode)) && w.ProductCode.Substring(0, 2).Equals("IF")).ToList());
            }

            var inventoryGroup = inventoryList
                    .GroupBy(g => new { ProductCode = g.ProductCode }, (key, group) =>
                    new {
                        ReportId = group.Sum(s => s.PeriodId),
                        ProductCode = key.ProductCode,
                        OpeningStock = group.Sum(s => s.Opening),
                        OpeningValue = group.Sum(s => s.OpeningValue),
                        Purchased = group.Sum(s => s.Purchased),
                        PClaimed = group.Sum(s => s.PClaimed),
                        RClaimed = group.Sum(s => s.RClaimed),
                        Sold = group.Sum(s => s.Sold),
                        SClaimed = group.Sum(s => s.SClaimed),
                        Wasted = group.Sum(s => s.UcWasted),
                        TransFrom = group.Sum(s => s.TransFrom),
                        TransTo = group.Sum(s => s.TransTo),
                        PackUsed = group.Sum(s => s.PackUsed),
                        Packed = group.Sum(s => s.Packed),
                        Calculated = group.Sum(s => s.Calculated),
                        StockCount = group.Sum(s => s.StockCount),
                        Variance = group.Sum(s => s.Variance),
                        Closing = group.Sum(s => s.Closing),
                        ClosingValue = group.Sum(s => s.ClosingValue),
                        Result = group.ToList()
                    }).ToList();


            foreach (var inventory in inventoryGroup)
            {
                RawProduceView rawProduceView = new RawProduceView();

                rawProduceView.ProductCode = inventory.ProductCode;
                rawProduceView.Description = _context.GetMPSContext().Product.Where(w => (!string.IsNullOrEmpty(w.Code)) && w.Code.Equals(inventory.ProductCode)).First().Desc;

                rawProduceView.OpeningStock = inventory.OpeningStock.Value;

                rawProduceView.Purchased = inventory.Purchased.Value;
                rawProduceView.Packed = inventory.Packed.Value;
                rawProduceView.PClaimed = inventory.PClaimed.Value;
                rawProduceView.TransTo = inventory.TransTo.Value;

                rawProduceView.Added = (rawProduceView.Purchased + rawProduceView.Packed + rawProduceView.PClaimed + rawProduceView.TransTo);

                rawProduceView.Sold = inventory.Sold.Value;
                rawProduceView.SClaimed = inventory.SClaimed.Value;
                rawProduceView.Wasted = inventory.Wasted.Value;
                rawProduceView.TransFrom = inventory.TransFrom.Value;
                rawProduceView.PackUsed = inventory.PackUsed.Value;

                rawProduceView.Taken = (rawProduceView.Sold + rawProduceView.SClaimed + rawProduceView.Wasted + rawProduceView.TransFrom + rawProduceView.PackUsed);

                rawProduceView.StockCalc = rawProduceView.OpeningStock + rawProduceView.Added - rawProduceView.Taken;

                rawProduceView.StockCount = inventory.StockCount.Value;

                rawProduceView.VarUnits = rawProduceView.StockCount - rawProduceView.StockCalc;

                if (rawProduceView.ProductCode.Equals("IRCARROT"))
                {
                    rawProduceView.CostingUnit = 0;
                }
                else
                {
                    rawProduceView.CostingUnit = 0;
                }

                if (rawProduceView.StockCount == 0)
                {
                    //Check
                    rawProduceView.VarCost = rawProduceView.VarUnits * rawProduceView.CostingUnit;
                }
                else
                {
                    rawProduceView.VarCost = rawProduceView.VarUnits * rawProduceView.CostingUnit;
                }

                rawProduceView.ClosingStock = rawProduceView.StockCount * rawProduceView.CostingUnit;

                rawProduceView.OpeningValue = inventory.OpeningValue.Value;

                rawProduceView.RClaimed = inventory.RClaimed.Value;

                rawProduceView.Calculated = inventory.Calculated.Value;

                rawProduceView.Variance = inventory.Variance.Value;
                rawProduceView.Closing = inventory.Closing.Value;
                rawProduceView.ClosingValue = inventory.ClosingValue.Value;

                rawProduceList.Add(rawProduceView);
            }

            var allToTalGroup = inventoryGroup
            .GroupBy(g => new { ReportId = g.ReportId }, (key, group) =>
                new {
                    ReportId = key.ReportId,
                    OpeningStock = group.Sum(s => s.OpeningStock),
                    OpeningValue = group.Sum(s => s.OpeningValue),
                    Purchased = group.Sum(s => s.Purchased),
                    PClaimed = group.Sum(s => s.PClaimed),
                    RClaimed = group.Sum(s => s.RClaimed),
                    Sold = group.Sum(s => s.Sold),
                    SClaimed = group.Sum(s => s.SClaimed),
                    Wasted = group.Sum(s => s.Wasted),
                    TransFrom = group.Sum(s => s.TransFrom),
                    TransTo = group.Sum(s => s.TransTo),
                    PackUsed = group.Sum(s => s.PackUsed),
                    Packed = group.Sum(s => s.Packed),
                    Calculated = group.Sum(s => s.Calculated),
                    StockCount = group.Sum(s => s.StockCount),
                    Variance = group.Sum(s => s.Variance),
                    Closing = group.Sum(s => s.Closing),
                    ClosingValue = group.Sum(s => s.ClosingValue),
                    Result = group.ToList()
                }).ToList();

            foreach (var total in allToTalGroup)
            {
                RawProduceView totalProduceView = new RawProduceView();

                totalProduceView.ProductCode = "Total";
                totalProduceView.Description = "Total";

                totalProduceView.OpeningStock = total.OpeningStock.Value;

                totalProduceView.Purchased = total.Purchased.Value;
                totalProduceView.Packed = total.Packed.Value;
                totalProduceView.PClaimed = total.PClaimed.Value;
                totalProduceView.TransTo = total.TransTo.Value;

                totalProduceView.Added = (totalProduceView.Purchased + totalProduceView.Packed + totalProduceView.PClaimed + totalProduceView.TransTo);

                totalProduceView.Sold = total.Sold.Value;
                totalProduceView.SClaimed = total.SClaimed.Value;
                totalProduceView.Wasted = total.Wasted.Value;
                totalProduceView.TransFrom = total.TransFrom.Value;
                totalProduceView.PackUsed = total.PackUsed.Value;

                totalProduceView.Taken = (totalProduceView.Sold + totalProduceView.SClaimed + totalProduceView.Wasted + totalProduceView.TransFrom + totalProduceView.PackUsed);

                totalProduceView.StockCalc = totalProduceView.OpeningStock + totalProduceView.Added - totalProduceView.Taken;

                totalProduceView.StockCount = total.StockCount.Value;

                totalProduceView.VarUnits = totalProduceView.StockCount - totalProduceView.StockCalc;

                if (totalProduceView.ProductCode.Equals("IRCARROT"))
                {
                    totalProduceView.CostingUnit = 0;
                }
                else
                {
                    totalProduceView.CostingUnit = 0;
                }

                if (totalProduceView.StockCount == 0)
                {
                    //Check
                    totalProduceView.VarCost = totalProduceView.VarUnits * totalProduceView.CostingUnit;
                }
                else
                {
                    totalProduceView.VarCost = totalProduceView.VarUnits * totalProduceView.CostingUnit;
                }

                totalProduceView.ClosingStock = totalProduceView.StockCount * totalProduceView.CostingUnit;

                totalProduceView.OpeningValue = total.OpeningValue.Value;

                totalProduceView.RClaimed = total.RClaimed.Value;

                totalProduceView.Calculated = total.Calculated.Value;

                totalProduceView.Variance = total.Variance.Value;
                totalProduceView.Closing = total.Closing.Value;
                totalProduceView.ClosingValue = total.ClosingValue.Value;

                rawProduceList.Add(totalProduceView);
            }

            return rawProduceList;
        }


        public double GetTotalPurchases(string compName, int invId)
        {
            var grandTotal = 0.0;

            List<PurchaseItem> purchaseItems = new List<PurchaseItem>();

            if (_context.GetMPSContext().PurchaseItem.Any(w => w.InvoiceId.Equals(invId)))
            {
                purchaseItems = _context.GetMPSContext().PurchaseItem.Where(w => w.InvoiceId.Equals(invId)).ToList();
            }

            if (purchaseItems.Count > 0)
            {
                foreach (var item in purchaseItems)
                {
                    double taxRate = _context.GetMPSContext().Tax.Where(w => w.Code.Equals(item.Tax)).First().Rate.Value;

                    if (item.InvoicedQty.HasValue && item.Price.HasValue)
                    {
                        grandTotal = grandTotal + (item.InvoicedQty.Value * item.Price.Value * taxRate);
                    }
                }
            }

            return Math.Round(grandTotal, 2);
        }

        public double GetTotalSale(string compName, int invId)
        {
            decimal invoiceSubTotal = 0, invoiceTaxTotal = 0, invoiceTotal = 0;
            double grandTotal = 0.0;

            List<SaleItem> saleItemList = new List<SaleItem>();
            saleItemList = _context.GetMPSContext().SaleItem.Where(w => w.InvoiceId.Equals(invId)).ToList();

            if (saleItemList.Count > 0)
            {
                foreach (var sale in saleItemList)
                {
                    decimal invoiceSubItem = 0, invoiceTaxItem = 0, invoiceItemTotal = 0;

                    if (sale.InvoicedQty.HasValue && sale.Price.HasValue)
                    {
                        invoiceSubItem = ((decimal)sale.InvoicedQty.Value * (decimal)sale.Price.Value);
                    }

                    decimal taxRate = (decimal)_context.GetMPSContext().Tax.First(f => f.Code.Equals(sale.Tax)).Rate.Value;

                    invoiceTaxItem = (((decimal)sale.InvoicedQty.Value * (decimal)sale.Price.Value) * (1 - taxRate));

                    invoiceItemTotal = invoiceSubItem + invoiceTaxItem;

                    //Grand Total
                    invoiceSubTotal = invoiceSubTotal + invoiceSubItem;
                    invoiceTaxTotal = invoiceTaxTotal + invoiceTaxItem;
                    invoiceTotal = invoiceTotal + invoiceItemTotal;

                    grandTotal = grandTotal + (double) invoiceItemTotal;
                }
            }
            
            return (double)Math.Round(grandTotal, 2);
        }
        
        public int GetPeriodIdByDate(DateTime baseDate)
        {
            if(_context.GetMPSContext().Period.Any(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0))
            {
                return _context.GetMPSContext().Period.Where(w => w.StartDate.HasValue && w.EndDate.HasValue && baseDate.Date.CompareTo(w.StartDate.Value.Date) >= 0 && baseDate.Date.CompareTo(w.EndDate.Value.Date) <= 0).First().Id;
            }
            else
            {
                return 0;
            }
        }
        
        public Period GetPeriodById(string compName, int periodId)
        {
            Period returnPeriod = new Period();

            if (compName.Equals("TCC"))
            {
                if (_context.GetMPSContext().Period.Any(w => w.Id > 0 && w.Id.Equals(periodId)))
                {
                    returnPeriod = _context.GetMPSContext().Period.Where(w => w.Id > 0 && w.Id.Equals(periodId)).First();
                }
            }

            return returnPeriod;
        }

        public Period GetTccPeriodIdByInvoiceId(string invoiceType, int invoiceId)
        {
            Period returnPeriod = new Period();

            if(invoiceType.Equals("Purchase"))
            {
                var purchase = _context.GetMPSContext().Purchase.Where(x=>x.InvoiceId.Equals(invoiceId)).First();

                returnPeriod = GetTccPeriodByDate(purchase.DeliveryDate.Value);
            }

            return returnPeriod;
        }
        
        public int GetDeliveryMonthByInvoiceId(string compName, string invoiceType, int invoiceId)
        {
            int returnMonth = 0;

            if (invoiceType.Equals("Purchase"))
            {
                returnMonth = _context.GetMPSContext().Purchase.Where(x => x.InvoiceId.Equals(invoiceId)).First().DeliveryDate.Value.Month;
            }

            return returnMonth;
        }

        public int GetDeliveryYearByInvoiceId(string compName, string invoiceType, int invoiceId)
        {
            int returnYear = 0;
            
            if (invoiceType.Equals("Purchase"))
            {
                returnYear = _context.GetMPSContext().Purchase.Where(x => x.InvoiceId.Equals(invoiceId)).First().DeliveryDate.Value.Year;
            }

            return returnYear;
        }

        public double GetTaxRate(string compName, string taxCode)
        {
            return _context.GetMPSContext().Tax.Where(w=>w.Code.Equals(taxCode)).First().Rate.Value;
        }

        
        // End
    }
}
