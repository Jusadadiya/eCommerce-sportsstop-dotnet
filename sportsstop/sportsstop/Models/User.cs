using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace sportsstop.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Telephone { get; set; }
        public DateTime DOB { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime RecordDate { get; set; }
        public virtual ICollection<Address> UserAddresses { get; set; }
        public virtual ICollection<ItemComment> ItemComments { get; set; }

        /*[ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart UserCart { get; set; }
        */
        public virtual ICollection<Order> Orders { get; set; }
    }
}
