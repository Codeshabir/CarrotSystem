using CarrotSystem.Areas.Identity.Data;
using CarrotSystem.Data;
using CarrotSystem.Models.MPS;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarrotSystem.Models.Context
{
    public partial class MPSContext : IdentityDbContext<CarrotSystemUser>
    {
        public MPSContext(DbContextOptions<MPSContext> options)
              : base(options)
        {
        }
        public virtual DbSet<Address> Address { get; set; } = null!;
        public virtual DbSet<Company> Company { get; set; } = null!;
        public virtual DbSet<CompanyType> CompanyType { get; set; } = null!;
        public virtual DbSet<Constant> Constant { get; set; } = null!;
        public virtual DbSet<EmailGroup> EmailGroup { get; set; } = null!;
        public virtual DbSet<ErrorLog> ErrorLog { get; set; } = null!;
        public virtual DbSet<Event> Events { get; set; } = null!;
        public virtual DbSet<EventSetting> EventSettings { get; set; } = null!;
        public virtual DbSet<Expense> Expense { get; set; } = null!;
        public virtual DbSet<ExportTccline> ExportTcclines { get; set; } = null!;
        public virtual DbSet<Job> Job { get; set; } = null!;
        public virtual DbSet<Myoblog> Myoblog { get; set; } = null!;
        public virtual DbSet<Myobsync> Myobsync { get; set; } = null!;
        public virtual DbSet<Period> Period { get; set; } = null!;
        public virtual DbSet<Product> Product { get; set; } = null!;
        public virtual DbSet<ProductCont> ProductCont { get; set; } = null!;
        public virtual DbSet<ProductContGgp> ProductContGgp { get; set; } = null!;
        public virtual DbSet<ProductContPkg> ProductContPkg { get; set; } = null!;
        public virtual DbSet<ProductInventory> ProductInventory { get; set; } = null!;
        public virtual DbSet<ProductInventoryFreight> ProductInventoryFreight { get; set; } = null!;
        public virtual DbSet<ProductInventoryValue> ProductInventoryValue { get; set; } = null!;
        public virtual DbSet<ProductInventoryYtd> ProductInventoryYtd { get; set; } = null!;
        public virtual DbSet<ProductMainGroup> ProductMainGroup { get; set; } = null!;
        public virtual DbSet<ProductMapping> ProductMapping { get; set; } = null!;
        public virtual DbSet<ProductMinorGroup> ProductMinorGroup { get; set; } = null!;
        public virtual DbSet<ProductPacking> ProductPacking { get; set; } = null!;
        public virtual DbSet<ProductPackingDesc> ProductPackingDesc { get; set; } = null!;
        public virtual DbSet<ProductRecipe> ProductRecipe { get; set; } = null!;
        public virtual DbSet<ProductRepacking> ProductRepacking { get; set; } = null!;
        public virtual DbSet<ProductSetting> ProductSettings { get; set; } = null!;
        public virtual DbSet<ProductSubGroup> ProductSubGroup { get; set; } = null!;
        public virtual DbSet<ProductTransfer> ProductTransfer { get; set; } = null!;
        public virtual DbSet<ProductUnit> ProductUnit { get; set; } = null!;
        public virtual DbSet<Purchase> Purchase { get; set; } = null!;
        public virtual DbSet<PurchaseItem> PurchaseItem { get; set; } = null!;
        public virtual DbSet<PurchaseStatus> PurchaseStatus { get; set; } = null!;
        public virtual DbSet<PurchaseType> PurchaseType { get; set; } = null!;
        public virtual DbSet<RepackReason> RepackReason { get; set; } = null!;
        public virtual DbSet<Sale> Sale { get; set; } = null!;
        public virtual DbSet<SaleDispatch> SaleDispatch { get; set; } = null!;
        public virtual DbSet<SaleDispatchItem> SaleDispatchItem { get; set; } = null!;
        public virtual DbSet<SaleItem> SaleItem { get; set; } = null!;
        public virtual DbSet<SaleStatus> SaleStatus { get; set; } = null!;
        public virtual DbSet<SaleType> SaleType { get; set; } = null!;
        public virtual DbSet<StockCount> StockCount { get; set; } = null!;
        public virtual DbSet<Tax> Tax { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Waste> Waste { get; set; } = null!;
        public virtual DbSet<WasteReason> WasteReason { get; set; } = null!;
        public virtual DbSet<XeroLog> XeroLog { get; set; } = null!;
        public virtual DbSet<XeroSync> XeroSync { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-A4RV200\\SQLEXPRESS;Initial Catalog=MercProductionService;MultipleActiveResultSets=true;TrustServerCertificate=true;Integrated Security=True;");
                //optionsBuilder.UseSqlServer("Server=103.28.51.32;Database=MercProductionService;user id=mpsDBadmin;pwd=t5xiwD2rcjg?02BVv;TrustServerCertificate=True;MultipleActiveResultSets=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This is important to include the default identity configurations

            modelBuilder.HasDefaultSchema("mpsDBadmin");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AddressName).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.Comment).HasMaxLength(255);

                entity.Property(e => e.Company).HasMaxLength(50);

                entity.Property(e => e.ContactName).HasMaxLength(25);

                entity.Property(e => e.Country).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.Fax).HasMaxLength(21);

                entity.Property(e => e.Phone1).HasMaxLength(21);

                entity.Property(e => e.Phone2).HasMaxLength(21);

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.Street).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Www)
                    .HasMaxLength(100)
                    .HasColumnName("WWW");
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.CompanyName);

                entity.ToTable("Company", "dbo");

                entity.Property(e => e.CompanyName).HasMaxLength(50);

                entity.Property(e => e.Abn)
                    .HasMaxLength(11)
                    .HasColumnName("ABN");

                entity.Property(e => e.Comment).HasMaxLength(255);

                entity.Property(e => e.File).HasColumnType("ntext");

                entity.Property(e => e.Pk)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("PK");

                entity.Property(e => e.Type).HasMaxLength(10);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.VendorNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<CompanyType>(entity =>
            {
                entity.HasKey(e => e.Type);

                entity.ToTable("CompanyType", "dbo");

                entity.Property(e => e.Type).HasMaxLength(10);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Constant>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("Constants", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Abn)
                    .HasMaxLength(11)
                    .HasColumnName("ABN");

                entity.Property(e => e.AccountingEmail).HasMaxLength(255);

                entity.Property(e => e.Acn)
                    .HasMaxLength(11)
                    .HasColumnName("ACN");

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.Comment).HasMaxLength(255);

                entity.Property(e => e.Company).HasMaxLength(4);

                entity.Property(e => e.EmailNewCompany).HasMaxLength(255);

                entity.Property(e => e.EmailNewInvoice).HasMaxLength(255);

                entity.Property(e => e.EmailNewProduct).HasMaxLength(255);

                entity.Property(e => e.Fax).HasMaxLength(21);

                entity.Property(e => e.File).HasColumnType("ntext");

                entity.Property(e => e.FullName).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(21);

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.Street).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Www)
                    .HasMaxLength(100)
                    .HasColumnName("WWW");
            });

            modelBuilder.Entity<EmailGroup>(entity =>
            {
                entity.ToTable("EmailGroup", "dbo");

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.GroupName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("ErrorLog", "dbo");

                entity.Property(e => e.ErrorDescription).HasMaxLength(255);

                entity.Property(e => e.ErrorNumber).HasMaxLength(50);

                entity.Property(e => e.ErrorSource).HasMaxLength(255);

                entity.Property(e => e.Interface).HasMaxLength(50);

                entity.Property(e => e.Module).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events", "dbo");

                entity.Property(e => e.ActionType).HasMaxLength(20);

                entity.Property(e => e.EventBy).HasMaxLength(20);

                entity.Property(e => e.EventDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EventDesc).HasMaxLength(500);

                entity.Property(e => e.EventType).HasMaxLength(20);
            });

            modelBuilder.Entity<EventSetting>(entity =>
            {
                entity.ToTable("EventSettings", "dbo");

                entity.Property(e => e.ActionType).HasMaxLength(20);

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EventType).HasMaxLength(20);

                entity.Property(e => e.IsShowingLog)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.ToTable("Expense", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ExpenseCode).HasMaxLength(50);

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ExportTccline>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ExportTCCLines", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.AccountNo).HasMaxLength(10);

                entity.Property(e => e.CreditExTax).HasColumnType("decimal(13, 2)");

                entity.Property(e => e.CreditInTax).HasColumnType("decimal(13, 2)");

                entity.Property(e => e.DateOccurred).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DebitExTax).HasColumnType("decimal(13, 2)");

                entity.Property(e => e.DebitInTax).HasColumnType("decimal(13, 2)");

                entity.Property(e => e.ExportedBy).HasMaxLength(20);

                entity.Property(e => e.ExportedOn).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Gstreporting)
                    .HasMaxLength(10)
                    .HasColumnName("GSTReporting")
                    .IsFixedLength();

                entity.Property(e => e.Inclusive)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Job).HasMaxLength(10);

                entity.Property(e => e.JournalNo).HasMaxLength(50);

                entity.Property(e => e.Memo).HasMaxLength(50);

                entity.Property(e => e.TaxCode).HasMaxLength(10);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("Job", "dbo");

                entity.Property(e => e.Code).HasMaxLength(10);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Myoblog>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("MYOBLog", "dbo");

                entity.Property(e => e.ErrorDescription).HasMaxLength(200);

                entity.Property(e => e.ErrorNumber).HasMaxLength(50);

                entity.Property(e => e.ErrorSource).HasMaxLength(50);

                entity.Property(e => e.ExportedBy).HasMaxLength(50);

                entity.Property(e => e.ExportedOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.Result).HasMaxLength(50);

                entity.Property(e => e.Target).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(20);
            });

            modelBuilder.Entity<Myobsync>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("MYOBSync", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Type).HasMaxLength(10);

                entity.Property(e => e.Uid)
                    .HasMaxLength(100)
                    .HasColumnName("UID");
            });

            modelBuilder.Entity<Period>(entity =>
            {
                entity.ToTable("Period", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("Product", "dbo");

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.Color).HasMaxLength(20);

                entity.Property(e => e.Comment).HasMaxLength(255);

                entity.Property(e => e.Desc).HasMaxLength(100);

                entity.Property(e => e.File).HasColumnType("ntext");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID");

                entity.Property(e => e.MinorGroupId).HasColumnName("MinorGroupID");

                entity.Property(e => e.Size).HasMaxLength(20);

                entity.Property(e => e.Tax).HasMaxLength(3);

                entity.Property(e => e.Unit).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductCont>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductCont", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Company).HasMaxLength(50);

                entity.Property(e => e.Desc).HasMaxLength(100);

                entity.Property(e => e.Fr).HasColumnName("FR");

                entity.Property(e => e.Hf).HasColumnName("HF");

                entity.Property(e => e.Lg).HasColumnName("LG");

                entity.Property(e => e.Lp).HasColumnName("LP");

                entity.Property(e => e.Lr).HasColumnName("LR");

                entity.Property(e => e.Pc).HasColumnName("PC");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.Pf).HasColumnName("PF");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.Rc).HasColumnName("RC");

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.Ytdcont).HasColumnName("YTDCont");

                entity.Property(e => e.Ytdsale).HasColumnName("YTDSale");

                entity.Property(e => e.YtdsaleValue).HasColumnName("YTDSaleValue");
            });

            modelBuilder.Entity<ProductContGgp>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductContGGP", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Desc).HasMaxLength(100);

                entity.Property(e => e.Lc).HasColumnName("LC");

                entity.Property(e => e.Lr).HasColumnName("LR");

                entity.Property(e => e.Pc).HasColumnName("PC");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.Pf).HasColumnName("PF");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.Ytdcont).HasColumnName("YTDCont");
            });

            modelBuilder.Entity<ProductContPkg>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductContPKG", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Desc).HasMaxLength(100);

                entity.Property(e => e.Hf).HasColumnName("HF");

                entity.Property(e => e.Pc).HasColumnName("PC");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.Ytdcont).HasColumnName("YTDCont");

                entity.Property(e => e.Ytdsale).HasColumnName("YTDSale");

                entity.Property(e => e.YtdsaleValue).HasColumnName("YTDSaleValue");
            });

            modelBuilder.Entity<ProductInventory>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductInventory", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Nc).HasColumnName("nc");

                entity.Property(e => e.Ncv).HasColumnName("ncv");

                entity.Property(e => e.PClaimed).HasColumnName("pClaimed");

                entity.Property(e => e.PClaimedValue).HasColumnName("pClaimedValue");

                entity.Property(e => e.PackSold).HasColumnName("PackSOLD");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.RClaimed).HasColumnName("rClaimed");

                entity.Property(e => e.SClaimed).HasColumnName("sClaimed");

                entity.Property(e => e.UcWasted).HasColumnName("ucWasted");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.Ytd).HasColumnName("YTD");

                entity.Property(e => e.Ytdclaim).HasColumnName("YTDClaim");

                entity.Property(e => e.YtdclaimValue).HasColumnName("YTDClaimValue");

                entity.Property(e => e.Ytdvalue).HasColumnName("YTDValue");

                entity.Property(e => e.Ytdwaste).HasColumnName("YTDWaste");

                entity.Property(e => e.YtdwasteValue).HasColumnName("YTDWasteValue");
            });

            modelBuilder.Entity<ProductInventoryFreight>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductInventoryFreight", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(255);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductInventoryValue>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductInventoryValue", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.PClaimed).HasColumnName("pClaimed");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.Sclaimed).HasColumnName("SClaimed");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductInventoryYtd>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductInventoryYTD", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.PClaimed).HasColumnName("pClaimed");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.Sclaimed).HasColumnName("SClaimed");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductMainGroup>(entity =>
            {
                entity.ToTable("ProductMainGroup", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductMapping>(entity =>
            {
                entity.ToTable("ProductMapping", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Company).HasMaxLength(50);

                entity.Property(e => e.CompanyCode).HasMaxLength(50);

                entity.Property(e => e.CompanyDesc).HasMaxLength(100);

                entity.Property(e => e.MercCode).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductMinorGroup>(entity =>
            {
                entity.ToTable("ProductMinorGroup", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Prefix).HasMaxLength(10);

                entity.Property(e => e.SubGroupId).HasColumnName("SubGroupID");

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductPacking>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("ProductPacking", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.BestBefore).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.FinishTime).HasColumnType("datetime");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.LabourCode).HasMaxLength(50);

                entity.Property(e => e.PackingDate).HasColumnType("datetime");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.Supplier).HasMaxLength(255);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductPackingDesc>(entity =>
            {
                entity.HasKey(e => e.Reason);

                entity.ToTable("ProductPackingDesc", "dbo");

                entity.Property(e => e.Reason).HasMaxLength(255);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(255);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductRecipe>(entity =>
            {
                entity.ToTable("ProductRecipe", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Component).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductRepacking>(entity =>
            {
                entity.ToTable("ProductRepacking", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Comment).HasMaxLength(50);

                entity.Property(e => e.LabourCode).HasMaxLength(50);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.Reason).HasMaxLength(20);

                entity.Property(e => e.RepackingDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductSetting>(entity =>
            {
                entity.ToTable("ProductSettings", "dbo");

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ProductCode).HasMaxLength(50);
            });

            modelBuilder.Entity<ProductSubGroup>(entity =>
            {
                entity.ToTable("ProductSubGroup", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.MainGroupId).HasColumnName("MainGroupID");

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductTransfer>(entity =>
            {
                entity.ToTable("ProductTransfer", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FromProduct).HasMaxLength(50);

                entity.Property(e => e.InvoiceNo).HasMaxLength(255);

                entity.Property(e => e.ToProduct).HasMaxLength(50);

                entity.Property(e => e.TransferDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductUnit>(entity =>
            {
                entity.HasKey(e => e.Type);

                entity.ToTable("ProductUnit", "dbo");

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.Comment).HasMaxLength(50);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);

                entity.ToTable("Purchase", "dbo");

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.Comment).HasMaxLength(255);

                entity.Property(e => e.Company).HasMaxLength(100);

                entity.Property(e => e.CompanyNo)
                    .HasMaxLength(50)
                    .HasColumnName("CompanyNO");

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.ToTable("PurchaseItem", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.Job).HasMaxLength(10);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.Tax).HasMaxLength(3);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<PurchaseStatus>(entity =>
            {
                entity.HasKey(e => e.Status);

                entity.ToTable("PurchaseStatus", "dbo");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<PurchaseType>(entity =>
            {
                entity.HasKey(e => e.Type);

                entity.ToTable("PurchaseType", "dbo");

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.Desc).HasMaxLength(255);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<RepackReason>(entity =>
            {
                entity.HasKey(e => e.Reason);

                entity.ToTable("RepackReason", "dbo");

                entity.Property(e => e.Reason).HasMaxLength(20);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);

                entity.ToTable("Sale", "dbo");

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.ArrivalDate).HasColumnType("datetime");

                entity.Property(e => e.Comment).HasColumnType("ntext");

                entity.Property(e => e.Company).HasMaxLength(50);

                entity.Property(e => e.CompanyNo)
                    .HasMaxLength(50)
                    .HasColumnName("CompanyNO");

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<SaleDispatch>(entity =>
            {
                entity.HasKey(e => e.DispatchId);

                entity.ToTable("SaleDispatch", "dbo");

                entity.Property(e => e.DispatchId).HasColumnName("DispatchID");

                entity.Property(e => e.ArrivalDate).HasColumnType("datetime");

                entity.Property(e => e.DispatchDate).HasColumnType("datetime");

                entity.Property(e => e.DispatchName).HasMaxLength(50);

                entity.Property(e => e.SaleInvoiceId).HasColumnName("SaleInvoiceID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<SaleDispatchItem>(entity =>
            {
                entity.ToTable("SaleDispatchItem", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BestBefore).HasColumnType("datetime");

                entity.Property(e => e.DispatchId).HasColumnName("DispatchID");

                entity.Property(e => e.Grower).HasMaxLength(50);

                entity.Property(e => e.SaleItemId).HasColumnName("SaleItemID");

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.ToTable("SaleItem", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CompanyCode).HasMaxLength(50);

                entity.Property(e => e.CompanyDesc).HasMaxLength(100);

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.Job).HasMaxLength(10);

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.ProductDesc).HasMaxLength(100);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.Tax).HasMaxLength(3);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<SaleStatus>(entity =>
            {
                entity.HasKey(e => e.Status);

                entity.ToTable("SaleStatus", "dbo");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<SaleType>(entity =>
            {
                entity.HasKey(e => e.Type);

                entity.ToTable("SaleType", "dbo");

                entity.Property(e => e.Type).HasMaxLength(20);

                entity.Property(e => e.Desc).HasMaxLength(255);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<StockCount>(entity =>
            {
                entity.ToTable("StockCount", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BatchCode).HasMaxLength(255);

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<Tax>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.ToTable("Tax", "dbo");

                entity.Property(e => e.Code).HasMaxLength(3);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.LoginId);

                entity.ToTable("Users", "dbo");

                entity.Property(e => e.LoginId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateLastLogin).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateLastLogout).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateModified).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.MobileNumbers)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.NewPassword)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Token).HasMaxLength(255);

                entity.Property(e => e.VerifyType).HasColumnType("text");
            });

            modelBuilder.Entity<Waste>(entity =>
            {
                entity.ToTable("Waste", "dbo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ProductCode).HasMaxLength(50);

                entity.Property(e => e.Reason).HasMaxLength(20);

                entity.Property(e => e.Supplier).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

                entity.Property(e => e.WasteDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<WasteReason>(entity =>
            {
                entity.HasKey(e => e.Reason);

                entity.ToTable("WasteReason", "dbo");

                entity.Property(e => e.Reason).HasMaxLength(20);

                entity.Property(e => e.SortId).HasColumnName("SortID");

                entity.Property(e => e.UpdatedBy).HasMaxLength(50);

                entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<XeroLog>(entity =>
            {
                entity.HasKey(e => e.Index);

                entity.ToTable("XeroLog", "dbo");

                entity.Property(e => e.ErrorDescription).HasMaxLength(200);

                entity.Property(e => e.ErrorNumber).HasMaxLength(50);

                entity.Property(e => e.ErrorSource).HasMaxLength(50);

                entity.Property(e => e.ExportedBy).HasMaxLength(50);

                entity.Property(e => e.ExportedOn).HasColumnType("datetime");

                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

                entity.Property(e => e.PeriodId).HasColumnName("PeriodID");

                entity.Property(e => e.Result).HasMaxLength(50);

                entity.Property(e => e.Target).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(20);
            });

            modelBuilder.Entity<XeroSync>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.ToTable("XeroSync", "dbo");

                entity.Property(e => e.Pk).HasColumnName("PK");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Type).HasMaxLength(10);

                entity.Property(e => e.Uid)
                    .HasMaxLength(100)
                    .HasColumnName("UID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
