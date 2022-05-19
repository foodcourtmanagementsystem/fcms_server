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
    public class FoodCategoriesController : ControllerBase
    {
        private readonly FcmsContext _context;
        private readonly IWebHostEnvironment _env;

        public FoodCategoriesController(FcmsContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/FoodCategories
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FoodCategory>>> GetFoodCategory()
        {
          if (_context.FoodCategory == null)
          {
              return NotFound();
          }
            return await _context.FoodCategory.ToListAsync();
        }

        // GET: api/FoodCategories/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FoodCategory>> GetFoodCategory(long id)
        {
          if (_context.FoodCategory == null)
          {
              return NotFound();
          }
            var foodCategory = await _context.FoodCategory
                                    .Where(fc => fc.Id == id)
                                    .Include(fc => fc.FoodItems)
                                    .FirstOrDefaultAsync();

            if (foodCategory == null)
            {
                return NotFound();
            }

            return foodCategory;
        }

        // PUT: api/FoodCategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFoodCategory(long id, FoodCategory foodCategory)
        {
            if (id != foodCategory.Id)
            {
                return BadRequest();
            }

            foodCategory.UpdatedAt = DateTime.UtcNow;
            _context.Entry(foodCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!FoodCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(new
                    {
                        Error = "Please choose a different food category title."
                    });
                }
            }

            return Ok(foodCategory);
        }

        // POST: api/FoodCategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<FoodCategory>> PostFoodCategory(FoodCategory foodCategory)
        {
          if (_context.FoodCategory == null)
          {
              return Problem("Entity set 'FcmsContext.FoodCategory'  is null.");
          }
          
            try
            {
                foodCategory.CreatedAt = DateTime.UtcNow;
                _context.FoodCategory.Add(foodCategory);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetFoodCategory", new { id = foodCategory.Id }, foodCategory);
            }
            catch(Exception)
            {
                return BadRequest(new
                {
                    Error = "Please choose a different food category title."
                });
            }
           
        }

        // DELETE: api/FoodCategories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFoodCategory(long id)
        {
            if (_context.FoodCategory == null)
            {
                return NotFound();
            }
            var foodCategory = await _context.FoodCategory.FindAsync(id);
            if (foodCategory == null)
            {
                return NotFound();
            }

            _context.FoodCategory.Remove(foodCategory);
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
            if(file == null)
            {
                return BadRequest(new
                {
                    Error = "File is requiured."
                });
            }

            var strRegex = @"^image\/*";
            var regex = new Regex(strRegex);
            if(!regex.IsMatch(file.ContentType))
            {
                return BadRequest(new
                {
                    Error = "Upload an image."
                });
            }

            if(file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    Error = "File length should be less than or equal to 5MB."
                });
            }

            var uniqueFileName = GenerateUniqueFileName(file.FileName);
            var dirPath = Path.Combine(_env.WebRootPath, "Image");
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var filePath = Path.Combine(dirPath, uniqueFileName);
            using(var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            return Ok(new {
                Path = $"Image/{uniqueFileName}"
            });
        }

        [HttpPost("DeleteFile")]
        public IActionResult DeleteFile([FromBody]FileDelete fileDelete)
        {
            if(!ModelState.IsValid)
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


        private bool FoodCategoryExists(long id)
        {
            return (_context.FoodCategory?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
