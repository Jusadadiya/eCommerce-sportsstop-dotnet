using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sportsstop.Models;
using Microsoft.EntityFrameworkCore;

namespace sportsstop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext context;
        private ResponseObject response;

        public CommentsController(AppDbContext appDbContext)
        {
            this.context = appDbContext;
            response = new ResponseObject();
            response.Status = false;
        }

        public class CommentsPostDetails
        {
            public string Comment { get; set; }
            public int Rating { get; set; }
            public int ItemID { get; set; }
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] CommentsPostDetails comment)
        {
            await HttpContext.Session.LoadAsync();
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                List<Order> orders = new List<Order>();
                Item item = new Item();

                response.SetContent(false, "Cannot post comment");
                if (string.IsNullOrEmpty(comment.Comment))
                    return response;

                await HttpContext.Session.LoadAsync();
                var userID = HttpContext.Session.GetInt32("UserID") ?? 0;
                
                if(userID != 0)
                {
                    orders = context.Orders
                                .Where(o => o.UserId == userID)
                                .Include(oi => oi.OrderItems).ToList();

                    if (orders != null)
                    {
                        if (orders.Any(o => o.OrderItems.Any(oi => oi.ItemId == comment.ItemID)))
                        {
                            context.ItemComments.Add(new ItemComment() { Comment = comment.Comment, Rating = comment.Rating, ItemId = comment.ItemID, UserId = userID });
                            await context.SaveChangesAsync();
                            response.SetContent(true, "Comment saved successfully");
                            return response;
                        }
                        else
                        {
                            response.SetContent(false, "Cannot comment if you have not purchased an item.");
                        }
                    }
                    else
                    {
                        response.SetContent(false, "Cannot comment if you have not purchased an item.");
                    }
                }
            }
            else
            {
                response.SetContent(false, "Unfortunately guest users cannot post comments");
            }
            return response;   
        }
    }
}
