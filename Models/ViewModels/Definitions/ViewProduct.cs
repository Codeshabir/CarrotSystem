using CarrotSystem.Models.MPS;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewProduct
    {
        public List<ProductMainGroup> mainGroupList { get; set; }
        public List<ProductMinorGroup> minorGroupList { get; set; }
        public List<ProductSubGroup> subGroupList { get; set; }
        public List<ProductRecipe> recipeList { get; set; }
        public List<ProductUnit> unitList { get; set; }
        public List<Tax> taxList { get; set; }
        public List<RecipeJsonView> recipeItemList { get; set; }

        public ProductView productView { get; set; }
        public ProductDetailView productDetailView { get; set; }

        public List<ProductView> productViewList { get; set; }
        public List<MappingView> mappingList { get; set; }

        public string productId { get; set; }
        
        public string newGroupType { get; set; }
        public string newGroupName { get; set; }

        public string newPrefix { get; set; }
        public string newMainGroupId { get; set; }
        public string newSubGroupId { get; set; }

        public string pageTitle { get; set; }

        public bool isNew { get; set; }
    }
}
