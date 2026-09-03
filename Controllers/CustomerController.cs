using dress_ordering_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace dress_ordering_system.Controllers
{
    public class CustomerController : Controller
    {
        private readonly myContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CustomerController(
            myContext context,
            IWebHostEnvironment env,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // =========================================================
        // HOME
        // =========================================================

        public IActionResult Index()
        {
            List<Category> category =
                _context.tbl_category.ToList();

            ViewData["category"] = category;

            List<Dress> dresses =
                _context.tbl_dress.ToList();

            ViewData["dress"] = dresses;

            ViewBag.checkSession =
                HttpContext.Session.GetString("customerSession");

            return View();
        }

        // =========================================================
        // LOGIN
        // =========================================================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(
            string customerEmail,
            string customerPassword)
        {
            var customer = _context.tbl_customer
                .FirstOrDefault(c =>
                    c.customer_email == customerEmail);

            if (customer != null &&
                customer.customer_password == customerPassword)
            {
                HttpContext.Session.SetString(
                    "customerSession",
                    customer.customer_id.ToString());

                return RedirectToAction("Index");
            }

            ViewBag.Error =
                "Invalid email or password";

            return View();
        }

        // =========================================================
        // CUSTOMER REGISTRATION
        // =========================================================

        public IActionResult CustomerRegistration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CustomerRegistration(
            Customer customer)
        {
            _context.tbl_customer.Add(customer);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        public IActionResult customerLogout()
        {
            HttpContext.Session.Remove(
                "customerSession");

            return RedirectToAction("Index");
        }

        // =========================================================
        // CUSTOMER PROFILE
        // =========================================================

        public IActionResult customerProfile()
        {
            string? customerSession =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerSession))
            {
                return RedirectToAction("Login");
            }

            ViewData["category"] =
                _context.tbl_category.ToList();

            int customerId =
                int.Parse(customerSession);

            var row = _context.tbl_customer
                .Where(c =>
                    c.customer_id == customerId)
                .ToList();

            return View(row);
        }

        // =========================================================
        // UPDATE CUSTOMER PROFILE
        // =========================================================

        [HttpPost]
        public IActionResult updateCustomerProfile(
            Customer customer)
        {
            _context.tbl_customer.Update(customer);
            _context.SaveChanges();

            return RedirectToAction("customerProfile");
        }

        // =========================================================
        // CHANGE PROFILE IMAGE
        // =========================================================

        public IActionResult changeProfileImage(
            Customer customer,
            IFormFile customer_image)
        {
            if (customer_image == null ||
                customer_image.Length == 0)
            {
                return RedirectToAction(
                    "customerProfile");
            }

            string imagePath =
                Path.Combine(
                    _env.WebRootPath,
                    "customer_image",
                    customer_image.FileName);

            using (FileStream fs =
                   new FileStream(
                       imagePath,
                       FileMode.Create))
            {
                customer_image.CopyTo(fs);
            }

            customer.customer_image =
                customer_image.FileName;

            _context.tbl_customer.Update(customer);
            _context.SaveChanges();

            return RedirectToAction(
                "customerProfile");
        }

        // =========================================================
        // FEEDBACK
        // =========================================================

        public IActionResult feedback()
        {
            ViewData["category"] =
                _context.tbl_category.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult feedback(
            Feedback feedback)
        {
            TempData["message"] =
                "Feedback Successfully Submitted";

            _context.tbl_feedback.Add(feedback);
            _context.SaveChanges();

            return RedirectToAction("feedback");
        }

        // =========================================================
        // ALL DRESSES
        // =========================================================

        public IActionResult fetchAllDress()
        {
            ViewData["category"] =
                _context.tbl_category.ToList();

            List<Dress> dresses =
                _context.tbl_dress.ToList();

            return View(dresses);
        }

        // =========================================================
        // CATEGORY PRODUCTS
        // =========================================================

        public IActionResult CategoryProducts(int id)
        {
            ViewData["category"] =
                _context.tbl_category.ToList();

            var selectedCategory =
                _context.tbl_category
                    .FirstOrDefault(c =>
                        c.category_id == id);

            if (selectedCategory == null)
            {
                return NotFound();
            }

            var dresses =
                _context.tbl_dress
                    .Where(d => d.cat_id == id)
                    .ToList();

            ViewBag.CategoryName =
                selectedCategory.category_name;

            return View(dresses);
        }

        // =========================================================
        // DRESS DETAILS
        // =========================================================

        public IActionResult DressDetails(int id)
        {
            ViewData["category"] =
                _context.tbl_category.ToList();

            var dress =
                _context.tbl_dress
                    .Where(p =>
                        p.dress_id == id)
                    .Include(p => p.Category)
                    .ToList();

            return View(dress);
        }

        // =========================================================
        // ADD TO CART
        // =========================================================

        public IActionResult AddToCart(int dress_id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var dress =
                _context.tbl_dress
                    .FirstOrDefault(d =>
                        d.dress_id == dress_id);

            if (dress == null)
            {
                return RedirectToAction(
                    "fetchAllDress");
            }

            // Check stock
            if (dress.dress_quantity <= 0)
            {
                TempData["message"] =
                    "This product is out of stock.";

                return RedirectToAction(
                    "fetchAllDress");
            }

            // Check existing cart item
            var existingCart =
                _context.tbl_cart
                    .FirstOrDefault(c =>
                        c.cust_id == customerIdInt &&
                        c.dress_id == dress_id);

            if (existingCart != null)
            {
                // Don't exceed available stock
                if (existingCart.dress_quantity <
                    dress.dress_quantity)
                {
                    existingCart.dress_quantity++;

                    _context.SaveChanges();

                    TempData["message"] =
                        "Product quantity increased.";
                }
                else
                {
                    TempData["message"] =
                        "You cannot add more than the available stock.";
                }
            }
            else
            {
                Cart cart = new Cart
                {
                    dress_id = dress_id,
                    cust_id = customerIdInt,
                    dress_quantity = 1,
                    cart_status = 0
                };

                _context.tbl_cart.Add(cart);
                _context.SaveChanges();

                TempData["message"] =
                    "Product successfully added to cart.";
            }

            return RedirectToAction("fetchCart");
        }

        // =========================================================
        // BUY NOW
        // =========================================================

        public IActionResult BuyNow(int dress_id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var dress =
                _context.tbl_dress
                    .FirstOrDefault(d =>
                        d.dress_id == dress_id);

            if (dress == null)
            {
                return RedirectToAction(
                    "fetchAllDress");
            }

            if (dress.dress_quantity <= 0)
            {
                TempData["message"] =
                    "This product is out of stock.";

                return RedirectToAction(
                    "fetchAllDress");
            }

            var existingCart =
                _context.tbl_cart
                    .FirstOrDefault(c =>
                        c.cust_id == customerIdInt &&
                        c.dress_id == dress_id);

            if (existingCart != null)
            {
                existingCart.dress_quantity = 1;
            }
            else
            {
                Cart cart = new Cart
                {
                    dress_id = dress_id,
                    cust_id = customerIdInt,
                    dress_quantity = 1,
                    cart_status = 0
                };

                _context.tbl_cart.Add(cart);
            }

            _context.SaveChanges();

            return RedirectToAction(
                "checkoutProduct");
        }

        // =========================================================
        // ADD TO WISHLIST
        // =========================================================

        public IActionResult AddToWishlist(int id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var dress =
                _context.tbl_dress
                    .FirstOrDefault(d =>
                        d.dress_id == id);

            if (dress == null)
            {
                return NotFound();
            }

            var existingWishlist =
                _context.tbl_wishlist
                    .FirstOrDefault(w =>
                        w.customer_id == customerIdInt &&
                        w.dress_id == id);

            if (existingWishlist == null)
            {
                Wishlist wishlist = new Wishlist
                {
                    customer_id = customerIdInt,
                    dress_id = id
                };

                _context.tbl_wishlist.Add(wishlist);
                _context.SaveChanges();

                TempData["WishlistMessage"] =
                    "Product added to wishlist ❤️";
            }
            else
            {
                TempData["WishlistMessage"] =
                    "Product is already in your wishlist ❤️";
            }

            return RedirectToAction(
                "DressDetails",
                new { id = id });
        }

        // =========================================================
        // WISHLIST
        // =========================================================

        public IActionResult Wishlist()
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            ViewData["category"] =
                _context.tbl_category.ToList();

            var wishlist =
                _context.tbl_wishlist
                    .Where(w =>
                        w.customer_id == customerIdInt)
                    .Include(w => w.Dress)
                    .ThenInclude(d => d.Category)
                    .ToList();

            return View(wishlist);
        }

        // =========================================================
        // REMOVE FROM WISHLIST
        // =========================================================

        public IActionResult RemoveFromWishlist(int id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var wishlist =
                _context.tbl_wishlist
                    .FirstOrDefault(w =>
                        w.wishlist_id == id &&
                        w.customer_id == customerIdInt);

            if (wishlist != null)
            {
                _context.tbl_wishlist.Remove(wishlist);
                _context.SaveChanges();
            }

            return RedirectToAction("Wishlist");
        }

        // =========================================================
        // FETCH CART
        // =========================================================

        public IActionResult fetchCart()
        {
            ViewData["category"] =
                _context.tbl_category.ToList();

            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var cart =
                _context.tbl_cart
                    .Where(c =>
                        c.cust_id == customerIdInt)
                    .Include(c => c.products)
                    .ToList();

            return View(cart);
        }

        // =========================================================
        // REMOVE PRODUCT FROM CART
        // =========================================================

        public IActionResult removeProduct(int id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var product =
                _context.tbl_cart
                    .FirstOrDefault(c =>
                        c.cart_id == id &&
                        c.cust_id == customerIdInt);

            if (product != null)
            {
                _context.tbl_cart.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("fetchCart");
        }

        // =========================================================
        // INCREASE CART QUANTITY
        // =========================================================

        [HttpPost]
        public IActionResult IncreaseQuantity(
            int cart_id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var cart =
                _context.tbl_cart
                    .Include(c => c.products)
                    .FirstOrDefault(c =>
                        c.cart_id == cart_id &&
                        c.cust_id == customerIdInt);

            if (cart == null)
            {
                return RedirectToAction("fetchCart");
            }

            if (cart.products == null)
            {
                TempData["message"] =
                    "Product information is missing.";

                return RedirectToAction("fetchCart");
            }

            if (cart.dress_quantity <
                cart.products.dress_quantity)
            {
                cart.dress_quantity++;

                _context.SaveChanges();
            }
            else
            {
                TempData["message"] =
                    "You cannot add more than the available stock.";
            }

            return RedirectToAction("fetchCart");
        }

        // =========================================================
        // DECREASE CART QUANTITY
        // =========================================================

        [HttpPost]
        public IActionResult DecreaseQuantity(
            int cart_id)
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var cart =
                _context.tbl_cart
                    .FirstOrDefault(c =>
                        c.cart_id == cart_id &&
                        c.cust_id == customerIdInt);

            if (cart == null)
            {
                return RedirectToAction("fetchCart");
            }

            // Minimum quantity is 1
            if (cart.dress_quantity > 1)
            {
                cart.dress_quantity--;

                _context.SaveChanges();
            }

            return RedirectToAction("fetchCart");
        }

        // =========================================================
        // CHECKOUT
        // =========================================================

        public IActionResult checkoutProduct()
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            ViewData["category"] =
                _context.tbl_category.ToList();

            int customerIdInt =
                int.Parse(customerId);

            var cart =
                _context.tbl_cart
                    .Where(c =>
                        c.cust_id == customerIdInt)
                    .Include(c => c.products)
                    .ToList();

            return View(cart);
        }

        // =========================================================
        // PLACE ORDER + SSLCOMMERZ
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            // -----------------------------------------------------
            // GET CUSTOMER SESSION
            // -----------------------------------------------------

            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            // -----------------------------------------------------
            // GET CART
            // -----------------------------------------------------

            var cartItems =
                _context.tbl_cart
                    .Where(c =>
                        c.cust_id == customerIdInt)
                    .Include(c => c.products)
                    .ToList();

            if (!cartItems.Any())
            {
                TempData["message"] =
                    "Your cart is empty.";

                return RedirectToAction("fetchCart");
            }

            // -----------------------------------------------------
            // CALCULATE TOTAL
            // -----------------------------------------------------

            decimal totalAmount = 0m;

            foreach (var item in cartItems)
            {
                if (item.products == null)
                {
                    TempData["message"] =
                        "Product information is missing.";

                    return RedirectToAction("fetchCart");
                }

                // Remove TK from price
                // Example:
                // "5000 TK" -> "5000"
                // "1500 tk" -> "1500"

                string priceText =
                    item.products.dress_price
                        .Replace("TK", "")
                        .Replace("tk", "")
                        .Trim();

                if (!decimal.TryParse(
                        priceText,
                        out decimal price))
                {
                    TempData["message"] =
                        "Invalid price for " +
                        item.products.dress_name;

                    return RedirectToAction("fetchCart");
                }

                // -------------------------------------------------
                // CHECK STOCK
                // -------------------------------------------------

                if (item.dress_quantity <= 0)
                {
                    TempData["message"] =
                        item.products.dress_name +
                        " has an invalid quantity.";

                    return RedirectToAction("fetchCart");
                }

                if (item.dress_quantity >
                    item.products.dress_quantity)
                {
                    TempData["message"] =
                        "Not enough stock available for " +
                        item.products.dress_name;

                    return RedirectToAction("fetchCart");
                }

                // -------------------------------------------------
                // ITEM TOTAL
                // -------------------------------------------------

                decimal itemTotal =
                    item.dress_quantity * price;

                totalAmount += itemTotal;
            }

            // -----------------------------------------------------
            // GET CUSTOMER
            // -----------------------------------------------------

            var customer =
                _context.tbl_customer
                    .FirstOrDefault(c =>
                        c.customer_id == customerIdInt);

            if (customer == null)
            {
                return RedirectToAction("Login");
            }

            // -----------------------------------------------------
            // CREATE ORDER
            // -----------------------------------------------------

            Order order = new Order
            {
                cust_id = customerIdInt,
                order_date = DateTime.Now,
                total_amount = totalAmount,
                status = "Pending"
            };

            _context.tbl_order.Add(order);

            _context.SaveChanges();

            // -----------------------------------------------------
            // CREATE TRANSACTION ID
            // -----------------------------------------------------

            string transactionId =
                "DRESS_" +
                order.order_id +
                "_" +
                DateTime.Now.ToString(
                    "yyyyMMddHHmmss");

            // -----------------------------------------------------
            // GET SSLCOMMERZ SETTINGS
            // -----------------------------------------------------

            string storeId =
                _configuration[
                    "SSLCommerz:StoreId"] ?? "";

            string storePassword =
                _configuration[
                    "SSLCommerz:StorePassword"] ?? "";

            string sessionApi =
                _configuration[
                    "SSLCommerz:SessionApi"] ?? "";

            if (string.IsNullOrWhiteSpace(
                    storeId) ||
                string.IsNullOrWhiteSpace(
                    storePassword) ||
                string.IsNullOrWhiteSpace(
                    sessionApi))
            {
                return Content(
                    "SSLCOMMERZ settings are not configured correctly.");
            }

            // -----------------------------------------------------
            // CREATE HTTP CLIENT
            // -----------------------------------------------------

            var client =
                _httpClientFactory.CreateClient();

            // -----------------------------------------------------
            // PAYMENT INFORMATION
            // -----------------------------------------------------

            string successUrl =
                Url.Action(
                    "PaymentSuccess",
                    "Payment",
                    null,
                    Request.Scheme) ?? "";

            string failUrl =
                Url.Action(
                    "PaymentFail",
                    "Payment",
                    null,
                    Request.Scheme) ?? "";

            string cancelUrl =
                Url.Action(
                    "PaymentCancel",
                    "Payment",
                    null,
                    Request.Scheme) ?? "";

            string ipnUrl =
                Url.Action(
                    "IPN",
                    "Payment",
                    null,
                    Request.Scheme) ?? "";

            var values =
                new Dictionary<string, string>
                {
                    {
                        "store_id",
                        storeId
                    },

                    {
                        "store_passwd",
                        storePassword
                    },

                    {
                        "total_amount",
                        totalAmount.ToString("0.00")
                    },

                    {
                        "currency",
                        "BDT"
                    },

                    {
                        "tran_id",
                        transactionId
                    },

                    {
                        "value_a",
                        order.order_id.ToString()
                    },

                    {
                        "success_url",
                        successUrl
                    },

                    {
                        "fail_url",
                        failUrl
                    },

                    {
                        "cancel_url",
                        cancelUrl
                    },

                    {
                        "ipn_url",
                        ipnUrl
                    },

                    {
                        "cus_name",
                        customer.customer_name
                    },

                    {
                        "cus_email",
                        customer.customer_email
                    },

                    {
                        "cus_phone",
                        customer.customer_phone
                            ?? "01700000000"
                    },

                    {
                        "cus_add1",
                        customer.customer_address
                            ?? "Dhaka"
                    },

                    {
                        "cus_city",
                        customer.customer_city
                            ?? "Dhaka"
                    },

                    {
                        "cus_country",
                        customer.customer_country
                            ?? "Bangladesh"
                    },

                    {
                        "shipping_method",
                        "NO"
                    },

                    {
                        "product_name",
                        "Dress"
                    },

                    {
                        "product_category",
                        "Fashion"
                    },

                    {
                        "product_profile",
                        "general"
                    },

                    {
                        "num_of_item",
                        cartItems.Count.ToString()
                    }
                };

            // -----------------------------------------------------
            // SEND REQUEST TO SSLCOMMERZ
            // -----------------------------------------------------

            using var content =
                new FormUrlEncodedContent(values);

            HttpResponseMessage response;

            try
            {
                response =
                    await client.PostAsync(
                        sessionApi,
                        content);
            }
            catch (Exception ex)
            {
                return Content(
                    "Unable to connect to SSLCOMMERZ.\n\n" +
                    ex.Message);
            }

            string responseString =
                await response.Content
                    .ReadAsStringAsync();

            // -----------------------------------------------------
            // EMPTY RESPONSE
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    responseString))
            {
                return Content(
                    "SSLCOMMERZ returned an empty response.");
            }

            // -----------------------------------------------------
            // HTML RESPONSE
            // -----------------------------------------------------

            if (responseString
                .TrimStart()
                .StartsWith("<"))
            {
                return Content(
                    "SSLCOMMERZ returned HTML instead of JSON.\n\n" +
                    "HTTP Status: " +
                    response.StatusCode +
                    "\n\nResponse:\n" +
                    responseString);
            }

            // -----------------------------------------------------
            // HTTP ERROR
            // -----------------------------------------------------

            if (!response.IsSuccessStatusCode)
            {
                return Content(
                    "SSLCOMMERZ connection failed.\n\n" +
                    "HTTP Status: " +
                    response.StatusCode +
                    "\n\nResponse:\n" +
                    responseString);
            }

            // -----------------------------------------------------
            // DESERIALIZE RESPONSE
            // -----------------------------------------------------

            PaymentResponse? paymentResponse;

            try
            {
                paymentResponse =
                    JsonSerializer.Deserialize<PaymentResponse>(
                        responseString,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch (JsonException)
            {
                return Content(
                    "SSLCOMMERZ returned an unexpected response.\n\n" +
                    responseString);
            }

            // -----------------------------------------------------
            // CHECK PAYMENT URL
            // -----------------------------------------------------

            if (paymentResponse == null ||
                string.IsNullOrWhiteSpace(
                    paymentResponse.GatewayPageURL))
            {
                return Content(
                    "SSLCOMMERZ payment session failed.\n\n" +
                    responseString);
            }

            // -----------------------------------------------------
            // SAVE PAYMENT INFORMATION
            // -----------------------------------------------------

            HttpContext.Session.SetString(
                "SSLTransactionId",
                transactionId);

            HttpContext.Session.SetString(
                "CurrentOrderId",
                order.order_id.ToString());

            // -----------------------------------------------------
            // REDIRECT TO PAYMENT
            // -----------------------------------------------------

            return Redirect(
                paymentResponse.GatewayPageURL);
        }

        // =========================================================
        // ORDER SUCCESS
        // =========================================================

        public IActionResult OrderSuccess()
        {
            ViewData["category"] =
                _context.tbl_category.ToList();

            return View();
        }

        // =========================================================
        // MY ORDERS
        // =========================================================

        public IActionResult MyOrders()
        {
            string? customerId =
                HttpContext.Session.GetString(
                    "customerSession");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login");
            }

            int customerIdInt =
                int.Parse(customerId);

            var orders =
                _context.tbl_order
                    .Where(o =>
                        o.cust_id == customerIdInt)
                    .OrderByDescending(
                        o => o.order_date)
                    .ToList();

            ViewData["category"] =
                _context.tbl_category.ToList();

            return View(orders);
        }

        // =========================================================
        // BLOG
        // =========================================================

        public IActionResult Blog()
        {
            var blogs =
                _context.tbl_blog
                    .OrderByDescending(
                        b => b.created_date)
                    .ToList();

            ViewData["category"] =
                _context.tbl_category.ToList();

            return View(blogs);
        }

        // =========================================================
        // BLOG DETAILS
        // =========================================================

        public IActionResult BlogDetails(int id)
        {
            var blog =
                _context.tbl_blog
                    .FirstOrDefault(
                        b => b.blog_id == id);

            ViewData["category"] =
                _context.tbl_category.ToList();

            return View(blog);
        }
    }
}