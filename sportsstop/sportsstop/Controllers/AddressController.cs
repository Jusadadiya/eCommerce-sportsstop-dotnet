using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sportsstop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace sportsstop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly ResponseObject response;

        public AddressController(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
            response = new ResponseObject();
            response.Status = false;
        }

        // GET: api/Address
        [HttpGet]
        public async Task<ResponseObject> Get()
        {
            await HttpContext.Session.LoadAsync();
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                try
                {
                    response.Message = "All Addresses retrived";
                    List<Address> addresses = await appDbContext.Addresses.ToListAsync();
                    response.Data = addresses.ToList<object>();
                    response.Status = true;
                }
                catch (Exception e)
                {
                    response.Message = e.Message;
                    response.Status = false;
                }
            }
            else
            {
                response.Message = "Unauthorized access not allowed";
                response.Status = false;
            }
            return response;
        }

        // GET: api/Address/5
        [HttpGet("{id}")]
        public async Task<ResponseObject> Get(int id)
        {
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                try
                {
                    var address = await appDbContext.Addresses
                        .Where<Address>(a => a.Id == id)
                        .Select(add => new
                        {
                            Address1 = add.Address1,
                            Address2 = add.Address2,
                            Postal = add.Postal,
                            City = add.City,
                            Country = add.Country
                        })
                        .SingleOrDefaultAsync();

                    if (address != null)
                    {
                        response.SetContent(true, "address returned successfully", new List<object>() { address });
                    }
                    else
                    {
                        response.SetContent(false, "Address not found");
                    }
                }
                catch (Exception e)
                {
                    response.Message = e.Message + " -> " + e.InnerException;
                    response.Status = false;
                }
            }
            else
            {
                response.Message = "Unauthorized access not allowed";
                response.Status = false;
            }
            return response;
        }

        // POST: api/Address
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] Address address)
        {
            await HttpContext.Session.LoadAsync();
            try
            {
                if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
                {
                    if (address != null)
                    {
                        if (address.Address1 == null || address.City == null || address.Country == null || address.Postal == null)
                        {
                            response.Message = "Please enter all the details";
                        }
                        else
                        {
                            address.UserId = HttpContext.Session.GetInt32("UserID") ?? 0;
                           // address.IsDefaultShipping = false;
                            appDbContext.Addresses.Add(address);
                            await appDbContext.SaveChangesAsync();
                            response.Message = "Address added successfully";
                            response.Status = true;
                        }
                    }
                    else
                    {
                        response.Message = "Provide address details";
                        response.Status = false;
                    }
                }
                else
                {
                    response.Message = "Unauthorized access not allowed";
                    response.Status = false;
                }
            }
            catch (Exception e)
            {
                response.Status = false;
                response.Message = e.Message + " -> " + e.InnerException;
            }
            return response;
        }

        // PUT: api/Address/5
        [HttpPut("{id}")]
        public async Task<ResponseObject> Put(int id, [FromBody] Address address)
        {
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                try
                {
                    var oldAddress = await appDbContext.Addresses.SingleOrDefaultAsync(i => i.Id == id);
                    if (!address.GetType().GetProperties().All(a => a == null))
                    {
                        oldAddress.Address1 = address.Address1;
                        oldAddress.Address2 = address.Address2;
                        oldAddress.City = address.City;
                        oldAddress.Country = address.Country;
                        oldAddress.Postal = address.Postal;
                        //oldAddress.IsDefaultShipping = address.IsDefaultShipping;

                        await appDbContext.SaveChangesAsync();
                        response.SetContent(true, "Address updated");
                    }
                    else
                    {
                        response.SetContent(false, "Please provide all the details of address");
                    }
                }
                catch (Exception e)
                {
                    response.Status = false;
                    response.Message = e.Message + " -> " + e.InnerException;
                }
            }
            else
            {
                response.Message = "Unauthorized access not allowed";
                response.Status = false;
            }
            return response;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<ResponseObject> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("IsLoggedIn") == 1)
            {
                try
                {
                    var address = await appDbContext.Addresses.FindAsync(id);

                    if (address == null)
                    {
                        response.Message = "Address not found in database";
                        response.Status = false;
                    }
                    else
                    {
                        if (address != null)
                        {
                            appDbContext.Addresses.Remove(address);
                            await appDbContext.SaveChangesAsync();
                            response.SetContent(true, "Address deleted successfully");
                        }
                        else
                            response.SetContent(false, "Address not found");
                    }
                }
                catch (Exception e)
                {
                    response.Status = false;
                    response.Message = e.Message + " -> " + e.InnerException;
                }
            }
            else
            {
                response.Message = "Unauthorized access not allowed";
                response.Status = false;
            }
            return response;
        }
    }
}