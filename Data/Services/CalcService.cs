using CarrotSystem.Models.MPS;
using CarrotSystem.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace CarrotSystem.Services
{
    public interface ICalcService
    {
        //Calculation
        void UpdateInventory(Period period, string updateBy);
    }

    public class CalcService : ICalcService
    {
        private IContextService _context;
        private readonly IAPIService _api;
        private readonly IEventWriter _logger;
        public string userName = "";
        public string loginId = "";

        public CalcService(IEventWriter logger, IContextService context, IAPIService api)
        {
            _logger = logger;
            _context = context;
            _api = api;
        }

        public CalcService()
        {
        }

        public void UpdateInventory(Period period, string updateBy)
        {
            _logger.WriteTestLog("Start Calculate Period : " + period.StartDate.Value.ToString("dd/MM/yyyy") + " - " + period.EndDate.Value.ToString("dd/MM/yyyy") + ", ID : " + period.Id);

            try
            {
                     // Your existing code here
                    // 1. Clear Old Data
                    if (_context.GetMPSContext().ProductInventory.Any(a => a.PeriodId.Equals(period.Id)))
                    {
                        var oldInvenList = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(period.Id)).ToList();

                        _context.GetMPSContext().ProductInventory.RemoveRange(oldInvenList);
                        var deletedInventory = _context.GetMPSContext().SaveChanges();

                        if (deletedInventory > 0)
                        {
                            _logger.WriteTestLog("Removed [" + deletedInventory + "] old ProductInventory data, " + period.StartDate.Value.Date + " - " + period.EndDate.Value.Date);
                        }
                    }

                    // 2. Insert Inventory
                    InsertInventory(period, updateBy);

                    // 3. Update Inventory (Added, AddedValue, PackedValue, ReductionValue, ClosingValue)
                    UpdateInventory2(period);

                    // 4. Update YTD Inventory
                    UpdateYTDInventory(period);

                    // 5. Delete Product Cont Data
                    if (_context.GetMPSContext().ProductCont.Any(x => x.PeriodId.HasValue && x.PeriodId >= (short)period.Id))
                    {
                        var oldProductContList = _context.GetMPSContext().ProductCont.Where(x => x.PeriodId.HasValue && x.PeriodId >= (short)period.Id).ToList();

                        _context.GetMPSContext().ProductCont.RemoveRange(oldProductContList);
                        var deletedProductCont = _context.GetMPSContext().SaveChanges();

                        if (deletedProductCont > 0)
                        {
                            _logger.WriteTestLog("Removed [" + deletedProductCont + "] old ProductCont data, " + period.StartDate.Value.Date + " - " + period.EndDate.Value.Date);
                        }
                    }

                    // 6. Insert ProductCont Data
                    InsertProductCont(period);
                
            }
            catch (Exception ex)
            {
                // Handle exceptions here
            }

            period.Calculated = true;
            period.UpdatedBy = updateBy;
            period.UpdatedOn = DateTime.Now;

            _context.GetMPSContext().Period.Update(period);
            _context.GetMPSContext().SaveChanges();


            _logger.WriteTestLog("The End of Calculation");
        }

        public void InsertInventory(Period period, string updateBy)
        {
            int prvPeriodID = period.Id - 1;

            //Get Purchase Data
            var purchaseList = _context.GetMPSContext().Purchase.Join(_context.GetMPSContext().PurchaseItem, purchase => purchase.InvoiceId, purchaseItem => purchaseItem.InvoiceId, (purchase, purchaseItem) => new { Purchase = purchase, PurchaseItem = purchaseItem }).Where(w => w.Purchase.InvoiceId > 0 && (w.Purchase.Status.Equals("Invoice") || w.Purchase.Status.Equals("Exported")) && (!w.Purchase.Type.Contains("Claim")) && w.Purchase.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.Purchase.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

            var purchaseGroup = purchaseList.GroupBy(g => new { ProductCode = g.PurchaseItem.ProductCode, Returned = g.Purchase.Returned, Type = g.Purchase.Type })
            .Select(s => new {
                purchaseType = s.Key.Type,
                productCode = s.Key.ProductCode,
                purchaseQty = s.Sum(b => b.PurchaseItem.InvoicedQty),
                purchaseCost = s.Sum(b => b.PurchaseItem.Price.Value * b.PurchaseItem.InvoicedQty),
                returned = s.Key.Returned.Value,
            }).ToList();

            //Get Purchase Freight Data
            var purFreightList = _context.GetMPSContext().Purchase
                .Join(_context.GetMPSContext().PurchaseItem, purchase => purchase.InvoiceId, purchaseItem => purchaseItem.InvoiceId, (purchase, purchaseItem) => new { Purchase = purchase, PurchaseItem = purchaseItem })
                .Where(w => w.Purchase.InvoiceId > 0 && (w.Purchase.Status.Equals("Invoice") || w.Purchase.Status.Equals("Exported")) && w.Purchase.Type.Equals("FreightInw") && w.Purchase.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.Purchase.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0 && w.Purchase.PurchaseFreightReference.HasValue)
                .ToList();

            var purFreightGroup = purFreightList.GroupBy(g => new { ProductCode = g.PurchaseItem.ProductCode, Returned = g.Purchase.Returned, Type = g.Purchase.Type })
            .Select(s => new {
                purchaseType = s.Key.Type,
                productCode = s.Key.ProductCode,
                purchaseQty = s.Sum(b => b.PurchaseItem.InvoicedQty),
                purchaseCost = s.Sum(b => b.PurchaseItem.Price.Value * b.PurchaseItem.InvoicedQty),
                returned = s.Key.Returned.Value,
            }).ToList();

            //Get Purchase Claim List
            var purchaseClaimList = _context.GetMPSContext().Purchase.Join(_context.GetMPSContext().PurchaseItem, purchase => purchase.InvoiceId, purchaseItem => purchaseItem.InvoiceId, (purchase, purchaseItem) => new { Purchase = purchase, PurchaseItem = purchaseItem })
                .Where(w => w.Purchase.InvoiceId > 0 && (w.Purchase.Status.Equals("Invoice") || w.Purchase.Status.Equals("Exported")) && w.Purchase.Type.Contains("Claim") && w.Purchase.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.Purchase.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0)
                .ToList();

            var purClaimGroup = purchaseClaimList.GroupBy(g => new { ProductCode = g.PurchaseItem.ProductCode, Returned = g.Purchase.Returned, Type = g.Purchase.Type, InvoiceID = g.Purchase.InvoiceId })
            .Select(s => new {
                purchaseType = s.Key.Type,
                fileNumber = s.Key.InvoiceID,
                productCode = s.Key.ProductCode,
                claimQty = s.Sum(b => b.PurchaseItem.InvoicedQty) * -1,
                purchaseCost = s.Sum(b => b.PurchaseItem.Price.Value * b.PurchaseItem.InvoicedQty.Value),
                returned = s.Key.Returned.Value,
            }).ToList();

            //Get Sale Data
            var saleList = _context.GetMPSContext().Sale.Join(_context.GetMPSContext().SaleItem, sale => sale.InvoiceId, saleItem => saleItem.InvoiceId, (sale, saleItem) => new { Sale = sale, SaleItem = saleItem }).Where(w => w.Sale.InvoiceId > 0 && (w.Sale.Status.Equals("Invoice") || w.Sale.Status.Equals("Exported")) && (!w.Sale.Type.Contains("Claim")) && w.Sale.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.Sale.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

            var saleGroup = saleList.GroupBy(g => new { ProductCode = g.SaleItem.ProductCode, Type = g.Sale.Type })
            .Select(s => new {
                saleType = s.Key.Type,
                productCode = s.Key.ProductCode,
                saleQty = s.Sum(b => b.SaleItem.InvoicedQty),
                saleValue = s.Sum(b => b.SaleItem.Price.Value * b.SaleItem.InvoicedQty)
            }).ToList();

            List<WholeSaleUsed> wholeSaleUsedList = new List<WholeSaleUsed>();

            foreach (var wholeSale in saleGroup)
            {
                var wholeSaleProduct = _context.GetMPSContext().Product.Where(w => w.Code.Equals(wholeSale.productCode)).First();

                if (_api.IsMainGroup("WHOLESALE", wholeSaleProduct.MinorGroupId.Value))
                {
                    var wholeSaleRecipeItems = _context.GetMPSContext().ProductRecipe.Where(x => x.ProductCode.Equals(wholeSaleProduct.Code)).ToList();

                    foreach (var wholeSaleReItem in wholeSaleRecipeItems)
                    {
                        if (wholeSaleReItem.Embeded.HasValue && wholeSaleReItem.Embeded.Value.Equals(false))
                        {
                            WholeSaleUsed whSaleUsed = new WholeSaleUsed();

                            whSaleUsed.ProductCode = wholeSaleReItem.Component;
                            whSaleUsed.SaleUsedQty = wholeSaleReItem.Qty.Value * wholeSale.saleQty.Value;

                            wholeSaleUsedList.Add(whSaleUsed);
                        }
                    }
                }
            }

            var wholeSaleGroup = wholeSaleUsedList.GroupBy(g => new { ProductCode = g.ProductCode })
            .Select(s => new {
                productCode = s.Key.ProductCode,
                saleUsedQty = s.Sum(b => b.SaleUsedQty)
            }).ToList();

            //Get Sale Claim Data
            var saleClaimList = _context.GetMPSContext().Sale.Join(_context.GetMPSContext().SaleItem, sale => sale.InvoiceId, saleItem => saleItem.InvoiceId, (sale, saleItem) => new { Sale = sale, SaleItem = saleItem }).Where(w => w.Sale.InvoiceId > 0 && (w.Sale.Status.Equals("Invoice") || w.Sale.Status.Equals("Exported")) && w.Sale.Type.Contains("Claim") && w.Sale.DeliveryDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.Sale.DeliveryDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

            var saleClaimGroup = saleClaimList.GroupBy(g => new { ProductCode = g.SaleItem.ProductCode, Type = g.Sale.Type })
            .Select(s => new {
                saleType = s.Key.Type,
                productCode = s.Key.ProductCode,
                saleQty = s.Sum(b => b.SaleItem.InvoicedQty) * -1,
                saleValue = s.Sum(b => b.SaleItem.Price.Value * b.SaleItem.InvoicedQty)
            }).ToList();

            //Get Waste Data
            var wasteList = _context.GetMPSContext().Waste.Where(w => w.Qty.HasValue && w.WasteDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.WasteDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

            var wasteGroup = wasteList.GroupBy(g => new { ProductCode = g.ProductCode })
            .Select(s => new {
                //wasteReason = s.Key.Reason,
                productCode = s.Key.ProductCode,
                wasteQty = s.Sum(b => b.Qty)
            }).ToList();

            //Get Transfer Data
            var transferList = _context.GetMPSContext().ProductTransfer.Where(w => w.TransferDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.TransferDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

            var transferFromGroup = transferList.GroupBy(g => new { ProductCode = g.FromProduct })
            .Select(s => new {
                fromProductCode = s.Key.ProductCode,
                fromQty = s.Sum(b => b.FromQty)
            }).ToList();

            var transferToGroup = transferList.GroupBy(g => new { ProductCode = g.ToProduct })
           .Select(s => new {
               toProductCode = s.Key.ProductCode,
               toQty = s.Sum(b => b.ToQty)
           }).ToList();

            //Get Packed Data
            var packingList = _context.GetMPSContext().ProductPacking.Where(w => w.PackingDate.Value.Date.CompareTo(period.StartDate.Value.Date) >= 0 && w.PackingDate.Value.Date.CompareTo(period.EndDate.Value.Date) <= 0).ToList();

            var packingGroup = packingList.GroupBy(g => new { ProductCode = g.ProductCode })
            .Select(s => new {
                packingProductCode = s.Key.ProductCode,
                packingQty = s.Sum(b => b.ProductQty)
            }).ToList();

            var recipeList = new List<ProductPackUsed>();

            foreach (var pckUse in packingGroup)
            {
                var recipeItems = _context.GetMPSContext().ProductRecipe.Where(w => w.ProductCode.Equals(pckUse.packingProductCode)).ToList();

                foreach (var item in recipeItems)
                {
                    var unitItem = _context.GetMPSContext().Product.Where(w => w.Code.Equals(item.Component)).First();

                    if (_api.IsMainGroup("PACKAGING", unitItem.MinorGroupId.Value))
                    {
                        ProductPackUsed packUsedItem = new ProductPackUsed();

                        packUsedItem.ProductCode = unitItem.Code;
                        packUsedItem.PackUsedQty = item.Qty.Value * pckUse.packingQty.Value;

                        recipeList.Add(packUsedItem);
                    }
                    else if (_api.IsMainGroup("RAW", unitItem.MinorGroupId.Value))
                    {
                        ProductPackUsed packUsedItem = new ProductPackUsed();

                        packUsedItem.ProductCode = unitItem.Code;
                        packUsedItem.PackUsedQty = item.Qty.Value * pckUse.packingQty.Value;

                        recipeList.Add(packUsedItem);
                    }
                }
            }

            foreach (var saleUnit in saleGroup)
            {
                var saleRecipeItems = _context.GetMPSContext().ProductRecipe.Where(w => w.Component.Equals(saleUnit.productCode)).ToList();

                //_logger.LogError("Purchase Recipe Items : " + purchaseRecipeItems.Count + ", Code : " + purchaseUnit.productCode);

                foreach (var repItem in saleRecipeItems)
                {
                    var unItem = _context.GetMPSContext().Product.Where(w => w.Code.Equals(repItem.ProductCode)).First();

                    if (_api.IsMainGroup("PACKAGING", unItem.MinorGroupId.Value))
                    {
                        ProductPackUsed packUsedPurItem = new ProductPackUsed();

                        packUsedPurItem.ProductCode = unItem.Code;
                        packUsedPurItem.PackUsedQty = repItem.Qty.Value * saleUnit.saleQty.Value;

                        recipeList.Add(packUsedPurItem);
                    }
                }
            }

            foreach (var purUnit in purchaseGroup)
            {
                var purRecipeItems = _context.GetMPSContext().ProductRecipe.Where(w => w.Component.Equals(purUnit.productCode)).ToList();

                //_logger.LogError("Purchase Recipe Items : " + purchaseRecipeItems.Count + ", Code : " + purchaseUnit.productCode);

                foreach (var reItem in purRecipeItems)
                {
                    var unItem = _context.GetMPSContext().Product.Where(w => w.Code.Equals(reItem.ProductCode)).First();

                    if (_api.IsMainGroup("PACKAGING", unItem.MinorGroupId.Value))
                    {
                        ProductPackUsed packUsedPurItem = new ProductPackUsed();

                        packUsedPurItem.ProductCode = unItem.Code;
                        packUsedPurItem.PackUsedQty = reItem.Qty.Value * purUnit.purchaseQty.Value;

                        recipeList.Add(packUsedPurItem);
                    }
                }
            }

            var packingUsedGroup = recipeList.GroupBy(g => new { ProductCode = g.ProductCode })
            .Select(s => new {
                component = s.Key.ProductCode,
                packingUsedQty = s.Sum(b => b.PackUsedQty)
            }).ToList();


            List<Product> productList = new List<Product>();
            var tempProduct = _context.GetMPSContext().Product.ToList();

            foreach (var product in tempProduct)
            {
                var minorGroupId = product.MinorGroupId.Value;
                var mainName = _api.GetStockGroupMainNameByMinorId(minorGroupId);
                var subName = _api.GetStockGroupSubNameByMinorId(minorGroupId);

                if ((mainName == "RAW" || mainName == "FINISHED" || mainName == "WHOLESALE" || mainName == "PACKAGING") &&
                    subName != "SALE")
                {
                    productList.Add(product);
                }
            }

            //Generate Inventory
            foreach (var product in productList)
            {
                ProductInventory newInventory = new ProductInventory();

                newInventory.PeriodId = period.Id;
                newInventory.ProductCode = product.Code;

                //Opening, OpeningVal
                if (_context.GetMPSContext().ProductInventory.Any(a => a.ProductCode.Equals(product.Code) && a.PeriodId.Value == prvPeriodID))
                {
                    ProductInventory prvInven = _context.GetMPSContext().ProductInventory.Where(a => a.ProductCode.Equals(product.Code) && a.PeriodId.Value == prvPeriodID).First();

                    newInventory.Opening = prvInven.Closing;
                    newInventory.OpeningValue = prvInven.ClosingValue;
                }
                else
                {
                    newInventory.Opening = 0;
                    newInventory.OpeningValue = 0;
                }

                //_logger.WriteTestLog("[" + newInventory.ProductCode + "] Opening : " + newInventory.Opening + ", " + newInventory.OpeningValue);

                //StockCount, Closing
                if (_context.GetMPSContext().StockCount.Any(w => w.PeriodId.Equals(period.Id) && w.ProductCode.Equals(product.Code)))
                {
                    StockCount stockCount = _context.GetMPSContext().StockCount.Where(w => w.PeriodId.Equals(period.Id) && w.ProductCode.Equals(product.Code)).First();

                    newInventory.StockCount = stockCount.Qty;
                    newInventory.Closing = stockCount.Qty;
                }
                else
                {
                    newInventory.StockCount = 0;
                    newInventory.Closing = 0;
                }

                //_logger.WriteTestLog("Stock Count : " + newInventory.StockCount + ", Closing " + newInventory.Closing);

                /*
                var totalAmountList = taskList.GroupBy(g => new { Division = g.Division, Category = g.Category })
                .Select(s => new {
                    totalTimeMinutes = s.Sum(b => b.Duration.Value.TotalMinutes),
                    category = _context.GetTaskContext().Category.Where(w => w.Code.Equals(s.Key.Category)).First().Rate,
                    totalAmount = Math.Round(_context.GetTaskContext().Category.Where(w => w.Code.Equals(s.Key.Category)).First().Rate.Value * (decimal)(s.Sum(b => b.Duration.Value.TotalMinutes) / 60), 2),
                    divisionCode = s.Key.Division
                }).ToList();
                */

                //Inventory Purchase
                if (purchaseGroup.Any(w => w.productCode.Equals(product.Code)))
                {
                    var purchaseItem = purchaseGroup.Where(w => w.productCode.Equals(product.Code)).First();

                    newInventory.Purchased = purchaseItem.purchaseQty;
                }
                else
                {
                    newInventory.Purchased = 0;
                }

                //Inventory Purchase Claim
                if (purClaimGroup.Any(w => w.productCode.Equals(product.Code)))
                {
                    var pCliamItem = purClaimGroup.Where(w => w.productCode.Equals(product.Code)).First();

                    newInventory.PClaimed = pCliamItem.claimQty;
                    newInventory.PClaimedValue = pCliamItem.purchaseCost;

                    if (pCliamItem.returned)
                    {
                        newInventory.RClaimed = pCliamItem.claimQty;
                    }
                }
                else
                {
                    newInventory.PClaimed = 0;
                    newInventory.PClaimedValue = 0;
                    newInventory.RClaimed = 0;
                }

                //_logger.WriteTestLog("Purchase : " + newInventory.Purchased + ", PClaim " + newInventory.PClaimed + ", " + newInventory.PClaimedValue);

                //Inventory Sale
                if (saleGroup.Any(w => w.productCode.Equals(product.Code)))
                {
                    var saleItem = saleGroup.Where(w => w.productCode.Equals(product.Code)).First();

                    newInventory.Sold = saleItem.saleQty;
                }
                else
                {
                    newInventory.Sold = 0;
                }

                //Inventory sClaimItem
                if (saleClaimGroup.Any(w => w.productCode.Equals(product.Code)))
                {
                    var sClaimItem = saleClaimGroup.Where(w => w.productCode.Equals(product.Code)).First();

                    newInventory.SClaimed = sClaimItem.saleQty;
                }
                else
                {
                    newInventory.SClaimed = 0;
                }

                //_logger.WriteTestLog("Sold : " + newInventory.Sold + ", PClaim " + newInventory.SClaimed);

                //Inventory ucWasted
                if (wasteGroup.Any(w => w.productCode.Equals(product.Code)))
                {
                    var ucWasted = wasteGroup.Where(w => w.productCode.Equals(product.Code)).First();

                    newInventory.UcWasted = ucWasted.wasteQty;
                }
                else
                {
                    newInventory.UcWasted = 0;
                }

                //Inventory transFrom
                if (transferFromGroup.Any(w => w.fromProductCode.Equals(product.Code)))
                {
                    var transFromItem = transferFromGroup.Where(w => w.fromProductCode.Equals(product.Code)).First();

                    newInventory.TransFrom = transFromItem.fromQty;
                }
                else
                {
                    newInventory.TransFrom = 0;
                }

                //Inventory transTo
                if (transferToGroup.Any(w => w.toProductCode.Equals(product.Code)))
                {
                    var transToItem = transferToGroup.Where(w => w.toProductCode.Equals(product.Code)).First();

                    newInventory.TransTo = transToItem.toQty;
                }
                else
                {
                    newInventory.TransTo = 0;
                }

                //_logger.WriteTestLog("TransFrom : " + newInventory.TransFrom + ", To " + newInventory.TransTo);

                //Inventory packed
                if (packingGroup.Any(w => w.packingProductCode.Equals(product.Code)))
                {
                    var packedItem = packingGroup.Where(w => w.packingProductCode.Equals(product.Code)).First();

                    newInventory.Packed = packedItem.packingQty;
                }
                else
                {
                    newInventory.Packed = 0;
                }

                //Inventory PackUsed
                if (packingUsedGroup.Any(w => w.component.Equals(product.Code)))
                {
                    var packUsedItem = packingUsedGroup.Where(w => w.component.Equals(product.Code)).First();

                    newInventory.PackUsed = packUsedItem.packingUsedQty;
                }
                else
                {
                    newInventory.PackUsed = 0;
                }

                var saleUsed = 0.0;

                if (wholeSaleGroup.Any(x => x.productCode.Equals(product.Code)))
                {
                    saleUsed = wholeSaleGroup.Where(w => w.productCode.Equals(product.Code)).First().saleUsedQty;
                }

                //_logger.WriteTestLog("Packed : " + newInventory.Packed + ", PackUsed " + newInventory.PackUsed + ", SaleUsed : " + saleUsed);

                //Inventory Calculated
                newInventory.Calculated = newInventory.Opening + (newInventory.Purchased - newInventory.Sold + newInventory.SClaimed - newInventory.UcWasted - newInventory.RClaimed - newInventory.TransFrom + newInventory.TransTo - newInventory.PackUsed - saleUsed + newInventory.Packed - newInventory.PClaimed);

                newInventory.Variance = newInventory.StockCount - newInventory.Calculated;

                //_logger.WriteTestLog("Calced : " + newInventory.Calculated + ", Variance " + newInventory.Variance);

                //transferFromGroup transferToGroup packingGroup packingUsedGroup
                //Inventory Added
                var addedQty = 0.0;
                var addedValue = 0.0;

                if (purchaseGroup.Any(w => w.productCode.Equals(product.Code)))
                {
                    var purchaseAddList = purchaseGroup.Where(w => w.productCode.Equals(product.Code)).ToList();

                    foreach (var purchaseAdd in purchaseAddList)
                    {
                        addedQty = addedQty + purchaseAdd.purchaseQty.Value;
                        addedValue = addedValue + purchaseAdd.purchaseCost.Value;
                    }
                }

                if (transferToGroup.Any(w => w.toProductCode.Equals(product.Code)))
                {
                    var transToAddedList = transferToGroup.Where(w => w.toProductCode.Equals(product.Code)).ToList();

                    foreach (var transtoAdded in transToAddedList)
                    {
                        addedQty = addedQty + transtoAdded.toQty.Value;
                    }
                }

                newInventory.Added = addedQty;
                newInventory.AddedValue = addedValue;

                //Inventory Reduction 
                newInventory.Reduction = newInventory.Opening + newInventory.Added + newInventory.Packed - newInventory.Closing + newInventory.Variance;

                //Inventory Reduction Value
                if (newInventory.Reduction < newInventory.Opening)
                {
                    if (newInventory.Opening == 0)
                    {
                        newInventory.ReductionValue = 0;
                    }
                    else
                    {
                        newInventory.ReductionValue = (newInventory.OpeningValue / newInventory.Opening) * newInventory.Reduction;
                    }
                }
                else
                {
                    if (newInventory.Added == 0)
                    {
                        newInventory.ReductionValue = newInventory.OpeningValue;
                    }
                    else
                    {
                        newInventory.ReductionValue = (newInventory.Reduction - newInventory.Opening) * (newInventory.AddedValue / newInventory.Added) + newInventory.OpeningValue;
                    }
                }

                //_logger.WriteTestLog("Added : " + newInventory.Added + ", " + newInventory.AddedValue);
                //_logger.WriteTestLog("Reduction : " + newInventory.Reduction + ", " + newInventory.ReductionValue);

                //Inventory Closing Value
                if (newInventory.Closing == 0 || newInventory.Calculated == 0)
                {
                    newInventory.ClosingValue = 0;
                }
                else
                {
                    //(([OpeningValue]+[AddedValue]-[ReductionValue])+((([OpeningValue]+[AddedValue]-[ReductionValue])/([Calculated]))*([Variance])))) 
                    newInventory.ClosingValue = (newInventory.OpeningValue + newInventory.AddedValue - newInventory.ReductionValue) + (((newInventory.OpeningValue + newInventory.AddedValue - newInventory.ReductionValue) / newInventory.Calculated) * newInventory.Variance);
                }

                //_logger.WriteTestLog("Closing : " + newInventory.Closing + ", " + newInventory.ClosingValue);

                newInventory.UpdatedBy = updateBy;
                newInventory.UpdatedOn = DateTime.Now;

                _context.GetMPSContext().ProductInventory.Add(newInventory);
                _context.GetMPSContext().SaveChanges();

            }
        }

        bool IsValidStockGroup(int minorGroupId)
        {
            var mainName = _api.GetStockGroupMainNameByMinorId(minorGroupId);
            var subName = _api.GetStockGroupSubNameByMinorId(minorGroupId);

            return (mainName == "RAW" || mainName == "FINISHED" || mainName == "WHOLESALE" || mainName == "PACKAGING") &&
                   subName != "SALE";
        }

        public void UpdateInventory2(Period period)
        {
            //Product Inventory
            var qvProductInventory = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(period.Id)).ToList();

            //_logger.WriteTestLog("qvProductInventory : " + qvProductInventory.Count);

            //Product Type
            var qryProductType = (
                from mainGroup in _context.GetMPSContext().ProductMainGroup
                join subGroup in _context.GetMPSContext().ProductSubGroup on mainGroup.Id equals subGroup.MainGroupId into subGroupJoin
                from subGroup in subGroupJoin.DefaultIfEmpty()
                join minorGroup in _context.GetMPSContext().ProductMinorGroup on subGroup.Id equals minorGroup.SubGroupId into minorGroupJoin
                from minorGroup in minorGroupJoin.DefaultIfEmpty()
                join product in _context.GetMPSContext().Product on minorGroup.Id equals product.MinorGroupId into productJoin
                from product in productJoin.DefaultIfEmpty()
                orderby product.Code
                select new qryProductType
                {
                    Code = product.Code,
                    Desc = product.Desc,
                    Inactive = product.Inactive,
                    MainType = mainGroup.Type,
                    SubType = subGroup.Type,
                    MinorType = minorGroup.Type,
                    MainGroupID = mainGroup.Id,
                    SubGroupID = minorGroup.SubGroupId,
                    MinorGroupID = product.MinorGroupId,
                    Tax = product.Tax
                }
            ).ToList();

            //_logger.WriteTestLog("qryProductType : " + qryProductType.Count);

            //Sale List
            var qcSaleList = (
                from sale in _context.GetMPSContext().Sale
                join saleItem in _context.GetMPSContext().SaleItem on sale.InvoiceId equals saleItem.InvoiceId
                where saleItem.InvoicedQty != 0 && (sale.Status == "Invoice" || sale.Status == "Exported")
                      && sale.DeliveryDate.Value >= period.StartDate.Value && sale.DeliveryDate.Value <= period.EndDate.Value
                group new { sale, saleItem } by saleItem.ProductCode into saleGroup
                select new
                {
                    ProductCode = saleGroup.Key,
                    TotalQty = saleGroup.Sum(x => x.sale.Type == "PriceClaim" ? 0 : x.saleItem.InvoicedQty),
                    TotalPrice = saleGroup.Sum(x => x.saleItem.InvoicedQty * x.saleItem.Price),
                    SaleUnitPrice = saleGroup.Sum(x => x.saleItem.InvoicedQty * x.saleItem.Price) / saleGroup.Sum(x => x.sale.Type == "PriceClaim" ? 0 : x.saleItem.InvoicedQty)
                }
            ).ToList();

            //_logger.WriteTestLog("qcSaleList : " + qcSaleList.Count);

            //Repacking Labour
            var qvLabourRecTotal = (
                from productRepacking in _context.GetMPSContext().ProductRepacking
                join qcSale in qcSaleList on productRepacking.ProductCode equals qcSale.ProductCode
                where productRepacking.RepackingDate.HasValue && productRepacking.RepackingDate.Value >= period.StartDate.Value && productRepacking.RepackingDate.Value <= period.EndDate.Value
                group new { productRepacking, qcSale } by new { productRepacking.ProductCode, qcSale.TotalQty } into g
                select new
                {
                    ProductCode = g.Key.ProductCode,
                    LRSum = 0,
                    TotalQty = g.Key.TotalQty,
                    LRUS = 0
                }
            ).ToList();

            //_logger.WriteTestLog("qvLabourRecTotal : " + qvLabourRecTotal.Count);

            //Packing Labour
            var qvProductLabourValue = (
                from productPacking in _context.GetMPSContext().ProductPacking
                where productPacking.PackingDate.HasValue && productPacking.PackingDate.Value >= period.StartDate.Value && productPacking.PackingDate.Value <= period.EndDate.Value
                group productPacking by productPacking.ProductCode into g
                let labourUnitCost = _context.GetMPSContext().Expense
                    .Where(exp => exp.PeriodId.HasValue && exp.PeriodId == period.Id)
                    .Sum(exp => exp.Price) /
                    g.Sum(pp => pp.LabourQty * GetTotalMinutes(pp.StartTime, pp.FinishTime))
                select new
                {
                    ProductCode = g.Key,
                    LabourUnitCost = labourUnitCost,
                    TotalQty = g.Sum(pp => pp.ProductQty),
                    TotalLabourCost = g.Sum(pp => pp.LabourQty * GetTotalMinutes(pp.StartTime, pp.FinishTime) * labourUnitCost),
                    UnitCost = g.Sum(pp => pp.ProductQty) == 0 ? 0 : g.Sum(pp => pp.LabourQty * GetTotalMinutes(pp.StartTime, pp.FinishTime) * labourUnitCost) / g.Sum(pp => pp.ProductQty)
                }
            ).OrderBy(g => g.ProductCode).ToList();

            //_logger.WriteTestLog("qvProductLabourValue : " + qvProductLabourValue.Count);

            //Pure Recipe List
            var qryPureRecipe = (
                from productRecipe in _context.GetMPSContext().ProductRecipe
                join productType in qryProductType
                on productRecipe.Component equals productType.Code
                where (!string.IsNullOrEmpty(productType.MainType)) && (!new[] { "EXPENSE", "FREIGHT" }.Contains(productType.MainType))
                select productRecipe
            ).ToList();

            //_logger.WriteTestLog("qryPureRecipe : " + qryPureRecipe.Count);

            //Stock Code Translate
            var qryStockCodeTranslate = (
                from productType in qryProductType
                join pureRecipe in qryPureRecipe
                on productType.Code equals pureRecipe.ProductCode into recipeJoin
                from pureRecipe in recipeJoin.DefaultIfEmpty()
                where (!string.IsNullOrEmpty(productType.Code)) && (pureRecipe != null && !string.IsNullOrEmpty(pureRecipe.Component))
                group new { productType, pureRecipe } by new { productType.Code, StockCode = (productType.SubType == "SALE") ? pureRecipe.Component : pureRecipe.ProductCode } into g
                select new
                {
                    SaleCode = g.Key.Code,
                    StockCode = g.Key.StockCode
                }
            ).ToList();

            //_logger.WriteTestLog("qryStockCodeTranslate : " + qryStockCodeTranslate.Count);

            //Sub Recipe Value
            var qvRecipeValueSubList = (
                from productRecipe in _context.GetMPSContext().ProductRecipe
                join compType in qryProductType on productRecipe.Component equals compType.Code into compTypeJoin
                from compType in compTypeJoin.DefaultIfEmpty()
                join productInventory in qvProductInventory on productRecipe.Component equals productInventory.ProductCode into inventoryJoin
                from productInventory in inventoryJoin.DefaultIfEmpty()
                join productLabourValue in qvProductLabourValue on productRecipe.ProductCode equals productLabourValue.ProductCode into labourValueJoin
                from productLabourValue in labourValueJoin.DefaultIfEmpty()
                join labourRecTotal in qvLabourRecTotal on productRecipe.ProductCode equals labourRecTotal.ProductCode into labourRecTotalJoin
                from labourRecTotal in labourRecTotalJoin.DefaultIfEmpty()
                where (!string.IsNullOrEmpty(productInventory.ProductCode)) && (!string.IsNullOrEmpty(productRecipe.Component)) && productRecipe.Embeded.HasValue && productRecipe.Embeded == false
                group new { productRecipe, compType, productInventory, productLabourValue, labourRecTotal } by productRecipe.ProductCode into g
                select new
                {
                    ProductCode = g.Key,
                    PC = g.Sum(x => x.productInventory != null && x.compType != null && (x.productInventory.Reduction == 0 || x.compType.MainType != "PACKAGING") ? 0 : (x.productRecipe.Qty ?? 0) * x.productInventory.ReductionValue / x.productInventory.Reduction),
                    RC1 = g.Sum(x => x.productInventory != null && x.compType != null && (x.productInventory.Reduction == 0 || x.compType.MainType != "RAW") ? 0 : (x.productRecipe.Qty ?? 0) * x.productInventory.ReductionValue / x.productInventory.PackUsed),
                    LC = g.Sum(x => x.productLabourValue != null ? x.productLabourValue.UnitCost : 0),
                    TLR = g.Sum(x => x.labourRecTotal != null ? x.labourRecTotal.LRSum : 0),
                    LRUSold = g.Sum(x => x.labourRecTotal != null ? x.labourRecTotal.LRUS : 0),
                    PF = g.Sum(x => (x.productRecipe.Component != "ECPAF") ? 0 : (x.productRecipe.Qty ?? 0)),
                    HFRate = g.Sum(x => (x.productRecipe.Component != "ECHAF") ? 0 : (x.productRecipe.Qty ?? 0)) / 100
                }
            ).OrderBy(x => x.ProductCode).ToList();

            //_logger.WriteTestLog("qvRecipeValueSubList : " + qvRecipeValueSubList.Count);

            //Main Recipe Value
            var qvRecipeValueMain = (
                from qvRecipeValueSub in qvRecipeValueSubList
                join qryProductTypeJoin in qryProductType on qvRecipeValueSub.ProductCode equals qryProductTypeJoin.Code
                join qryStockCodeTranslateJoin in qryStockCodeTranslate on qvRecipeValueSub.ProductCode equals qryStockCodeTranslateJoin.SaleCode
                join qvProductInventoryJoin in qvProductInventory on qryStockCodeTranslateJoin.StockCode equals qvProductInventoryJoin.ProductCode
                let RC = qvRecipeValueSub.RC1 + ((qvProductInventoryJoin.Reduction == 0) || (qryProductTypeJoin.MainType != "WHOLESALE") ? 0 : qvProductInventoryJoin.ReductionValue / qvProductInventoryJoin.Reduction)
                let HF = qryProductTypeJoin.MainGroupID == 4 ? qvRecipeValueSub.PC : (RC * qvRecipeValueSub.HFRate)
                select new 
                {
                    PeriodId = qvProductInventoryJoin.PeriodId,
                    ProductCode = qvRecipeValueSub.ProductCode,
                    MainType = qryProductTypeJoin.MainType,
                    RC,
                    PF = qvRecipeValueSub.PF,
                    HF,
                    PC = qvRecipeValueSub.PC,
                    LC = qvRecipeValueSub.LC,
                    TLR = qvRecipeValueSub.TLR,
                    LRUS = qvRecipeValueSub.LRUSold,
                    PackedUnitValue = (qryProductTypeJoin.MainGroupID == 3 ? 0 : HF) + qvRecipeValueSub.PF + RC + ((qryProductTypeJoin.Code == "IFCAR20KGHORBAG" || qryProductTypeJoin.Code == "IFCAR20KGSECBAG") ? qvProductInventoryJoin.ReductionValue / qvProductInventoryJoin.Reduction : 0)
                }
            ).OrderBy(q => q.ProductCode).ToList();

            //_logger.WriteTestLog("qvRecipeValueMain : " + qvRecipeValueMain.Count);

            //Recipe List
            var qpRecipe = (
                from productRecipe in _context.GetMPSContext().ProductRecipe
                join productType in qryProductType on productRecipe.ProductCode equals productType.Code
                join componentType in qryProductType on productRecipe.Component equals componentType.Code
                where (!string.IsNullOrEmpty(productRecipe.Component)) && productRecipe.Embeded.HasValue && productRecipe.Embeded == false
                orderby productRecipe.ProductCode, productRecipe.Component
                select new
                {
                    ProductCode = productRecipe.ProductCode,
                    Desc = productType.Desc,
                    Component = productRecipe.Component,
                    CompDesc = componentType.Desc,
                    Qty = productRecipe.Qty
                }
            ).ToList();

            //_logger.WriteTestLog("qpRecipe : " + qpRecipe.Count);

            //Recipe Cal Hoss
            var qpRecipeCalHoss = (
                from productInventory in qvProductInventory
                join qpRecipeItem in qpRecipe on productInventory.ProductCode equals qpRecipeItem.Component into qpRecipeJoin
                from qpRecipeItem in qpRecipeJoin.DefaultIfEmpty()
                select new RecipeCalHossModel
                {
                    PeriodId = productInventory.PeriodId,
                    ProductCode = qpRecipeItem?.ProductCode,
                    Desc = qpRecipeItem?.Desc,
                    Component = qpRecipeItem?.Component,
                    Qty = qpRecipeItem?.Qty ?? 0,
                    UnitCost = productInventory.Closing == 0 ? 0 : productInventory.ClosingValue / productInventory.Closing,
                    Cost = (productInventory.Closing == 0 ? 0 : productInventory.ClosingValue / productInventory.Closing) * (qpRecipeItem?.Qty ?? 0) // Add null-check here and provide a default value (0 in this case)
                }
            ).ToList();

            //_logger.WriteTestLog("qpRecipeCalHoss : " + qpRecipeCalHoss.Count);

            //Recipe Cal Hoss Cross Tab
            var qpRecipeCalHoss_Crosstab = qpRecipeCalHoss
                .GroupBy(h => new { h.PeriodId, h.ProductCode, h.Desc })
                .Select(g => new
                {
                    PeriodId = g.Key.PeriodId,
                    ProductCode = g.Key.ProductCode,
                    Desc = g.Key.Desc,
                    Total = g.Sum(h => h.Cost)
                })
                .ToList();

            //_logger.WriteTestLog("qpRecipeCalHoss_Crosstab : " + qpRecipeCalHoss_Crosstab.Count);

            //Temp Inventory
            var qvCreateInventory2 = (
                from crosstab in qpRecipeCalHoss_Crosstab
                join inventory in qvProductInventory on crosstab.ProductCode equals inventory.ProductCode
                join recipeValue in qvRecipeValueMain on inventory.ProductCode equals recipeValue.ProductCode
                where recipeValue.MainType == "FINISHED"
                select new QvCreateInventory2
                {
                    PeriodId = inventory.PeriodId,
                    ProductCode = inventory.ProductCode,
                    Added2 = inventory.Added + inventory.Packed,
                    AddedValue2 = (inventory.Added + inventory.Packed) != 0 && recipeValue.PackedUnitValue.HasValue && !double.IsNaN(recipeValue.PackedUnitValue.Value)
                        ? (inventory.Added + inventory.Packed) * recipeValue.PackedUnitValue.Value
                        : 0,
                    ReductionValue2 = inventory.Reduction < inventory.Opening
                        ? inventory.Opening == 0
                            ? 0
                            : inventory.OpeningValue / inventory.Opening * inventory.Reduction
                        : inventory.OpeningValue + (inventory.Added + inventory.Packed) == 0
                            ? 0
                            : (inventory.Reduction - inventory.Opening) * (((inventory.Added + inventory.Packed) * recipeValue.PackedUnitValue.Value) / (inventory.Added + inventory.Packed)),
                    ClosingValue2 = crosstab.Total * inventory.Closing,
                    PackedValue2 = recipeValue.PackedUnitValue.HasValue && !double.IsNaN(recipeValue.PackedUnitValue.Value) 
                        ? inventory.Packed * recipeValue.PackedUnitValue.Value
                        : 0
                }
            ).ToList();

            //Update Inentory
            var updateInentory = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(period.Id)).ToList();

            foreach(var updateInventory in updateInentory)
            {
                if(qvCreateInventory2.Any(x => x.PeriodId.HasValue && x.PeriodId == updateInventory.PeriodId && (!string.IsNullOrEmpty(x.ProductCode)) && x.ProductCode == updateInventory.ProductCode))
                {
                    var calcedInventory = qvCreateInventory2.Where(x => x.PeriodId.HasValue && x.PeriodId == updateInventory.PeriodId && (!string.IsNullOrEmpty(x.ProductCode)) && x.ProductCode == updateInventory.ProductCode).First();

                    if(calcedInventory.Added2.HasValue)
                    {
                        updateInventory.Added = updateInventory.Added + calcedInventory.Added2;
                    }
                    else
                    {
                        updateInventory.Added = 0;
                    }

                    if (calcedInventory.AddedValue2.HasValue)
                    {
                        updateInventory.AddedValue = updateInventory.AddedValue + calcedInventory.AddedValue2;
                    }
                    else
                    {
                        updateInventory.AddedValue = 0;
                    }

                    if (calcedInventory.ReductionValue2.HasValue)
                    {
                        updateInventory.ReductionValue = updateInventory.ReductionValue + calcedInventory.ReductionValue2;
                    }
                    else
                    {
                        updateInventory.ReductionValue = 0;
                    }

                    if (double.IsNaN(updateInventory.ReductionValue.Value))
                    {
                        updateInventory.ReductionValue = 0;
                    }

                    if (calcedInventory.ClosingValue2.HasValue)
                    {
                        updateInventory.ClosingValue = updateInventory.ClosingValue + calcedInventory.ClosingValue2;
                    }
                    else
                    {
                        updateInventory.ClosingValue = 0;
                    }

                    if (calcedInventory.PackedValue2 != null && calcedInventory.PackedValue2.HasValue)
                    {
                        updateInventory.PackedValue = calcedInventory.PackedValue2;
                    }
                    else
                    {
                        updateInventory.PackedValue = 0;
                    }

                    _context.GetMPSContext().ProductInventory.Update(updateInventory);
                    _context.GetMPSContext().SaveChanges();
                }
            }

            //_logger.WriteTestLog("Test Completed");

        }

        public void UpdateYTDInventory(Period period)
        {
            //Product Inventory
            var qvProductInventory = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(period.Id)).ToList();

            //Prev ProductInventory
            var qvPrevProductInventory = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(period.Id-1)).ToList();

            var qvUpdateInventory4 = qvProductInventory
                .GroupJoin(
                    qvPrevProductInventory,
                    p => p.ProductCode,
                    p0 => p0.ProductCode,
                    (p, p0Group) => new { P = p, P0Group = p0Group })
                .SelectMany(
                    x => x.P0Group.DefaultIfEmpty(),
                    (x, p0) => new { P = x.P, P0 = p0 })
                .Select(x => new
                {
                    ProductCode = x.P.ProductCode,
                    YTDWaste = (129 < x.P0?.PeriodId && x.P0.PeriodId < period.Id)
                        ? (x.P.UcWasted ?? 0) + (x.P0?.Ytdwaste ?? 0)
                        : 0,
                    YTDWasteValue = (129 < x.P0?.PeriodId && x.P0.PeriodId < period.Id)
                        ? (x.P0?.YtdwasteValue ?? 0) + (x.P.UcWasted ?? 0) * ((x.P.Reduction != 0) ? (x.P.ReductionValue / x.P.Reduction) : 0)
                        : 0,
                    YTDClaim = (129 < x.P0?.PeriodId && x.P0.PeriodId < period.Id)
                        ? (x.P0?.Ytdclaim ?? 0) + (x.P.Nc ?? 0)
                        : 0,
                    YTDClaimValue = (129 < x.P0?.PeriodId && x.P0.PeriodId < period.Id)
                        ? (x.P0?.YtdclaimValue ?? 0) + (x.P.Ncv ?? 0)
                        : 0,
                    YTD = (129 < x.P0?.PeriodId && x.P0.PeriodId < period.Id)
                        ? (x.P0?.Ytd ?? 0) + (x.P.Variance ?? 0)
                        : 0,
                    YTDValue = (129 < x.P0?.PeriodId && x.P0.PeriodId < period.Id)
                        ? (x.P0?.Ytdvalue ?? 0) + ((x.P.Closing != 0) ? (x.P.ClosingValue / x.P.Closing) : 0) * (x.P.Variance ?? 0)
                        : 0
                });

            // Update the qvProductInventory records with the calculated values
            foreach (var item in qvUpdateInventory4)
            {
                if(qvProductInventory.Any(x=>x.ProductCode == item.ProductCode))
                {
                    var inventory = qvProductInventory.Where(x => x.ProductCode == item.ProductCode).First();

                    inventory.Ytdwaste = item.YTDWaste;
                    inventory.YtdwasteValue = item.YTDWasteValue;
                    inventory.Ytdclaim = item.YTDClaim;
                    inventory.YtdclaimValue = item.YTDClaimValue;
                    inventory.Ytd = item.YTD;
                    inventory.Ytdvalue = item.YTDValue;

                    //_logger.WriteTestLog("Inventory Updated : " + item.ProductCode);

                    _context.GetMPSContext().ProductInventory.Update(inventory);
                }
            }

            _context.GetMPSContext().SaveChanges();
        }

        public void InsertProductCont(Period period)
        {
            //Product Inventory
            var qvProductInventory = _context.GetMPSContext().ProductInventory.Where(a => a.PeriodId.Equals(period.Id)).ToList();

            //Sale List
            var qcSaleList = (
                from sale in _context.GetMPSContext().Sale
                join saleItem in _context.GetMPSContext().SaleItem on sale.InvoiceId equals saleItem.InvoiceId
                where saleItem.InvoicedQty != 0 && (sale.Status == "Invoice" || sale.Status == "Exported")
                      && sale.DeliveryDate.Value >= period.StartDate.Value && sale.DeliveryDate.Value <= period.EndDate.Value
                group new { sale, saleItem } by saleItem.ProductCode into saleGroup
                select new
                {
                    ProductCode = saleGroup.Key,
                    TotalQty = saleGroup.Sum(x => x.sale.Type == "PriceClaim" ? 0 : x.saleItem.InvoicedQty),
                    TotalPrice = saleGroup.Sum(x => x.saleItem.InvoicedQty * x.saleItem.Price),
                    SaleUnitPrice = saleGroup.Sum(x => x.saleItem.InvoicedQty * x.saleItem.Price) / saleGroup.Sum(x => x.sale.Type == "PriceClaim" ? 0 : x.saleItem.InvoicedQty)
                }
            ).ToList();

            //Repacking Labour
            var qvLabourRecTotal = (
                from productRepacking in _context.GetMPSContext().ProductRepacking
                join qcSale in qcSaleList on productRepacking.ProductCode equals qcSale.ProductCode
                where productRepacking.RepackingDate.HasValue && productRepacking.RepackingDate.Value >= period.StartDate.Value && productRepacking.RepackingDate.Value <= period.EndDate.Value
                group new { productRepacking, qcSale } by new { productRepacking.ProductCode, qcSale.TotalQty } into g
                select new
                {
                    ProductCode = g.Key.ProductCode,
                    LRSum = g.Sum(x => x.productRepacking.LabourCost),
                    TotalQty = g.Key.TotalQty,
                    LRUS = g.Key.TotalQty == 0 ? 0 : g.Sum(x => x.productRepacking.LabourCost) / g.Key.TotalQty
                }
            ).ToList();

            //_logger.WriteTestLog("qvLabourRecTotal : " + qvLabourRecTotal.Count);

            //Packing Labour
            var qvProductLabourValue = (
                from productPacking in _context.GetMPSContext().ProductPacking
                where productPacking.PackingDate.HasValue && productPacking.PackingDate.Value >= period.StartDate.Value && productPacking.PackingDate.Value <= period.EndDate.Value
                group productPacking by productPacking.ProductCode into g
                let labourUnitCost = _context.GetMPSContext().Expense
                    .Where(exp => exp.PeriodId.HasValue && exp.PeriodId == period.Id)
                    .Sum(exp => exp.Price) /
                    g.Sum(pp => pp.LabourQty * GetTotalMinutes(pp.StartTime, pp.FinishTime))
                select new
                {
                    ProductCode = g.Key,
                    LabourUnitCost = labourUnitCost,
                    TotalQty = g.Sum(pp => pp.ProductQty),
                    TotalLabourCost = g.Sum(pp => pp.LabourQty * GetTotalMinutes(pp.StartTime, pp.FinishTime) * labourUnitCost),
                    UnitCost = g.Sum(pp => pp.ProductQty) == 0 ? 0 : g.Sum(pp => pp.LabourQty * GetTotalMinutes(pp.StartTime, pp.FinishTime) * labourUnitCost) / g.Sum(pp => pp.ProductQty)
                }
            ).OrderBy(g => g.ProductCode).ToList();

            //Product Type
            var qryProductType = (
                from mainGroup in _context.GetMPSContext().ProductMainGroup
                join subGroup in _context.GetMPSContext().ProductSubGroup on mainGroup.Id equals subGroup.MainGroupId into subGroupJoin
                from subGroup in subGroupJoin.DefaultIfEmpty()
                join minorGroup in _context.GetMPSContext().ProductMinorGroup on subGroup.Id equals minorGroup.SubGroupId into minorGroupJoin
                from minorGroup in minorGroupJoin.DefaultIfEmpty()
                join product in _context.GetMPSContext().Product on minorGroup.Id equals product.MinorGroupId into productJoin
                from product in productJoin.DefaultIfEmpty()
                orderby product.Code
                select new qryProductType
                {
                    Code = product.Code,
                    Desc = product.Desc,
                    Inactive = product.Inactive,
                    MainType = mainGroup.Type,
                    SubType = subGroup.Type,
                    MinorType = minorGroup.Type,
                    MainGroupID = mainGroup.Id,
                    SubGroupID = minorGroup.SubGroupId,
                    MinorGroupID = product.MinorGroupId,
                    Tax = product.Tax
                }
            ).ToList();

            //Recipe List
            var qpRecipe = (
                from productRecipe in _context.GetMPSContext().ProductRecipe
                join productType in qryProductType on productRecipe.ProductCode equals productType.Code
                join componentType in qryProductType on productRecipe.Component equals componentType.Code
                where (!string.IsNullOrEmpty(productRecipe.Component)) && productRecipe.Embeded.HasValue && productRecipe.Embeded == false
                orderby productRecipe.ProductCode, productRecipe.Component
                select new
                {
                    ProductCode = productRecipe.ProductCode,
                    Desc = productType.Desc,
                    Component = productRecipe.Component,
                    CompDesc = componentType.Desc,
                    Qty = productRecipe.Qty
                }
            ).ToList();

            //Pure Recipe List
            var qryPureRecipe = (
                from productRecipe in _context.GetMPSContext().ProductRecipe
                join productType in qryProductType
                on productRecipe.Component equals productType.Code
                where (!string.IsNullOrEmpty(productType.MainType)) && (!new[] { "EXPENSE", "FREIGHT" }.Contains(productType.MainType))
                select productRecipe
            ).ToList();

            //Stock Code Translate
            var qryStockCodeTranslate = (
                from productType in qryProductType
                join pureRecipe in qryPureRecipe
                on productType.Code equals pureRecipe.ProductCode into recipeJoin
                from pureRecipe in recipeJoin.DefaultIfEmpty()
                where (!string.IsNullOrEmpty(productType.Code)) && (pureRecipe != null && !string.IsNullOrEmpty(pureRecipe.Component))
                group new { productType, pureRecipe } by new { productType.Code, StockCode = (productType.SubType == "SALE") ? pureRecipe.Component : pureRecipe.ProductCode } into g
                select new
                {
                    SaleCode = g.Key.Code,
                    StockCode = g.Key.StockCode
                }
            ).ToList();

            //Sale com List
            var qvSaleCom = (from sale in _context.GetMPSContext().Sale
                          join saleItem in _context.GetMPSContext().SaleItem on sale.InvoiceId equals saleItem.InvoiceId
                          join address in _context.GetMPSContext().Address on sale.ShippingAddress equals address.Id
                          join productType in qryProductType on saleItem.ProductCode equals productType.Code
                          where saleItem.InvoicedQty != 0 &&
                                (!string.IsNullOrEmpty(sale.Status)) &&
                                (sale.Status == "Invoice" || sale.Status == "Exported") &&
                                sale.DeliveryDate.Value >= period.StartDate.Value && sale.DeliveryDate.Value <= period.EndDate.Value
                          group new { sale, saleItem, address, productType } by new
                          {
                              productType.MainGroupID,
                              saleItem.ProductCode,
                              productType.Desc,
                              address.State,
                              sale.Company
                          } into g
                          select new
                          {
                              MainGroupID = g.Key.MainGroupID,
                              ProductCode = g.Key.ProductCode,
                              Desc = g.Key.Desc,
                              State = g.Key.State,
                              Company = g.Key.Company,
                              TotalSold = g.Sum(x => x.sale.Status == "PriceClaim" ? 0 : x.saleItem.InvoicedQty),
                              TotalPrice = g.Sum(x => x.saleItem.InvoicedQty * x.saleItem.Price),
                              SaleUnitPrice = g.Sum(x => x.sale.Status == "PriceClaim" ? 0 : x.saleItem.InvoicedQty) > 0
                                  ? g.Sum(x => x.saleItem.InvoicedQty * x.saleItem.Price) / g.Sum(x => x.sale.Status == "PriceClaim" ? 0 : x.saleItem.InvoicedQty)
                                  : 0
                          }).OrderBy(x => x.ProductCode)
                        .ThenBy(x => x.State)
                        .ThenBy(x => x.Company)
                        .ToList();

            //Recipe Calc Hoss 2
            var qpRecipeCalHoss2 = _context.GetMPSContext().ProductRecipe
                .Where(x => x.Component == "IRCARROT" || x.Component == "IRCARSECONDS")
                .Select(x => new
                {
                    x.ProductCode,
                    x.Component,
                    x.Qty
                })
                .OrderBy(x => x.ProductCode)
                .ToList();

            //Expense Cost "ELWMA"
            var expContCal8 = _context.GetMPSContext().Expense
                .Where(x=>(!string.IsNullOrEmpty(x.ExpenseCode)) && x.ExpenseCode == "ELWMA")
                .Select(x=> new
                {
                    PeriodId = x.PeriodId,
                    lg4Wage = x.Price
                })
                .ToList();

            //Expense Cost "ELWCL"
            var expContCal7 = _context.GetMPSContext().Expense
                .Where(x => (!string.IsNullOrEmpty(x.ExpenseCode)) && x.ExpenseCode == "ELWCL")
                .Select(x => new
                {
                    PeriodId = x.PeriodId,
                    lg3Wage = x.Price
                })
                .ToList();

            //Expense Cost "ELPPK"
            var expContCal6 = _context.GetMPSContext().Expense
                .Where(x => (!string.IsNullOrEmpty(x.ExpenseCode)) && x.ExpenseCode == "ELPPK")
                .Select(x => new
                {
                    PeriodId = x.PeriodId,
                    lg2Wage = x.Price
                })
                .ToList();

            //Expense Cost "ELPPC"
            var expContCal5 = _context.GetMPSContext().Expense
                .Where(x => (!string.IsNullOrEmpty(x.ExpenseCode)) && x.ExpenseCode == "ELPPC")
                .Select(x => new
                {
                    PeriodId = x.PeriodId,
                    lg1Wage = x.Price
                })
                .ToList();

            //Expense Cost "ELPPP"
            var expContCal4 = _context.GetMPSContext().Expense
                .Where(x => (!string.IsNullOrEmpty(x.ExpenseCode)) && x.ExpenseCode == "ELPPP")
                .Select(x => new
                {
                    PeriodId = x.PeriodId,
                    lppWage = x.Price
                })
                .ToList();

            //Expense Cost "ELDIR"
            var expContCal3 = _context.GetMPSContext().Expense
                .Where(x => (!string.IsNullOrEmpty(x.ExpenseCode)) && x.ExpenseCode == "ELDIR")
                .Select(x => new
                {
                    PeriodId = x.PeriodId,
                    ldpWage = x.Price
                })
                .ToList();

            var expContCal38 = (from exp3 in expContCal3
                          join exp4 in expContCal4 on exp3.PeriodId equals exp4.PeriodId
                          join exp5 in expContCal5 on exp4.PeriodId equals exp5.PeriodId
                          join exp6 in expContCal6 on exp5.PeriodId equals exp6.PeriodId
                          join exp7 in expContCal7 on exp6.PeriodId equals exp7.PeriodId
                          join exp8 in expContCal8 on exp7.PeriodId equals exp8.PeriodId
                          where exp3.PeriodId.HasValue && exp3.PeriodId == period.Id
                          select new 
                          {
                              LdpWage = exp3.ldpWage,
                              LppWage = exp4.lppWage,
                              LgWage = exp5.lg1Wage + exp6.lg2Wage + exp7.lg3Wage + exp8.lg4Wage,
                              PeriodId = exp3.PeriodId
                          }).ToList();


            var expContCal = (from saleCom in qvSaleCom
                              join recipe in qpRecipe on saleCom.ProductCode equals recipe.ProductCode into recipeJoin
                              from recipe in recipeJoin.DefaultIfEmpty()
                              join recipeCalHoss2 in qpRecipeCalHoss2 on saleCom.ProductCode equals recipeCalHoss2.ProductCode into recipeCalHoss2Join
                              from recipeCalHoss2 in recipeCalHoss2Join.DefaultIfEmpty()
                              where (!string.IsNullOrEmpty(recipe?.ProductCode)) && (recipe?.Component == "ELPPK" || recipe?.Component == "ELDIR")
                              group new { saleCom, recipeCalHoss2, recipe } by new
                              {
                                  saleCom.ProductCode,
                                  LabourFactor = saleCom.ProductCode == "IFCAR500GMCRAX24" ? 2 : 1,
                                  saleCom.TotalSold,
                                  recipeCalHoss2?.Component,
                                  recipeCalHoss2?.Qty,
                                  RecipeComp = recipe?.Component,
                                  saleCom.Company
                              } into g
                              let AG = g.Sum(x => x.saleCom.TotalSold * g.Key.LabourFactor * (g.Key.Qty ?? 0))
                              let ADP = g.Key.RecipeComp == "ELDIR" ? AG : 0
                              select new
                              {
                                  ProductCode = g.Key.ProductCode,
                                  LabourFactor = g.Key.LabourFactor,
                                  SaleQty = g.Key.TotalSold,
                                  Component = g.Key.RecipeComp,
                                  RCarrotQty = g.Key.Qty ?? 0,
                                  ADP = ADP,
                                  APP = AG - ADP,
                                  AG = AG,
                                  ExpCode = g.Key.Component
                              }).ToList();

            var expContCal2 = expContCal
                .GroupBy(e => 1)
                /*
                .GroupBy(e => new
                {
                    e.ADP,
                    e.APP,
                    e.AG
                })
                */
                .Select(g => new
                {
                    SADP = g.Sum(e => e.ADP),
                    SAPP = g.Sum(e => e.APP),
                    SAG = g.Sum(e => e.AG)
                })
                .ToList();
            
            var expContCal9Data = (
                from e2 in expContCal2
                from e in expContCal
                from e38 in expContCal38
                let LDP = e.ADP == 0 ? 0 : (e2.SADP == 0 ? 0 : e38.LdpWage / e2.SADP) * e.RCarrotQty * e.LabourFactor
                let LPP = e.APP == 0 ? 0 : (e2.SAPP == 0 ? 0 : e38.LppWage / e2.SAPP) * e.RCarrotQty * e.LabourFactor
                select new
                {
                    MyProd = e.ProductCode,
                    LDP = LDP,
                    LPP = LPP,
                    LP = LDP + LPP,
                    LG = e.AG == 0 ? 0 : (e2.SAG == 0 ? 0 : e38.LgWage / e2.SAG) * e.RCarrotQty * e.LabourFactor,
                    PeriodId = e38.PeriodId
                })
                .ToList();

            //Sub Recipe Value
            var qvRecipeValueSubList = (
                from productRecipe in _context.GetMPSContext().ProductRecipe
                join compType in qryProductType on productRecipe.Component equals compType.Code into compTypeJoin
                from compType in compTypeJoin.DefaultIfEmpty()
                join productInventory in qvProductInventory on productRecipe.Component equals productInventory.ProductCode into inventoryJoin
                from productInventory in inventoryJoin.DefaultIfEmpty()
                join productLabourValue in qvProductLabourValue on productRecipe.ProductCode equals productLabourValue.ProductCode into labourValueJoin
                from productLabourValue in labourValueJoin.DefaultIfEmpty()
                join labourRecTotal in qvLabourRecTotal on productRecipe.ProductCode equals labourRecTotal.ProductCode into labourRecTotalJoin
                from labourRecTotal in labourRecTotalJoin.DefaultIfEmpty()
                where (!string.IsNullOrEmpty(productInventory.ProductCode)) && (!string.IsNullOrEmpty(productRecipe.Component)) && productRecipe.Embeded.HasValue && productRecipe.Embeded == false
                group new { productRecipe, compType, productInventory, productLabourValue, labourRecTotal } by productRecipe.ProductCode into g
                select new
                {
                    ProductCode = g.Key,
                    PC = g.Sum(x => x.productInventory != null && x.compType != null && (x.productInventory.Reduction == 0 || x.compType.MainType != "PACKAGING") ? 0 : (x.productRecipe.Qty ?? 0) * x.productInventory.ReductionValue / x.productInventory.Reduction),
                    RC1 = g.Sum(x => x.productInventory != null && x.compType != null && (x.productInventory.Reduction == 0 || x.compType.MainType != "RAW") ? 0 : (x.productRecipe.Qty ?? 0) * x.productInventory.ReductionValue / x.productInventory.PackUsed),
                    LC = g.Sum(x => x.productLabourValue != null ? x.productLabourValue.UnitCost : 0),
                    TLR = g.Sum(x => x.labourRecTotal != null ? x.labourRecTotal.LRSum : 0),
                    LRUSold = g.Sum(x => x.labourRecTotal != null ? x.labourRecTotal.LRUS : 0),
                    PF = g.Sum(x => (x.productRecipe.Component != "ECPAF") ? 0 : (x.productRecipe.Qty ?? 0)),
                    HFRate = g.Sum(x => (x.productRecipe.Component != "ECHAF") ? 0 : (x.productRecipe.Qty ?? 0)) / 100
                }
            ).OrderBy(x => x.ProductCode).ToList();

            //_logger.WriteTestLog("qvRecipeValueSubList : " + qvRecipeValueSubList.Count);

            //Main Recipe Value
            var qvRecipeValueMain = (
                from qvRecipeValueSub in qvRecipeValueSubList
                join qryProductTypeJoin in qryProductType on qvRecipeValueSub.ProductCode equals qryProductTypeJoin.Code
                join qryStockCodeTranslateJoin in qryStockCodeTranslate on qvRecipeValueSub.ProductCode equals qryStockCodeTranslateJoin.SaleCode
                join qvProductInventoryJoin in qvProductInventory on qryStockCodeTranslateJoin.StockCode equals qvProductInventoryJoin.ProductCode
                let RC = qvRecipeValueSub.RC1 + ((qvProductInventoryJoin.Reduction == 0) || (qryProductTypeJoin.MainType != "WHOLESALE") ? 0 : qvProductInventoryJoin.ReductionValue / qvProductInventoryJoin.Reduction)
                let HF = qryProductTypeJoin.MainGroupID == 4 ? qvRecipeValueSub.PC : (RC * qvRecipeValueSub.HFRate)
                select new
                {
                    PeriodId = qvProductInventoryJoin.PeriodId,
                    ProductCode = qvRecipeValueSub.ProductCode,
                    MainType = qryProductTypeJoin.MainType,
                    RC = RC,
                    PF = qvRecipeValueSub.PF,
                    HF = HF,
                    PC = qvRecipeValueSub.PC,
                    LC = qvRecipeValueSub.LC,
                    TLR = qvRecipeValueSub.TLR,
                    LRUS = qvRecipeValueSub.LRUSold,
                    PackedUnitValue = (qryProductTypeJoin.MainGroupID == 3 ? 0 : HF) + qvRecipeValueSub.PF + RC + ((qryProductTypeJoin.Code == "IFCAR20KGHORBAG" || qryProductTypeJoin.Code == "IFCAR20KGSECBAG") ? qvProductInventoryJoin.ReductionValue / qvProductInventoryJoin.Reduction : 0)
                }
            ).OrderBy(q => q.ProductCode).ToList();

            //Freight Com
            var qcFreightCom = (
                from address in _context.GetMPSContext().Address
                join sale in _context.GetMPSContext().Sale
                    on address.Id equals sale.ShippingAddress
                join purchase in _context.GetMPSContext().Purchase
                    on sale.InvoiceId equals purchase.SaleFreightReference
                join purchaseItem in _context.GetMPSContext().PurchaseItem
                    on purchase.InvoiceId equals purchaseItem.InvoiceId
                join saleItem in _context.GetMPSContext().SaleItem
                    on sale.InvoiceId equals saleItem.InvoiceId
                where purchase.Type == "FREIGHTOUT" &&
                      (!string.IsNullOrEmpty(sale.Status)) && (!string.IsNullOrEmpty(purchase.Status)) &&
                      (sale.Status == "Invoice" || sale.Status == "Exported") &&
                      (purchase.Status == "Invoice" || purchase.Status == "Exported") &&
                      sale.DeliveryDate.HasValue && sale.DeliveryDate.Value >= period.StartDate.Value && sale.DeliveryDate.Value <= period.EndDate.Value
                group new
                {
                    saleItem,
                    address,
                    sale,
                    purchase,
                    purchaseItem
                } by new
                {
                    saleItem.ProductCode,
                    address.State,
                    sale.Company
                } into g
                select new
                {
                    ProductCode = g.Key.ProductCode,
                    State = g.Key.State,
                    Company = g.Key.Company,
                    TotalQty = g.Sum(x => x.saleItem.InvoicedQty),
                    TotalFreight = g.Sum(x => x.purchaseItem.InvoicedQty * x.purchaseItem.Price * x.saleItem.FreightProportion),
                    FreightUnitCost = g.Sum(x => x.purchaseItem.InvoicedQty * x.purchaseItem.Price * x.saleItem.FreightProportion) / g.Sum(x => x.saleItem.InvoicedQty)
                }).OrderBy(x => x.ProductCode)
                .ThenBy(x => x.State)
                .ThenBy(x => x.Company)
                .ToList();

            var qvContCom = (
                from saleCom in qvSaleCom
                join freightCom in qcFreightCom
                    on new { saleCom.ProductCode, saleCom.State, saleCom.Company } equals new { freightCom.ProductCode, freightCom.State, freightCom.Company } into freightComJoin
                from freightCom in freightComJoin.DefaultIfEmpty()
                join recipeValue in qvRecipeValueMain
                    on saleCom.ProductCode equals recipeValue.ProductCode into recipeValueJoin
                from recipeValue in recipeValueJoin.DefaultIfEmpty()
                join expContCal9 in expContCal9Data
                    on saleCom.ProductCode equals expContCal9.MyProd into expContCal9Join
                from expContCal9 in expContCal9Join.DefaultIfEmpty()
                group new
                {
                    saleCom,
                    freightCom,
                    recipeValue,
                    expContCal9
                } by new
                {
                    saleCom.ProductCode,
                    saleCom.Desc,
                    saleCom.State,
                    saleCom.Company,
                    saleCom.TotalSold,
                    saleCom.TotalPrice,
                    saleCom.SaleUnitPrice,
                    FreightTotal = freightCom != null ? freightCom.TotalFreight : 0,
                    RC = recipeValue != null ? recipeValue.RC : 0,
                    PF = recipeValue != null ? recipeValue.PF : 0,
                    HF = recipeValue != null ? recipeValue.HF : 0,
                    PC = recipeValue != null ? recipeValue.PC : 0,
                    LC = recipeValue != null ? recipeValue.LC : 0,
                    LRUS = recipeValue != null ? recipeValue.LRUS : 0,
                    saleCom.MainGroupID,
                    LG = expContCal9 != null ? expContCal9.LG : 0,
                    LP = expContCal9 != null ? expContCal9.LP : 0
                } into g
                select new
                {
                    ProductCode = g.Key.ProductCode,
                    Desc = g.Key.Desc,
                    State = g.Key.State,
                    Company = g.Key.Company,
                    TotalSold = g.Key.TotalSold,
                    TotalPrice = g.Key.TotalPrice,
                    SaleUnitPrice = g.Key.SaleUnitPrice,
                    FreightTotal = g.Key.FreightTotal,
                    RC = g.Key.RC,
                    PF = g.Key.PF,
                    HF = g.Key.HF,
                    PC = g.Key.PC,
                    LC = g.Key.LC,
                    LRUS = g.Key.LRUS,
                    MainGroupId = g.Key.MainGroupID,
                    LG = g.Key.LG,
                    LP = g.Key.LP
                }).OrderBy(x => x.ProductCode)
                .ThenBy(x => x.State)
                .ThenBy(x => x.Company)
                .ToList();

            //Insert Product Cont
            var qvContComData = 
                (from q in qvContCom
                    where q.MainGroupId >= 2 && q.MainGroupId <= 3
                    orderby q.ProductCode, q.State, q.Company
                    let SaleQty = q.TotalSold
                    let SalePrice = q.SaleUnitPrice
                    let Lvy = q.Company != "COLES SUPERMARKETS" ? 0 : (0.025 * SalePrice)
                    let Cont = SaleQty * (q.SaleUnitPrice - (q.RC + q.LG + q.PC + q.LP + Lvy + (SaleQty == 0 ? 0 : q.FreightTotal / SaleQty)))
                    select new ProductCont
                    {
                        PeriodId = (short)period.Id,
                        ProductCode = q.ProductCode,
                        Desc = q.Desc,
                        State = q.State,
                        Company = q.Company,
                        SaleQty = SaleQty,
                        SalePrice = SalePrice,
                        Rc = q.RC,
                        Pf = q.PF,
                        Hf = q.HF,
                        Lr = q.LRUS,
                        Fr = SaleQty == 0 ? 0 : q.FreightTotal / SaleQty,
                        Lvy = Lvy,
                        Cont = Cont,
                        Ytdcont = Cont + _context.GetMPSContext().ProductCont
                                    .Where(pc => (short)129 < pc.PeriodId && pc.PeriodId < (short)period.Id && pc.State == q.State && pc.Company == q.Company && pc.ProductCode == q.ProductCode)
                                    .Sum(pc => pc.Cont),
                        Ytdsale = SaleQty + _context.GetMPSContext().ProductCont
                                    .Where(pc => (short)129 < pc.PeriodId && pc.PeriodId < (short)period.Id && pc.State == q.State && pc.Company == q.Company && pc.ProductCode == q.ProductCode)
                                    .Sum(pc => pc.SaleQty),
                        YtdsaleValue = SaleQty * SalePrice + _context.GetMPSContext().ProductCont
                                        .Where(pc => (short)129 < pc.PeriodId && pc.PeriodId < (short)period.Id && pc.State == q.State && pc.Company == q.Company && pc.ProductCode == q.ProductCode)
                                        .Sum(pc => pc.SaleQty * pc.SalePrice),
                        Pc = q.PC,
                        Lg = q.LG,
                        Lp = q.LP
                }).ToList();

            // Insert data into the ProductCont table
            _context.GetMPSContext().ProductCont.AddRange(qvContComData);

            // Save changes to the database
            _context.GetMPSContext().SaveChanges();

            _logger.WriteTestLog("Test Completed");

        }


        public TimeSpan CalcTime(DateTime startTime, DateTime finishTime)
        {
            TimeSpan timeDifference = new TimeSpan();

            if (startTime > finishTime)
            {
                timeDifference = startTime - finishTime;
            }
            else
            {
                timeDifference = finishTime - startTime;
            }
            
            // Calculate the time difference

            return timeDifference;
        }

        private double GetTotalMinutes(DateTime? startTime, DateTime? finishTime)
        {
            if (startTime.HasValue && finishTime.HasValue)
            {
                if (startTime.Value > finishTime.Value)
                {
                    return (startTime.Value - finishTime.Value).TotalMinutes;
                }
                else
                {
                    return (finishTime.Value - startTime.Value).TotalMinutes;
                }
            }
            return 0;
        }

        //End
    }
}
