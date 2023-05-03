using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template_4332
{
    public class OrderContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public OrderContext() : base("name=OrderContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(o => o.CodeOrder)
                .HasMaxLength(50);

            modelBuilder.Entity<Order>()
                .Property(o => o.CodeClient)
                .HasMaxLength(50);

            modelBuilder.Entity<Order>()
                .Property(o => o.Services)
                .HasMaxLength(200);

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasMaxLength(50);

            base.OnModelCreating(modelBuilder);
        }
    }
}
