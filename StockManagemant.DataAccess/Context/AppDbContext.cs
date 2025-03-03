﻿using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using StockManagemant.Entities.Models;

namespace StockManagemant.DataAccess.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptDetail> ReceiptDetails { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseProduct> WarehouseProducts { get; set; }

        // Model yapılandırmaları
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Receipt - Warehouse ilişkisi
            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.Warehouse)
                .WithMany()
                .HasForeignKey(r => r.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ WarehouseProduct - Warehouse ilişkisi
            modelBuilder.Entity<WarehouseProduct>()
                .HasOne(wp => wp.Warehouse)
                .WithMany(w => w.WarehouseProducts)
                .HasForeignKey(wp => wp.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ WarehouseProduct - Product ilişkisi (Eksik WithMany() eklendi!)
            modelBuilder.Entity<WarehouseProduct>()
                .HasOne(wp => wp.Product)
                .WithMany(p => p.WarehouseProducts) // ✅ Eksik WithMany eklendi!
                .HasForeignKey(wp => wp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ ReceiptDetail - Receipt ilişkisi
            modelBuilder.Entity<ReceiptDetail>()
                .HasOne(rd => rd.Receipt)
                .WithMany(r => r.ReceiptDetails)
                .HasForeignKey(rd => rd.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ ReceiptDetail - Product ilişkisi (Eksik WithMany() eklendi!)
            modelBuilder.Entity<ReceiptDetail>()
                .HasOne(rd => rd.Product)
                .WithMany(p => p.ReceiptDetails) // ✅ Eksik WithMany eklendi!
                .HasForeignKey(rd => rd.ProductId);

            // ✅ Product - Category ilişkisi
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Soft Delete için Global Query Filter
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Warehouse>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<WarehouseProduct>().HasQueryFilter(wp => !wp.IsDeleted);
        }
    }

}
