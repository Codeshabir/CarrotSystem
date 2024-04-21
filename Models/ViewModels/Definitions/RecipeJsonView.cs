
namespace CarrotSystem.Models.ViewModel
{
    public partial class RecipeJsonView
    {
        public int MinorId { get; set; }
        public int ProductId { get; set; }
        public string Code { get; set; }
        public string Desc { get; set; }
    }

    public partial class NewRecipeJson
    {
        public int ProductId { get; set; }

        public string Code { get; set; }
        public string Desc { get; set; }
        public string Active { get; set; }

        public int MainGroupId { get; set; }
        public int SubGroupId { get; set; }
        public int MinorGroupId { get; set; }

        public string MainGroup { get; set; }
        public string SubGroup { get; set; }
        public string MinorGroup { get; set; }

        public string Unit { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Tax { get; set; }
        public double Weight { get; set; }
        public double Space { get; set; }
        public int LifeTime { get; set; }
        public string Comment { get; set; }
    }
}
