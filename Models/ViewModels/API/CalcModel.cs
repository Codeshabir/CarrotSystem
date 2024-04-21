using CarrotSystem.Models.MPS;
using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class CalcModel
    {
        public int Id { get; set; }
        public int Qty { get; set; }
        public string Code { get; set; }
    }

    public partial class CalcViewModel
    {
        public List<ProductInventory> invenList { get; set; }
    }

    public class QvCreateInventory2
    {
        public int? PeriodId { get; set; }
        public string ProductCode { get; set; }
        public double? Added2 { get; set; }
        public double? AddedValue2 { get; set; }
        public double? ReductionValue2 { get; set; }
        public double? ClosingValue2 { get; set; }
        public double? PackedValue2 { get; set; }
    }

    public class qryProductType
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public bool? Inactive { get; set; }
        public string MainType { get; set; }
        public string SubType { get; set; }
        public string MinorType { get; set; }
        public int MainGroupID { get; set; }
        public int? SubGroupID { get; set; }
        public int? MinorGroupID { get; set; }
        public string Tax { get; set; }
    }

    public class QpRecipeCalHossCrosstab
    {
        public int? PeriodId { get; set; }
        public string ProductCode { get; set; }
        public string Desc { get; set; }
        public double? Total { get; set; }
        public Dictionary<string, double?> ComponentData { get; set; }
    }

    public class qvRecipeValueSub
    {
        public string ProductCode { get; set; }
        public double? PC { get; set; }
        public double? RC1 { get; set; }
        public double? LC { get; set; }
        public double? TLR { get; set; }
        public double? LRUSold { get; set; }
        public double? PF { get; set; }
        public double? HFRate { get; set; }
    }

    public class qvRecipeValueMain
    {
        public int? PeriodId { get; set; }
        public string ProductCode { get; set; }
        public string MainType { get; set; }
        public double? RC { get; set; }
        public double? PF { get; set; }
        public double? HF { get; set; }
        public double? PC { get; set; }
        public double? LC { get; set; }
        public double? TLR { get; set; }
        public double? LRUS { get; set; }
        public double? PackedUnitValue { get; set; }
    }

    public partial class ProductPackUsed
    {
        public int Id { get; set; }
        public double PackUsedQty { get; set; }
        public string ProductCode { get; set; }
        public string PackType { get; set; }
    }

    public partial class WholeSaleUsed
    {
        public int Id { get; set; }
        public bool Embeded { get; set; }
        public double SaleUsedQty { get; set; }
        public string ProductCode { get; set; }
    }

    public partial class RecipeCalHossModel
    {
        public int? PeriodId { get; set; }
        public string ProductCode { get; set; }
        public string Desc { get; set; }
        public string Component { get; set; }
        public double? Qty { get; set; }
        public double? UnitCost { get; set; }
        public double? Cost { get; set; }
    }

}
