namespace DataLayer.Models.Mapping
{
    class ProductMap
      : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Product>
    {
        public ProductMap()
        {
            this.HasKey(current => current.Id);

            //Table Relations

            this.HasRequired(current => current.category)
                .WithMany(Category => Category.Products)
                .HasForeignKey(current => current.CategoryId)
                .WillCascadeOnDelete(false);

            this.Property(current => current.Stock)
                .IsOptional();


            //this.HasMany(current => current.RelatedProducts)
            //    .WithMany(Product => Product.RelatedProducts)
            //    .Map(current =>
            //    {
            //        current.ToTable("RelatedProducts");
            //        current.MapLeftKey("ParentId");
            //        current.MapRightKey("ProductId");
            //    });
        }
    }
}
