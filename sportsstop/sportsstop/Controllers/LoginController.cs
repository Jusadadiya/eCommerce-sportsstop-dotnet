using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sportsstop.Models;
using sportsstop.Util;
using Microsoft.EntityFrameworkCore;

namespace sportsstop.Controllers
{
    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly AppDbContext context;
        private ResponseObject response;

        public LoginController(AppDbContext context)
        {
            response = new ResponseObject();
            this.context = context;
        }
        // GET: api/Login
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Login/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return PasswordHash.HashPassword(id);
        }

        // POST: api/Login
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] Login login)
        {
            var userCount = new User();
            var passhash = PasswordHash.HashPassword(login.Password);
            try
            {
                userCount = await context.Users
                                .Where(u => u.Email == login.Email)
                                .Where(u => PasswordHash.VerifyHashedPassword(u.Password, login.Password))
                                .SingleOrDefaultAsync();

                if (userCount != null)
                {

                    Cart cart = await context.Carts
                    .Where(u => u.UserId == userCount.Id)
                    .Include(c => c.CartItems)
                    .SingleOrDefaultAsync();

                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetInt32("IsLoggedIn", 1);
                    HttpContext.Session.SetInt32("UserID", userCount.Id);
                    HttpContext.Session.SetInt32("CartID", (cart.Id != 0) ? cart.Id : 0);
                    await HttpContext.Session.CommitAsync();
                   
                    response.SetContent(true, "Login Successful");
                    return response;
                   
                }
                else
                {
                    response.SetContent(false, "Fail to Login");
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.SetContent(false, ex.Message);
                return response;
            }
        }

        // PUT: api/Login/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        public async Task<ResponseObject> Delete()
        {
            await HttpContext.Session.LoadAsync();
            HttpContext.Session.SetInt32("IsLoggedIn", 0);
            HttpContext.Session.SetInt32("UserID", 0);
            HttpContext.Session.SetInt32("CartID", 0);
            await HttpContext.Session.CommitAsync();

            response.SetContent(true, "Logout successfully");
            return response;
        }
    }
}
