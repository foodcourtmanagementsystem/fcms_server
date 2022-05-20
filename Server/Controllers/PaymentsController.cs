using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;
using Stripe;


namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IStripeClient _client;
        private readonly FcmsContext _context;

        public PaymentsController(IConfiguration configuration, 
                                  IStripeClient client,
                                  FcmsContext context)
        {
            _configuration = configuration;
            _client = client;
            _context = context;
        }
      
        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("UserId")).Value;

            var cartSessionId = Request.Cookies["CartSessionId"];
            if(cartSessionId == null)
            {
                return BadRequest();
            }

            var cartItems = await _context.CartItem.Where(ci => ci.CartSessionId == cartSessionId)
                                                    .Include(ci => ci.FoodItem).ToListAsync();

            if(cartItems.Count() < 1)
            {
                return BadRequest();
            }
            var amount = cartItems.Sum(ci => ci.FoodItem.Price * ci.Quantity) * 100; // in Paise
            if(amount < 1)
            {
                return BadRequest(new
                {
                    Error = "Amount can not be less than 1."
                });
            }
           

            var orderItems = new List<Models.OrderItem>();
            foreach (var cartItem in cartItems)
            {
                var orderItem = new Models.OrderItem
                {
                    FoodItemId = cartItem.FoodItemId,
                    Quantity = cartItem.Quantity,
                    UserId = userId,
                    Status = "The order has been created.",
                    CreatedAt = DateTime.UtcNow
                };
                 orderItems.Add(orderItem);
                _context.OrderItem.Add(orderItem);
                _context.CartItem.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            var metaData = new Dictionary<string, string>();
            foreach(var orderItem in orderItems)
            {
                metaData.Add(orderItem.Id.ToString(), "OrderItemId");
            }
            metaData.Add("UserId", userId);
        
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long) amount,
                Currency = "inr",
                Metadata = metaData
            };

            var service = new PaymentIntentService(_client);
            var paymentIntent = await service.CreateAsync(options);
        

            return Ok(new
            {
                clientSecret = paymentIntent.ClientSecret
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                                        Request.Headers["Stripe-Signature"],
                                        _configuration["Stripe:WebhookSigningKey"],
                                        throwOnApiVersionMismatch: false
                                        );

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    string userId = null;
                    var orderItemIds = new List<long>();

                    foreach (var keyValuePair in paymentIntent.Metadata)
                    {
                        if (keyValuePair.Key.Equals("UserId"))
                        {
                            userId = keyValuePair.Value;
                        }
                        else if (keyValuePair.Value.Equals("OrderItemId"))
                        {
                            var orderItemId = long.Parse(keyValuePair.Key);
                            orderItemIds.Add(orderItemId);
                        }

                    }

                    var orderItems = new List<Models.OrderItem>();
                    orderItemIds.ForEach(orderItemId =>
                    {
                        orderItems.Add(_context.OrderItem.Find(orderItemId));
                    });

                    orderItems.ForEach(orderItem =>
                    {
                        orderItem.PaymentId = paymentIntent.Id;
                        orderItem.Status = "The order has been paid.";
                    });

                    await _context.SaveChangesAsync();
                   
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    Error = "Server error."
                });
            }
        }
    }
}
