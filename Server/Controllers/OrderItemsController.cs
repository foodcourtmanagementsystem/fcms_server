using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderItemsController : ControllerBase
    {
        private readonly FcmsContext _context;

        public OrderItemsController(FcmsContext context)
        {
            _context = context;
        }

        // GET: api/OrderItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItem()
        {
          if (_context.OrderItem == null)
          {
              return NotFound();
          }
            string userId = HttpContext.User.Claims.FirstOrDefault(claim => claim.Type.Equals("UserId")).Value;
            return await _context.OrderItem.Where(oi => oi.UserId == userId).Include(oi => oi.FoodItem)
                .ThenInclude(fi => fi.FoodCategory).OrderByDescending(oi => oi.CreatedAt).ToListAsync();
        }

        [HttpGet("Admin")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItemByAdmin()
        {
            if (_context.OrderItem == null)
            {
                return NotFound();
            }
          
            return await _context.OrderItem.Include(oi => oi.User).ThenInclude(user => user.UserAddress).Include(oi => oi.FoodItem)
                .ThenInclude(fi => fi.FoodCategory).OrderByDescending(oi => oi.CreatedAt).ToListAsync();
        }

        // PUT: api/OrderItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> PutOrderItem(long id, OrderItem orderItem)
        {
            if (id != orderItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(orderItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/OrderItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<OrderItem>> PostOrderItem(OrderItem orderItem)
        {
          if (_context.OrderItem == null)
          {
              return Problem("Entity set 'FcmsContext.OrderItem'  is null.");
          }
            _context.OrderItem.Add(orderItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrderItem", new { id = orderItem.Id }, orderItem);
        }

        // DELETE: api/OrderItems/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteOrderItem(long id)
        {
            if (_context.OrderItem == null)
            {
                return NotFound();
            }
            var orderItem = await _context.OrderItem.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItem.Remove(orderItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderItemExists(long id)
        {
            return (_context.OrderItem?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
