﻿using Microsoft.EntityFrameworkCore;

namespace Intex2.Models
{
    public partial class LegoContext : DbContext

    {
        public LegoContext(DbContextOptions<LegoContext> options) : base(options) { }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<LineItem> LineItems { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Recommendation> Recommendations { get; set; }
        public virtual DbSet<Top_Rating> Top_Ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId).HasName("PK__Customer__CD64CFDD25C5C5DB");

                entity.Property(e => e.CustomerId)
                    .ValueGeneratedNever()
                    .HasColumnName("customer_ID");
                entity.Property(e => e.Age).HasColumnName("age");
                entity.Property(e => e.BirthDate).HasColumnName("birth_date");
                entity.Property(e => e.CountryOfResidence)
                    .HasMaxLength(50)
                    .HasColumnName("country_of_residence");
                entity.Property(e => e.CustEmail)
                    .HasMaxLength(50)
                    .HasColumnName("cust_email");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("first_name");
                entity.Property(e => e.Gender)
                    .HasMaxLength(50)
                    .HasColumnName("gender");
                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("last_name");
            });

            modelBuilder.Entity<LineItem>(entity =>
            {
                entity.HasKey(e => new { e.TransactionId, e.ProductId });

                entity.Property(e => e.TransactionId).HasColumnName("transaction_ID");
                entity.Property(e => e.ProductId).HasColumnName("product_ID");
                entity.Property(e => e.Qty).HasColumnName("qty");
                entity.Property(e => e.Rating).HasColumnName("rating");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => new { e.TransactionId, e.CustomerId });

                entity.Property(e => e.TransactionId).HasColumnName("transaction_ID");
                entity.Property(e => e.CustomerId).HasColumnName("customer_ID");
                entity.Property(e => e.Amount).HasColumnName("amount");
                entity.Property(e => e.Bank)
                    .HasMaxLength(50)
                    .HasColumnName("bank");
                entity.Property(e => e.CountryOfTransaction)
                    .HasMaxLength(50)
                    .HasColumnName("country_of_transaction");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.DayOfWeek)
                    .HasMaxLength(50)
                    .HasColumnName("day_of_week");
                entity.Property(e => e.EntryMode)
                    .HasMaxLength(50)
                    .HasColumnName("entry_mode");
                entity.Property(e => e.Fraud).HasColumnName("fraud");
                entity.Property(e => e.ShippingAddress)
                    .HasMaxLength(50)
                    .HasColumnName("shipping_address");
                entity.Property(e => e.Time).HasColumnName("time");
                entity.Property(e => e.TypeOfCard)
                    .HasMaxLength(50)
                    .HasColumnName("type_of_card");
                entity.Property(e => e.TypeOfTransaction)
                    .HasMaxLength(50)
                    .HasColumnName("type_of_transaction");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.ProductId)
                    .ValueGeneratedNever()
                    .HasColumnName("product_ID");
                entity.Property(e => e.Category)
                    .HasMaxLength(50)
                    .HasColumnName("category");
                entity.Property(e => e.Description)
                    .HasMaxLength(2800)
                    .HasColumnName("description");
                entity.Property(e => e.ImgLink)
                    .HasMaxLength(150)
                    .HasColumnName("img_link");
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
                entity.Property(e => e.NumParts).HasColumnName("num_parts");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.PrimaryColor)
                    .HasMaxLength(50)
                    .HasColumnName("primary_color");
                entity.Property(e => e.SecondaryColor)
                    .HasMaxLength(50)
                    .HasColumnName("secondary_color");
                entity.Property(e => e.Year).HasColumnName("year");
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
