using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public decimal Price { get; set; }
        public decimal StockQty { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public virtual ICollection<ItemComment> ItemComments { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
