using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eCommerceCore.Models;
using eCommerceCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace eCommerceCore.Controllers
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
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
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
                                .Where(u => u.Password == passhash)
                                .SingleOrDefaultAsync();

                if(userCount.Id != 0)
                {
                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetInt32("IsLoggedIn", 1);
                    HttpContext.Session.SetInt32("UserID", userCount.Id);
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
            catch(Exception ex)
            {
                response.SetContent(false, "Fail to Login");
                return response;
            }
        }

        // PUT: api/Login/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
