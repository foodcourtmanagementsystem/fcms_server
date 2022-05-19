using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : ControllerBase
    {
        private readonly FcmsContext _context;

        public CartItemsController(FcmsContext context)
        {
            _context = context;
        }

        // GET: api/CartItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItem()
        {
          if (_context.CartItem == null)
          {
              return NotFound();
          }
            
          if(!IsCookieAvailable())
          {
                return new List<CartItem>(); 
          }

           var cartSessionId = Request.Cookies["CartSessionId"];
            
           return await _context.CartItem.Where(ci => ci.CartSessionId.Equals(cartSessionId)).Include(ci => ci.FoodItem).ThenInclude(fi => fi.FoodCategory).ToListAsync();
        }

        // GET: api/CartItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CartItem>> GetCartItem(long id)
        {
          if (_context.CartItem == null)
          {
              return NotFound();
          }

            if (!IsCookieAvailable())
            {
                return BadRequest();
            }

            var cartItem = await _context.CartItem.Where(ci => ci.Id == id).Include(ci => ci.FoodItem).ThenInclude(fi => fi.FoodCategory).FirstOrDefaultAsync();

            if (cartItem == null)
            {
                return NotFound();
            }

            return cartItem;
        }

        // PUT: api/CartItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<CartItem>> PutCartItem(long id, CartItem cartItem)
        {
            if (id != cartItem.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsCookieAvailable())
            {
                return BadRequest();
            }

            var cartSessionId = Request.Cookies["CartSessionId"];

            var oldCartItem = await _context.CartItem.FindAsync(cartItem.Id);
            if (!cartSessionId.Equals(oldCartItem.CartSessionId))
            {
                return BadRequest();
            }

            oldCartItem.Quantity = cartItem.Quantity;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return await GetCartItem(id);
        }

        // POST: api/CartItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CartItem>> PostCartItem(CartItem cartItem)
        {
          if (_context.CartItem == null)
          {
              return Problem("Entity set 'FcmsContext.CartItem'  is null.");
          }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cartSessionId = null;
            if(IsCookieAvailable())
            {
                cartSessionId = Request.Cookies["CartSessionId"];
            }

            if(cartSessionId == null)
            {
                cartSessionId = Guid.NewGuid().ToString();
                Response.Cookies.Append("CartSessionId", cartSessionId, new CookieOptions
                {
                    MaxAge = TimeSpan.FromDays(365)
                });
            }
            cartItem.CartSessionId = cartSessionId;

            if(_context.CartItem.Any(ci => ci.FoodItemId == cartItem.FoodItemId && ci.CartSessionId == cartSessionId))
            {
                return BadRequest();
            }

            _context.CartItem.Add(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GetCartItem), new
                    {
                        id = cartItem.Id,
                    });
        }

        // DELETE: api/CartItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(long id)
        {
            if (_context.CartItem == null)
            {
                return NotFound();
            }

            if (!IsCookieAvailable())
            {
                return BadRequest();
            }

            var cartItem = await _context.CartItem.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            var cartSessionId = Request.Cookies["CartSessionId"];
            if(!cartSessionId.Equals(cartItem.CartSessionId))
            {
                return BadRequest();
            }

            _context.CartItem.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartItemExists(long id)
        {
            return (_context.CartItem?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool IsCookieAvailable()
        {
            return Request.Cookies.ContainsKey("CartSessionId");
        }

        
    }
}
