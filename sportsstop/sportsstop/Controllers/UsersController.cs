using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sportsstop.Models;
using Microsoft.EntityFrameworkCore;
using sportsstop.Util;

namespace eCommerceCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext context;
        private ResponseObject response;
        //private readonly ILogger logger;


        public UsersController(
           //ILogger<ProductController> logger,
           AppDbContext appDbContext
        )
        {
            //this.logger = logger;
            response = new ResponseObject();
            this.context = appDbContext;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ResponseObject> Get()
        {
            List<User> allusers = await context.Users.ToListAsync();
            response.SetContent(true, "Users Successfully Returned", allusers.ToList<object>());
            return response;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ResponseObject> Get(int id)
        {
            var user = new User();
            var filteredUser = new User();
            List<User> usersList = new List<User>();
            await HttpContext.Session.LoadAsync();
            var userId = HttpContext.Session.GetInt32("userId") ?? 0;

            /*if (userId != 0)
            {
                user = context.Users
                                .Include(c => c.UserAddresses)
                                .FirstOrDefault(c => c.Id == id);
            }*/
            try
            {

                //if (userId != 0)
                {
                    //user = await context.Users.SingleOrDefaultAsync<User>(c => c.Id == id);
                    user = await context.Users
                                .Where(u => u.Id == id)
                                .Select(p => new User
                                {
                                    Id = p.Id,
                                    FirstName = p.FirstName,
                                    IsRegistered = p.IsRegistered
                                })
                                .SingleOrDefaultAsync();

                    /*filteredUser.Id = user.Id;
                    filteredUser.FirstName = user.FirstName;
                    filteredUser.LastName = user.LastName;
                    filteredUser.DOB = user.DOB;
                    filteredUser.Email = user.Email;
                    filteredUser.IsRegistered = user.IsRegistered;
                    filteredUser.Telephone = user.Telephone;*/
                }

                if (user != null)
                {
                    List<User> userList = new List<User> { user };
                    response.SetContent(true, "Single User returned ", userList.ToList<object>());
                }
                else
                {
                    response.SetContent(false, "User not found");
                }

                //response.Data = usersList.ToList<object>();
                return response;
            }
            catch (Exception ex)
            {
                response.SetContent(false, ex.Message);
                return response;
            }

        }

        // POST: api/Users
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] User newUser)
        {
            try
            {
                ResponseObject AllUSersObj = await Get();

                if (ValidateUserDataOnPost(newUser) && !UserExist((AllUSersObj.Data).Cast<User>().ToList(), newUser))
                {
                    newUser.Password = PasswordHash.HashPassword(newUser.Password);
                    DateTime today = new DateTime();
                    today = DateTime.Now;
                    newUser.RecordDate = today;
                    context.Users.Add(newUser);
                    await context.SaveChangesAsync();
                    response.SetContent(true, "User Inserted Successfully");
                    //List<User> userList = new List<User>{newUser};
                    //response.Data = userList.ToList<object>();

                    Cart userCart = new Cart();
                    userCart.UserId = newUser.Id;
                    context.Carts.Add(userCart);
                    await context.SaveChangesAsync();

                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetInt32("IsLoggedIn", 1);
                    HttpContext.Session.SetInt32("UserID", newUser.Id);
                    HttpContext.Session.SetInt32("CartID", userCart.Id);
                    await HttpContext.Session.CommitAsync();

                    return response;
                }
                else
                {
                    response.SetContent(false, "User already exist");
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.SetContent(false, "EX: " + ex.Message + " INNER EX:" + ex.InnerException);
                return response;
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ResponseObject> Put(int id, [FromBody] User userToUpdate)
        {
            List<User> usersList = new List<User>();

            await HttpContext.Session.LoadAsync();
            var userId = HttpContext.Session.GetInt32("UserID") ?? id;

            //var entity = context.Users.FirstOrDefault(u => u.Id == id);
            try
            {
                var entity = context.Users.FirstOrDefault(u => u.Id == userId);

                response.SetContent(false, "Failed to Update account.");

                if (entity != null)
                {
                    entity.FirstName = userToUpdate.FirstName;
                    entity.LastName = userToUpdate.LastName;
                    entity.Telephone = userToUpdate.Telephone;
                    entity.Password = PasswordHash.HashPassword(userToUpdate.Password);

                    context.Users.Update(entity);
                    await context.SaveChangesAsync();
                    response.SetContent(true, "Updated Successfully");

                    List<User> userList = new List<User> { entity };
                    response.Data = userList.ToList<object>();
                }
            }
            catch (Exception ex)
            {
                response.SetContent(false, ex.Message);
                return response;
            }

            return response;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<ResponseObject> Delete(int id)
        {
            await HttpContext.Session.LoadAsync();
            var userId = 0;
            int isLoggedIn = HttpContext.Session.GetInt32("IsLoggedIn") ?? 0;
            var entity = context.Users.FirstOrDefault(u => u.Id == userId);

            try
            {

                if (isLoggedIn == 1)
                {
                    userId = HttpContext.Session.GetInt32("UserID") ?? 0;
                    entity = context.Users.FirstOrDefault(u => u.Id == userId);
                }
                else
                {
                    userId = id;
                    entity = await context.Users
                                    .Where(u => u.Id == id)
                                    .Where(e => e.IsRegistered == false)
                                    .SingleOrDefaultAsync();
                    context.Users.Remove(entity);
                    response.SetContent(true, "Deleted Successfully");
                    return response;
                }

                response.SetContent(false, "Failed to Delete account. Could not locate account");

                if (entity != null)
                {
                    context.Users.Remove(entity);
                    await context.SaveChangesAsync();
                    response.SetContent(true, "Deleted Successfully");
                    await HttpContext.Session.LoadAsync();
                    HttpContext.Session.SetInt32("UserID", 0);
                    HttpContext.Session.SetInt32("IsLoggedIn", 0);
                    await HttpContext.Session.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                response.SetContent(false, ex.Message);
                return response;
            }

            return response;
        }



        public bool UserExist(List<User> allUsers, User validateUser)
        {
            return !allUsers.Contains(validateUser) && allUsers.Any<User>(s => s.Email == validateUser.Email);
        }

        public bool ValidateUserDataOnPost(User user)
        {
            List<String> errorMessages = new List<string>();

            if (string.IsNullOrEmpty(user.FirstName)) errorMessages.Add("ERROR: Empty first name.");
            if (string.IsNullOrEmpty(user.LastName)) errorMessages.Add("ERROR: Empty last name.");
            if (string.IsNullOrEmpty(user.Email)) errorMessages.Add("ERROR: Empty email.");
            if (string.IsNullOrEmpty(user.Password)) errorMessages.Add("ERROR: Empty password.");
            //if (string.IsNullOrEmpty(user.DOB)) errorMessages.Add("ERROR: Empty DOB.");
            if (errorMessages.Count == 0)
                return true;
            else
                return false;
        }
    }
}