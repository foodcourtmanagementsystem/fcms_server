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
    public class UserAddressController : ControllerBase
    {
        private readonly FcmsContext _context;

        public UserAddressController(FcmsContext context)
        {
            _context = context;
        }

        // GET: api/UserAddress
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAddress>>> GetUserAddress()
        {
          if (_context.UserAddress == null)
          {
              return NotFound();
          }
            return await _context.UserAddress.ToListAsync();
        }


        // PUT: api/UserAddress/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<UserAddress>> PutUserAddress(long id, UserAddress userAddress)
        {
            if (id != userAddress.Id)
            {
                return BadRequest();
            }

            var oldUserAddress = await _context.UserAddress.FindAsync(id);
            if(oldUserAddress == null)
            {
                return NotFound();
            }

            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;
            if(!userId.Equals(oldUserAddress.UserId))
            {
                return BadRequest();
            }

            oldUserAddress.PhoneNumber = userAddress.PhoneNumber;
            oldUserAddress.Address1 = userAddress.Address1;
            oldUserAddress.Address2 = userAddress.Address2;
            oldUserAddress.PinCode = userAddress.PinCode;
            oldUserAddress.City = userAddress.City;
            oldUserAddress.State = userAddress.State;
            oldUserAddress.Country = userAddress.Country;
            oldUserAddress.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return oldUserAddress;
        }

        // POST: api/UserAddress
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserAddress>> PostUserAddress(UserAddress userAddress)
        {
          if (_context.UserAddress == null)
          {
              return Problem("Entity set 'FcmsContext.UserAddress'  is null.");
          }

            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;
            userAddress.UserId = userId;
            userAddress.CreatedAt = DateTime.UtcNow;

            _context.UserAddress.Add(userAddress);
            await _context.SaveChangesAsync();

            return userAddress;
        }

        // DELETE: api/UserAddress/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAddress(long id)
        {
            if (_context.UserAddress == null)
            {
                return NotFound();
            }
            var userAddress = await _context.UserAddress.FindAsync(id);
            if (userAddress == null)
            {
                return NotFound();
            }

            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;
            if (!userId.Equals(userAddress.UserId))
            {
                return BadRequest();
            }

            _context.UserAddress.Remove(userAddress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserAddressExists(long id)
        {
            return (_context.UserAddress?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
