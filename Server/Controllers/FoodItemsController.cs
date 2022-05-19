using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    [Authorize(Policy = "Admin")]
    public class FoodItemsController : ControllerBase
    {
        private readonly FcmsContext _context;
        private readonly IWebHostEnvironment _env;
        public FoodItemsController(FcmsContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/FoodItems
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItem()
        {
          if (_context.FoodItem == null)
          {
              return NotFound();
          }
            return await _context.FoodItem.ToListAsync();
        }

        // GET: api/FoodItems/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodItem>> GetFoodItem(long id)
        {
          if (_context.FoodItem == null)
          {
              return NotFound();
          }
            var foodItem = await _context.FoodItem
                                .Where(fi => fi.Id == id)
                                .Include(fi => fi.FoodCategory)
                                .FirstOrDefaultAsync();

            if (foodItem == null)
            {
                return NotFound();
            }

            return foodItem;
        }

        // PUT: api/FoodItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodItem(long id, FoodItem foodItem)
        {
            if (id != foodItem.Id)
            {
                return BadRequest();
            }

            foodItem.UpdatedAt = DateTime.UtcNow;
            _context.Entry(foodItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(foodItem);
        }

        // POST: api/FoodItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<FoodItem>> PostFoodItem(FoodItem foodItem)
        {
          if (_context.FoodItem == null)
          {
              return Problem("Entity set 'FcmsContext.FoodItem'  is null.");
          }

            foodItem.CreatedAt = DateTime.UtcNow;
            _context.FoodItem.Add(foodItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFoodItem", new { id = foodItem.Id }, foodItem);
        }

        // DELETE: api/FoodItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodItem(long id)
        {
            if (_context.FoodItem == null)
            {
                return NotFound();
            }
            var foodItem = await _context.FoodItem.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            _context.FoodItem.Remove(foodItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateUniqueFileName(string fileName)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extName = Path.GetExtension(fileName);
            return baseName + "_" + Guid.NewGuid() + extName;
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new
                {
                    Error = "File is requiured."
                });
            }

            var strRegex = @"^image\/*";
            var regex = new Regex(strRegex);
            if (!regex.IsMatch(file.ContentType))
            {
                return BadRequest(new
                {
                    Error = "Upload an image."
                });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    Error = "File length should be less than or equal to 5MB."
                });
            }

            var uniqueFileName = GenerateUniqueFileName(file.FileName);
            var dirPath = Path.Combine(_env.WebRootPath, "Image");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var filePath = Path.Combine(dirPath, uniqueFileName);
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return Ok(new
            {
                Path = $"Image/{uniqueFileName}"
            });
        }

        [HttpPost("DeleteFile")]
        public IActionResult DeleteFile([FromBody] FileDelete fileDelete)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Error = "Input is invalid."
                });
            }

            System.IO.File.Delete(Path.Combine(_env.WebRootPath, fileDelete.Path));
            return Ok(new
            {
                Message = "File is deleted successfully."
            });
        }


        private bool FoodItemExists(long id)
        {
            return (_context.FoodItem?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
