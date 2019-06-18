using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eCommerceCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerceCore.Controllers
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
            //try
            //{
            //    response.Message = "All Addresses retrived";
            //    response.Data = await appDbContext.Addresses.ToListAsync();
            //    response.Status = true;
            //    return response;
            //}
            //catch (Exception e)
            //{
            //    response.Message = e.Message;
                return response;
            //}
        }

        // GET: api/Address/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Address
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] Address address)
        {
            //try
            //{
            //    if (address != null)
            //    {
            //        if (address.Address1 == null && address.City == null && address.Country == null && address.Postal == null)
            //        {
            //            response.Message = "Please enter all the details";
            //        }
            //        else
            //        {
            //            address.IsDefaultShipping = false;
            //            appDbContext.Addresses.Add(address);
            //            await appDbContext.SaveChangesAsync();
            //            response.Message = "Address added successfully";
            //            response.Status = true;
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    response.Message = e.Message;
            //}
            return response;
        }

        // PUT: api/Address/5
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
