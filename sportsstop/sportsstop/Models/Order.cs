using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Postal { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }

        public int UserId { get; set; }

        public Order()
        {
            this.OrderItems = new List<OrderItem>();
        }
    }
}
