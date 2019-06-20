using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sportsstop.Models;
using Microsoft.EntityFrameworkCore;
using sportsstop.Util;

namespace sportsstop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext context;
        private ResponseObject response;
      
        public CartController(AppDbContext context)
        {
            response = new ResponseObject();
            this.context = context;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ResponseObject> Get()
        {
            Cart cart = new Cart();
            await HttpContext.Session.LoadAsync();
            var cartId = HttpContext.Session.GetInt32("CartID") ?? 0;
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;                                                 

            if (cartId <= 0)
            {
                if (userId <= 0)
                {
                    await HttpContext.Session.LoadAsync();
                    cart = HttpContext.Session.GetObject<Cart>("AnnonymousCart") ?? await NewAnnonymousCart();
                }
                else
                {
                    cart = await context.Carts
                        .Where(c => c.UserId == userId)
                        .Include(i => i.CartItems)
                        .SingleOrDefaultAsync();

                    if (cart != null)
                    {
                        await HttpContext.Session.LoadAsync();
                        HttpContext.Session.SetInt32("UserID", cart.UserId);
                    }
                    else
                    {
                        response.SetContent(false, "Fail to load cart, try again later");
                        return response;
                    }
                }
            }
            else
            {
                cart = await context.Carts
                    .Where(c => c.Id == cartId)
                    .Include(i => i.CartItems)
                    .SingleOrDefaultAsync();

                if (cart == null)
                {
                    response.SetContent(false, "Failed to load cart with given cart id.");
                    return response;
                }
            }
            List<Cart> allcarts = new List<Cart>() { cart };
            response.SetContent(true, "Carts Successfully Returned", allcarts.ToList<object>());
            return response;
        }


        // GET: api/Cart/5
        [HttpGet("{id}")]
        public async Task<ResponseObject> Get(int id = 0)
        {
            var cart = new Cart();
            response.SetContent(false, "Failed to load");
            var cartId = HttpContext.Session.GetInt32("CartID") ?? id;
            try
            {
                cart = await context.Carts
                    .Where(u => u.Id == cartId)
                    .Include(c => c.CartItems)
                    .SingleOrDefaultAsync();

                if (cart != null)
                {
                    response.SetContent(true, "Single Cart returned ");
                    List<Cart> cartList = new List<Cart>() { cart };
                    response.Data = cartList.ToList<object>();
                }
                else
                {
                    response.SetContent(false, "Could not load cart");
                }

            }
            catch (Exception ex)
            {
                response.SetContent(false, ex.Message);
            }
            return response;
        }

        public class PostCartDetials
        {
            public int itemID { get; set; }
            public int cartID { get; set; }
            public int qty { get; set; }
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] PostCartDetials pcd)
        {
            Item item = new Item();
            Cart cart = new Cart();
            CartItem cartItem = new CartItem();

            response.SetContent(false, "Item could not add to cart");

            await HttpContext.Session.LoadAsync();
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            try
            {
                if (userId == 0)//***********ANNONYMOUS USER
                {
                    await HttpContext.Session.LoadAsync();
                    cart = HttpContext.Session.GetObject<Cart>("AnnonymousCart") ?? await NewAnnonymousCart();

                    item = await context.Items
                            .Where(i => i.Id == pcd.itemID)
                            .SingleOrDefaultAsync();

                    if (pcd.qty > 0 && pcd.qty < item.StockQty)
                    {
                        if (item != null)
                        {
                            cartItem.CartId = cart.Id;
                            //cartItem.Cart = cart;

                            cartItem.ItemId = item.Id;
                            cartItem.Item = item;

                            cartItem.ItemQuantity = pcd.qty;

                            cart.CartItems.Add(cartItem);

                            cart = CalculateCart(cart);

                            await HttpContext.Session.LoadAsync();
                            HttpContext.Session.SetObject("AnnonymousCart", cart);
                            await HttpContext.Session.CommitAsync();

                            response.SetContent(true, "Added Successfully");
                        }
                        else
                        {
                            response.SetContent(false, "Invalid data provided");
                        }
                    }
                    else
                    {
                        if (pcd.qty < item.StockQty)
                            response.SetContent(false, "Insufficient quantity in stock. In Stock:" + item.StockQty);
                        else
                            response.SetContent(false, "Quantity cannot be 0");
                    }
                }
                else//****************************REGISTERED USER
                {
                    if (userId == 0)
                    {
                        response.SetContent(false, "Please login again");
                        return response;
                    }
                    if (pcd.itemID > 0 && pcd.cartID > 0)
                    {
                        cart = await context.Carts
                        .Where(c => c.Id == pcd.cartID)
                        .SingleOrDefaultAsync();

                        item = await context.Items
                            .Where(i => i.Id == pcd.itemID)
                            .SingleOrDefaultAsync();
                    }

                    if (pcd.qty < item.StockQty)
                    {
                        if (cart != null && item != null && pcd.qty > 0)
                        {
                            cartItem.CartId = cart.Id;
                            cartItem.Cart = cart;

                            cartItem.ItemId = item.Id;
                            cartItem.Item = item;
                            cartItem.ItemQuantity = pcd.qty;

                            cart.CartItems.Add(cartItem);
                            cart = CalculateCart(cart);
                            context.Carts.Update(cart);
                            await context.SaveChangesAsync();

                            response.SetContent(true, "Added Successfully");
                        }
                        else
                        {
                            response.SetContent(false, "Invalid data provided");
                        }
                    }
                    else
                    {
                        response.SetContent(false, "Insufficient quantity in stock. In Stock:" + item.StockQty);
                    }
                }
            }
            catch (Exception ex)
            {
                response.SetContent(false, ex.Message + " -> " + ex.InnerException);
            }
            return response;
        }

        public class UpdateCartDetails
        {
            public int qty { get; set; }
            public int itemID { get; set; }
        }

        // PUT: api/Cart/5
        [HttpPut]
        public async Task<ResponseObject> Put([FromBody] UpdateCartDetails ucd)
        {
            //Item item = new Item();
            Cart cart = new Cart();
            CartItem cartItem = new CartItem();

            response.SetContent(false, "Item could not add to cart");

            await HttpContext.Session.LoadAsync();
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                //item = await context.Items
                //            .Where(i => i.Id == ucd.itemID)
                //            .SingleOrDefaultAsync();

                if (userId <= 0)//***********Annonymous User
                {
                    await HttpContext.Session.LoadAsync();
                    cart = HttpContext.Session.GetObject<Cart>("AnnonymousCart") ?? await NewAnnonymousCart();

                    if (!cart.CartItems.Any<CartItem>(i => i.ItemId == ucd.itemID))
                    {
                        response.SetContent(false, "Item not found in cart");
                    }
                    else
                    {
                        cart.CartItems.Where(i => i.ItemId == ucd.itemID).FirstOrDefault().ItemQuantity = ucd.qty;
                        cart = CalculateCart(cart);
                        await HttpContext.Session.LoadAsync();
                        HttpContext.Session.SetObject("AnnonymousCart", cart);
                        await HttpContext.Session.CommitAsync();
                        response.SetContent(true, "Updated Successfully");
                        /*var citem = cart.CartItems.FirstOrDefault(i => i.ItemId == itemID);

                        if(citem != null)
                        {
                            cart.CartItems.Where(i => i.ItemId == itemID).FirstOrDefault().ItemQuantity = qty;
                        }
                        else
                        {
                            response.SetContent(false, "Could not find item in cart");
                        }*/
                    }
                }
                else if (userId > 0)
                {
                    await HttpContext.Session.LoadAsync();
                    var cartId = HttpContext.Session.GetInt32("CartID") ?? 0;
                    if (cartId != 0)
                    {
                        cart = await context.Carts
                            .Where(i => i.Id == cartId)
                            .Where(ui => ui.UserId == userId)
                            .Include(ci => ci.CartItems)
                            .SingleOrDefaultAsync();

                        if (cart != null)
                        {
                            cart.CartItems.Where(i => i.ItemId == ucd.itemID).FirstOrDefault().ItemQuantity = ucd.qty;
                            cart = CalculateCart(cart);
                            context.Carts.Update(cart);
                            await context.SaveChangesAsync();
                            response.SetContent(true, "Updated Successfully");
                        }
                        else
                        {
                            response.SetContent(false, "Could not load cart");
                        }
                    }
                    else
                    {
                        //CART COOKIE NOT FOUND
                        /*Cart userCart = new Cart();
                        userCart.UserId = userId;
                        context.Carts.Add(userCart);
                        await context.SaveChangesAsync();*/
                        response.SetContent(false, "Error trying to load cart. Please try again later");
                    }
                }
                else
                {
                    response.SetContent(false, "Invalid Item id");
                }
            }
            catch (Exception xe)
            {
                response.SetContent(false, xe.Message);
            }
            return response;
        }

        public class DeleteDetails
        {
            public int itemID { get; set; }
            public int cartID { get; set; }
        }

        // DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        [HttpDelete]
        public async Task<ResponseObject> Delete([FromBody] DeleteDetails dd)
        {
            Item item = new Item();
            Cart cart = new Cart();
            CartItem cartItem = new CartItem();

            response.SetContent(false, "Invalid inputs");
            if (dd.itemID == 0 && dd.cartID == 0)
                return response;

            await HttpContext.Session.LoadAsync();
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            dd.cartID = HttpContext.Session.GetInt32("CartID") ?? dd.cartID;

            try
            {
                if (userId == 0)//***********Annonymous User
                {
                    await HttpContext.Session.LoadAsync();
                    cart = HttpContext.Session.GetObject<Cart>("AnnonymousCart") ?? await NewAnnonymousCart();

                    if (!cart.CartItems.Any<CartItem>(i => i.ItemId == dd.itemID))
                    {
                        response.SetContent(false, "Item not found in cart");
                    }
                    else
                    {
                        //cartItem = await context.CartItems
                        //    .Where(ci => ci.CartId == dd.cartID)
                        //    .Where(i => i.ItemId == dd.itemID)
                        //    .SingleOrDefaultAsync();

                        //if(cartItem == null)
                        //{
                        //    response.SetContent(false, "CartItem not found");
                        //    return response;
                        //}

                        //cart = await context.Carts
                        //    .Where(ci => ci.Id == dd.cartID)
                        //    .SingleOrDefaultAsync();

                        //if (cart == null)
                        //{
                        //    response.SetContent(false, "Cart not found");
                        //    return response;
                        //}
                        cartItem = cart.CartItems.Where(x => x.ItemId == dd.itemID).SingleOrDefault() ?? null;
                        if (cartItem != null)
                        {
                            cart.CartItems.Remove(cartItem);
                        }
                        else
                        {
                            response.SetContent(false, "CartItem not found");
                            return response;
                        }

                        //cart.CartItems.Remove(cartItem);
                        cart = CalculateCart(cart);
                        //context.Carts.Update(cart);
                        //context.Carts.Remove(cart);
                        //await context.SaveChangesAsync();

                        await HttpContext.Session.LoadAsync();
                        HttpContext.Session.SetObject("AnnonymousCart", cart);
                        await HttpContext.Session.CommitAsync();
                        response.SetContent(true, "Deleted Successfully");
                    }
                }
                else if (userId > 0)
                {
                    if (dd.cartID != 0)
                    {
                        cartItem = await context.CartItems
                            .Where(ci => ci.CartId == dd.cartID)
                            .Where(i => i.ItemId == dd.itemID)
                            .SingleOrDefaultAsync();

                        cart = await context.Carts
                            .Where(i => i.Id == dd.cartID)
                            .Where(ui => ui.UserId == userId)
                            .SingleOrDefaultAsync();

                        if (cart != null && cartItem != null)
                        {
                            cart.CartItems.Remove(cart.CartItems.FirstOrDefault(i => i.ItemId == dd.itemID));
                            cart = CalculateCart(cart);
                            context.Carts.Update(cart);
                            await context.SaveChangesAsync();
                            response.SetContent(true, "Item delete successfully.");
                        }
                        else
                        {
                            response.SetContent(false, "Could not load cart item");
                        }
                    }
                    else
                    {
                        //CART COOKIE NOT FOUND
                        /*Cart userCart = new Cart();
                        userCart.UserId = userId;
                        context.Carts.Add(userCart);
                        await context.SaveChangesAsync();*/
                        response.SetContent(false, "Error trying to load cart. Please try again later");
                    }
                }
                else
                {
                    response.SetContent(false, "Invalid User id");
                }
            }
            catch (Exception xe)
            {
                response.SetContent(false, xe.Message);
            }
            return response;
        }

        public async Task<Cart> NewAnnonymousCart()
        {
            await HttpContext.Session.LoadAsync();
            Cart cart = new Cart();
            cart.Id = RandomInt.Get();
            HttpContext.Session.SetObject("AnnonymousCart", cart);
            await HttpContext.Session.CommitAsync();
            return cart;
        }

        public async Task<int> GetSessionCartID()
        {
            await HttpContext.Session.LoadAsync();
            var cartID = HttpContext.Session.GetInt32("CartID") ?? 0;
            return cartID;
        }

        public Cart CalculateCart(Cart cart)
        {
            List<decimal> subtotal = new List<decimal>();
            decimal subtotalFinal = 0;
            decimal priceFinal = 0;
            decimal taxFinal = 0;
            decimal shippingFinal = 0;

            List<decimal> price = new List<decimal>();
            List<decimal> tax = new List<decimal>();
            List<decimal> shipping = new List<decimal>();
            try
            {
                cart.CartItems.ToList().ForEach(x => subtotal.Add(x.Item.Price * x.ItemQuantity));
                cart.CartItems.ToList().ForEach(x => price.Add(x.Item.Price));
                cart.CartItems.ToList().ForEach(x => tax.Add(x.Item.Tax));
                cart.CartItems.ToList().ForEach(x => shipping.Add(x.Item.ShippingCost));
                for (int x = 0; x < subtotal.Count; x++)
                {
                    subtotalFinal += subtotal[x];
                    priceFinal += price[x];
                    taxFinal += tax[x];
                    shippingFinal += shipping[x];
                }
                cart.SubTotal = subtotalFinal;
                cart.TotalPrice = priceFinal;
                cart.Tax = taxFinal;
                cart.ShippingCost = shippingFinal;
            }
            catch (Exception ex)
            {
            }
            return cart;
        }

    }
}