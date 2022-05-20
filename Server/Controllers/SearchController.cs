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
    public class SearchController : ControllerBase
    {
        private readonly FcmsContext _context;

        public SearchController(FcmsContext context)
        {
            _context = context;
        }

        // GET: api/Search
        [HttpGet]
        public async Task<IActionResult> GetSearchResult(string query)
        {
          if (_context.FoodItem == null)
          {
              return NotFound();
          }

          if(string.IsNullOrEmpty(query))
          {
                return BadRequest();
          }

            return Ok(await _context.FoodItem.Where(fi => fi.Title.Contains(query) || fi.Description.Contains(query)).Include(fi => fi.FoodCategory).ToListAsync());
        }

       
    }
}
