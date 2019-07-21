using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sportsstop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sportsstop.Util;

namespace sportsstop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext context;
        private ResponseObject response;

        public OrdersController(AppDbContext appDbContext)
        {
            this.context = appDbContext;
            response = new ResponseObject();
            response.Status = false;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ResponseObject> Get()
        {
            await HttpContext.Session.LoadAsync();
            var userId = HttpContext.Session.GetInt32("UserID");
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                List<Order> orders = await context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(oi => oi.OrderItems)
                    .ToListAsync();
                response.SetContent(true, "Order history fetched successfully", orders.ToList<object>());
            }
            else
            {
                response.Status = false;
                response.Message = "Unauthorized access not allowed";
            }
            return response;
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ResponseObject> Get(int id)
        {
            await HttpContext.Session.LoadAsync();
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                List<Order> orders = context.Orders
                                    .Where(o => o.UserId == HttpContext.Session.GetInt32("UserId"))
                                    .Where(so => so.Id == id)
                                    .ToList<Order>();
                response.SetContent(true, "Order fetched successfully", orders.ToList<object>());
            }
            else
            {
                response.Status = false;
                response.Message = "Unauthorized access not allowed";
            }
            return response;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] OrderDetails orderDetails)
        {
            //int cartID, int shippingAddressID,Address address=null
            Order order = new Order();
            Cart cart = new Cart();
            //cart.CartItems = new List<CartItem>();

            await HttpContext.Session.LoadAsync();
            int userID = HttpContext.Session.GetInt32("UserID") ?? 0;

            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                cart = context.Carts
                        .Where(c => c.UserId == userID)
                        .Include(i => i.CartItems)
                        .Include(u => u.User)
                        .ThenInclude(ua => ua.UserAddresses)
                        .SingleOrDefault();



                if (cart != null || cart.CartItems != null)
                {
                    order = ProcessOrder(cart, orderDetails.shippingAddressID);

                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    List<Order> orders = new List<Order>() { order };
                    response.SetContent(true, "Order Placed Successfully", orders.ToList<object>());
                    //return response;
                }
                else
                {
                    response.SetContent(false, "Could not process order");
                    //return response;
                }
            }
            else//*********ANNONYMOUS USER
            {
                await HttpContext.Session.LoadAsync();
                cart = HttpContext.Session.GetObject<Cart>("AnnonymousCart") ?? null;
                if (cart != null && ValidateAddress(orderDetails.address))
                {
                    cart.User = new User();
                    cart.User.UserAddresses = new List<Address>();
                    cart.User.UserAddresses.Add(orderDetails.address);
                    order = ProcessOrder(cart, orderDetails.shippingAddressID);
                    List<Order> orders = new List<Order>() { order };
                    response.SetContent(true, "Order Placed Successfully", orders.ToList<object>());
                    //return response;
                }
                else
                {
                    response.SetContent(false, "Could not process order");
                    //return response;
                }
            }
            return response;
        }

        // PUT: api/Orders/5
        [HttpPut]
        public ResponseObject Put([FromBody] string value = "")
        {
            response.SetContent(false, "Trying to edit your order, PAGE 404 in your ass!");
            return response;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public ResponseObject Delete(int id)
        {
            response.SetContent(false, "Trying to delete your order, PAGE 404 in your ass! MONEY KEPT under our NON-REFUND policy");
            return response;
        }

        public Order ProcessOrder(Cart cart, int addressID)
        {
            Order order = new Order();
            Address ad = new Address();
            //DateTime now = new DateTime();
            //now = DateTime.Now;
            order.FirstName = cart.User.FirstName;
            order.LastName = cart.User.LastName;
            order.Email = cart.User.Email;

            ad = cart.User.UserAddresses.FirstOrDefault(i => i.Id == addressID);

            if (ad != null)
            {
                order.Address1 = ad.Address1;
                order.Address2 = ad.Address2;
                order.City = ad.City;
                order.Country = ad.Country;
                order.Postal = ad.Postal;
                //order.OrderDate = now;
              //  order.Status = "Shipped";
                order.UserId = cart.UserId;
            }

            foreach (CartItem x in cart.CartItems)
            {
                OrderItem oi = new OrderItem();
                oi.ItemId = x.ItemId;
                //oi.Name = x.Item.Name;
                oi.Name = context.Items.FirstOrDefault(i => i.Id == x.ItemId).Name;
                //oi.Description = x.Item.Description;
                oi.Description = context.Items.FirstOrDefault(i => i.Id == x.ItemId).Description;
                //oi.Price = x.Item.Price * x.ItemQuantity;
                oi.Price = context.Items.FirstOrDefault(i => i.Id == x.ItemId).Price * x.ItemQuantity;
                oi.Quantity = x.ItemQuantity;
                //oi.Weight = x.Item.Weight;
               // oi.Weight = context.Items.FirstOrDefault(i => i.Id == x.ItemId).Weight;
                //oi.ShippingCost = x.Item.ShippingCost;
                oi.ShippingCost = context.Items.FirstOrDefault(i => i.Id == x.ItemId).ShippingCost;
                //oi.Tax = x.Item.Tax;
                oi.Tax = context.Items.FirstOrDefault(i => i.Id == x.ItemId).Tax;
                //oi.TotalPrice = (x.Item.Price * x.ItemQuantity) + x.Item.ShippingCost + x.Item.Tax;
                oi.TotalPrice = oi.Price + oi.ShippingCost + oi.Tax;
                order.OrderItems.Add(oi);
                //order.OrderItems.Add(new OrderItem()
                //{
                //    ItemId = x.ItemId,
                //    Name = x.Item.Name,
                //    Description = x.Item.Description,
                //    Price = x.Item.Price * x.ItemQuantity,
                //    Quantity = x.ItemQuantity,
                //    Weight = x.Item.Weight,
                //    ShippingCost = x.Item.ShippingCost,
                //    Tax = x.Item.Tax,
                //    TotalPrice = (x.Item.Price * x.ItemQuantity) + x.Item.ShippingCost + x.Item.Tax
                //});
            }

            if (cart.CartItems != null)
            {
                //cart.CartItems.ToList<CartItem>().ForEach(x => order.OrderItems.Add(new OrderItem()
                //{
                //    ItemId = x.ItemId,
                //    Name = x.Item.Name,
                //    Description = x.Item.Description,
                //    Price = x.Item.Price * x.ItemQuantity,
                //    Quantity = x.ItemQuantity,
                //    Weight = x.Item.Weight,
                //    ShippingCost = x.Item.ShippingCost,
                //    Tax = x.Item.Tax,
                //    TotalPrice = (x.Item.Price * x.ItemQuantity) + x.Item.ShippingCost + x.Item.Tax
                //}));

                order.OrderItems.ToList().ForEach(o =>
                {
                    order.SubTotal += o.Price;
                    order.Tax += o.Tax;
                    order.ShippingCost += o.ShippingCost;
                });
            }


            return order;
        }

        public bool ValidateAddress(Address ad)
        {
            return ad.GetType().GetProperties().All(a => a != null);
        }
        public class OrderDetails
        {
            //public int cartID { get; set; }
            public int shippingAddressID { get; set; }
            public Address address { get; set; }
        }
    }
}
