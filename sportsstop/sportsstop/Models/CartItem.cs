using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class CartItem
    {
        [ForeignKey("Item")]
        public int ItemId { get; set;}
        public Item Item { get; set; }

        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int ItemQuantity { get; set; }
    }
}
