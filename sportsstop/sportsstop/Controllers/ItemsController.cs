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
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private ResponseObject response;

        public ItemsController(AppDbContext appDbContext)
        {
            response = new ResponseObject();
            this.appDbContext = appDbContext;
        }

        // GET: api/Items
        [HttpGet]
        public async Task<ResponseObject> Get()
        {
            List<Item> items = await appDbContext.Items.Include(ic => ic.ItemComments).ToListAsync();
            response.SetContent(true, "Items returned successfully", items.ToList<object>());
            return response;
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ResponseObject> Get(int id)
        {
            try
            {
                List<Item> items = new List<Item>() {
                    await appDbContext.Items.Include(ic=>ic.ItemComments).SingleAsync(i => i.Id == id)
                };
                response.SetContent(true, "Item returned successfully", items.ToList<object>());
            }
            catch (Exception e)
            {
                response.SetContent(false, "Item not found.");
            }
            return response;
        }

        // POST: api/Items
        [HttpPost]
        public async Task<ResponseObject> Post([FromBody] Item item)
        {
            try
            {
                if (!item.GetType().GetProperties().All(p => p == null))
                {
                    appDbContext.Items.Add(item);
                    await appDbContext.SaveChangesAsync();
                    response.SetContent(true, "Item added successfully");
                }
                else
                {
                    response.SetContent(false, "Please provide all item details");
                }
            }
            catch (Exception e)
            {
                response.SetContent(false, e.Message + " -> " + e.InnerException);
            }
            return response;
        }

        // PUT: api/Items/5
        [HttpPut("{id}")]
        public async Task<ResponseObject> Put(int id, [FromBody] Item itemToUpdate)
        {
            try
            {
                if (!itemToUpdate.GetType().GetProperties().All(ip => ip == null))
                {
                    var oldItem = await appDbContext.Items.SingleOrDefaultAsync(i => i.Id == id);
                    if (oldItem != null)
                    {
                        oldItem.Name = itemToUpdate.Name;
                        oldItem.Description = itemToUpdate.Description;
                        oldItem.ImagePath = itemToUpdate.ImagePath;
                        oldItem.Price = itemToUpdate.Price;
                       // oldItem.Weight = itemToUpdate.Weight;
                        oldItem.StockQty = itemToUpdate.StockQty;
                        oldItem.Tax = itemToUpdate.Tax;
                        oldItem.ShippingCost = itemToUpdate.ShippingCost;

                        await appDbContext.SaveChangesAsync();
                        response.SetContent(true, "Item updated successfully");
                    }
                    else
                        response.SetContent(false, "Item not found");
                }
                else
                {
                    response.SetContent(false, "Please provide all the details of the item");
                }
            }
            catch (Exception e)
            {
                response.SetContent(false, e.Message + " -> " + e.InnerException);
            }
            return response;
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<ResponseObject> Delete(int id)
        {
            try
            {
                appDbContext.Items.Remove(appDbContext.Items.SingleOrDefault(i => i.Id == id));
                await appDbContext.SaveChangesAsync();
                response.SetContent(true, "Item deleted successfully");
            }
            catch (Exception e)
            {
                response.SetContent(false, e.Message + " -> " + e.InnerException);
            }
            return response;
        }
    }
}

//{
//	"name":"2019 Audi SQ5 3.0T",
//	"description":"The TRUE Audi Experience.",
//	"imagePath":"/images/1000.jpg",
//	"price":7260.5,
//	"weight":1500,
//	"stockQty":10,
//	"tax":"943.87",
//	"shippingCost":605
//}

//{
//	"name":"2019 Audi Q8 3.0T",
//	"description":"Born from a brand famous for iconic designs, the Q",
//	"imagePath":"/images/1001.jpg",
//	"price":9500.99,
//	"weight":1852,
//	"stockQty":2,
//	"tax":"1235.13",
//	"shippingCost":641
//}

//{
//	"name":"2015 Audi Q3 2.0T",
//	"description":"2015 Audi Q3 FWD Navigation FrontTrak 4 Door 2.0T progressiv, Engine: 2.0 TFSI 4 Cylinder 200 HP, Axle Ratio: TBD.",
//	"imagePath":"/images/1002.jpg",
//	"price":1999,
//	"weight":2103,
//	"stockQty":2,
//	"tax":"259.87",
//	"shippingCost":119
//}

//{
//	"name":"2019 Audi Q8 3.0T",
//	"description":"3.0T TECHNIK QUATTRO 8SP TIPTRONIC, V6 Cylinder Engine",
//	"imagePath":"/images/1003.jpg",
//	"price":102299.99,
//	"weight":2290,
//	"stockQty":1,
//	"tax":"13298.99",
//	"shippingCost":5584
//}

//{
//	"name":"2016 Audi S5 Technik",
//	"description":"Used accident free 2016 Audi S5 Technik. Features include automatic transmission with paddle-shift override, keyless remote entry, push button start",
//	"imagePath":"/images/1004.jpg",
//	"price":39628.99,
//	"weight":1204,
//	"stockQty":3,
//	"tax":"5151.77",
//	"shippingCost":4114
//}