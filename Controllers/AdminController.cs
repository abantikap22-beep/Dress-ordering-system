using dress_ordering_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dress_ordering_system.Controllers
{
    public class AdminController : Controller
    {
        private myContext _context;
        private IWebHostEnvironment _env;

        public AdminController(myContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");

            if (string.IsNullOrEmpty(admin_session))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string adminEmail, string adminPassword)
        {
            // 1️⃣ Empty field validation
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                ViewBag.message = "Email and Password are required!";
                return View();
            }

            // 2️⃣ Email condition (example: only admin emails)
            if (!adminEmail.EndsWith("@company.com"))
            {
                ViewBag.message = "Only company email is allowed!";
                return View();
            }

            // 3️⃣ Password condition
            if (adminPassword.Length < 8)
            {
                ViewBag.message = "Password must be at least 8 characters long!";
                return View();
            }

            // 4️⃣ Database check
            var row = _context.tbl_admin.FirstOrDefault(a => a.admin_email == adminEmail);

            if (row == null || row.admin_password != adminPassword)
            {
                ViewBag.message = "Incorrect Username or Password";
                return View();
            }

            // 5️⃣ Login success
            HttpContext.Session.SetString("admin_session", row.admin_id.ToString());
            return RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();   // Clears all session data
            return RedirectToAction("Login");
        }

        // ================= ADMIN PROFILE =================

        public IActionResult Profile()
        {
            var adminIdStr = HttpContext.Session.GetString("admin_session");

            if (string.IsNullOrEmpty(adminIdStr))
                return RedirectToAction("Login");

            int adminId = int.Parse(adminIdStr);

            var row = _context.tbl_admin
                              .Where(a => a.admin_id == adminId)
                              .ToList();

            return View(row);
        }


        [HttpPost]
        public IActionResult Profile(Admin admin)
        {
            _context.tbl_admin.Update(admin);
            _context.SaveChanges();
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult ChangeProfileImage(IFormFile admin_image, Admin admin)
        {
            if (admin_image != null && admin_image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "admin_image");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = admin_image.FileName;
                string imagePath = Path.Combine(folderPath, fileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    admin_image.CopyTo(fs);
                }

                admin.admin_image = fileName;
            }

            _context.tbl_admin.Update(admin);
            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

        // ================= CUSTOMER =================

        public IActionResult fetchCustomer()
        {
            return View(_context.tbl_customer.ToList());
        }

        public IActionResult customerDetails(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
        }

        public IActionResult updateCustomer(int id)
        {
            return View(_context.tbl_customer.Find(id));
        }

        // 🔥 FIXED METHOD (MINIMAL CHANGE)
        [HttpPost]
        public IActionResult updateCustomer(Customer customer, IFormFile customer_image)
        {
            var existingCustomer = _context.tbl_customer
                .FirstOrDefault(c => c.customer_id == customer.customer_id);

            if (existingCustomer == null)
            {
                return RedirectToAction("fetchCustomer");
            }

            // Update normal fields
            existingCustomer.customer_name = customer.customer_name;
            existingCustomer.customer_phone = customer.customer_phone;
            existingCustomer.customer_email = customer.customer_email;
            existingCustomer.customer_password = customer.customer_password;
            existingCustomer.customer_country = customer.customer_country;
            existingCustomer.customer_city = customer.customer_city;
            existingCustomer.customer_address = customer.customer_address;
            existingCustomer.customer_gender = customer.customer_gender;

            // Update image ONLY if new file selected
            if (customer_image != null && customer_image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "customer_image");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = customer_image.FileName;
                string imagePath = Path.Combine(folderPath, fileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    customer_image.CopyTo(fs);
                }

                existingCustomer.customer_image = fileName;
            }

            _context.SaveChanges();
            return RedirectToAction("fetchCustomer");
        }
        public IActionResult deletePermission(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
          
        }

        public IActionResult deleteCustomer(int id)
        {
            var customer = _context.tbl_customer.Find(id);
            if (customer != null)
            {
                _context.tbl_customer.Remove(customer);
                _context.SaveChanges();
            }

            return RedirectToAction("fetchCustomer");
        }
        public IActionResult fetchCategory()
        {
            return View(_context.tbl_category.ToList());
        }
        public IActionResult addCategory()
        {
            return View();

        }
        [HttpPost]
        public IActionResult addCategory(Category cat)
        {
            _context.tbl_category.Add(cat);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");

        }
        public IActionResult updateCategory(int id)
        {
         var category=   _context.tbl_category.Find(id);
           
            return View(category);

        }
        [HttpPost]
        public IActionResult updateCategory(Category cat)
        {
            _context.tbl_category.Update(cat);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");

        }
        public IActionResult deletePermissionCategory(int id)
        {
            return View(_context.tbl_category.FirstOrDefault(c => c.category_id == id));

        }
        public IActionResult deleteCategory(int id)
        {
            var category = _context.tbl_category.Find(id);
            _context.tbl_category.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult fetchDress()
        {
            return View(_context.tbl_dress.ToList());
        }
        public ActionResult addDress()
        {
            List<Category> categories = _context.tbl_category.ToList();
            ViewData["category"] = categories;
            return View();
        }

            [HttpPost]
           public ActionResult addDress(Dress prod,IFormFile dress_image)
        {
            string imageName = Path.GetFileName(dress_image.FileName);
            string imagepath = Path.Combine(_env.WebRootPath, "dress_image", imageName);
            FileStream fs = new FileStream(imagepath, FileMode.Create);
            dress_image.CopyTo(fs);
            prod.dress_image = imageName;
            _context.tbl_dress.Add(prod);
                _context.SaveChanges();
            return RedirectToAction("fetchDress");
        }
        public IActionResult dressDetails(int id)
        {
            return View(_context.tbl_dress.Include(p=>p.Category).FirstOrDefault(p=>p.dress_id==id));
        }

        public IActionResult deletePermissionDress(int id)
        {
            return View(_context.tbl_dress.FirstOrDefault(p=> p.dress_id == id));

        }
        public IActionResult deleteDress(int id)
        {
            var dress = _context.tbl_dress.Find(id);
            _context.tbl_dress.Remove(dress);
            _context.SaveChanges();
            return RedirectToAction("fetchDress");
        }
        public IActionResult updateDress(int id)
        {
            List<Category> categories = _context.tbl_category.ToList();
            ViewData["category"] = categories;
            var dress = _context.tbl_dress.Find(id);
            ViewBag.selectedCategoryId = dress.cat_id;
            return View(dress);

        }

        [HttpPost]
        public IActionResult updateDress(Dress dress)
        {
            _context.tbl_dress.Update(dress);
            _context.SaveChanges();
            return RedirectToAction("fetchDress");

        }
        [HttpPost]
        public IActionResult ChangeDressImage(IFormFile dress_image,Dress dress)
        {
            if (dress_image != null && dress_image.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "dress_image");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string fileName = dress_image.FileName;
                string imagePath = Path.Combine(folderPath, fileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    dress_image.CopyTo(fs);
                }

                dress.dress_image = fileName;
            }

            _context.tbl_dress.Update(dress);
            _context.SaveChanges();

            return RedirectToAction("fetchDress");
        }
        public IActionResult fetchFeedback()
        {
            return View(_context.tbl_feedback.ToList());
        }

        public IActionResult deletePermissionFeedback(int id)
        {
            var feedback = _context.tbl_feedback.FirstOrDefault(f => f.feedback_id == id);
            return View(feedback);
        }

        [HttpPost]
        public IActionResult deleteFeedback(int id)
        {
            var feedback = _context.tbl_feedback.Find(id);

            if (feedback != null)
            {
                _context.tbl_feedback.Remove(feedback);
                _context.SaveChanges();
            }

            return RedirectToAction("fetchFeedback");
        }
        public IActionResult fetchCart()
        {
           var cart= _context.tbl_cart.Include(c => c.products).Include(c => 
            c.customers).ToList();
            return View(cart);
        }
        public IActionResult deletePermissionCart(int id)
        {
            return View(_context.tbl_cart.FirstOrDefault(c => c.cart_id == id));
        }

        public IActionResult deleteCart(int id)
        {
            var data = _context.tbl_cart.Find(id);
            _context.tbl_cart.Remove(data);
            _context.SaveChanges();
            return RedirectToAction("fetchCart");
        }

        public IActionResult updateCart(int id)
        {
            var cart = _context.tbl_cart.Find(id);
            return View(cart);
        }

        [HttpPost]
        public IActionResult updateCart(Cart cart)
        {
            var existingCart = _context.tbl_cart.Find(cart.cart_id);

            if (existingCart == null)
                return NotFound();

            // Only update what the user changed
            existingCart.cart_status = cart.cart_status;

            _context.SaveChanges();

            return RedirectToAction("fetchCart");
        }
        public IActionResult fetchOrders()
        {
            var orders = _context.tbl_order
                                 .OrderByDescending(o => o.order_date)
                                 .ToList();

            return View(orders);
        }

        // ================= SALES REPORT =================

        public IActionResult SalesReport(DateTime? fromDate, DateTime? toDate)
        {
            // Check admin login
            string admin_session = HttpContext.Session.GetString("admin_session");

            if (string.IsNullOrEmpty(admin_session))
            {
                return RedirectToAction("Login");
            }

            // Get only PAID orders
            var query = _context.tbl_order
                                .Where(o => o.status == "Paid")
                                .AsQueryable();

            // From date filter
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.order_date >= fromDate.Value);
            }

            // To date filter
            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.Date.AddDays(1);

                query = query.Where(o => o.order_date < endDate);
            }

            var orders = query
                        .OrderByDescending(o => o.order_date)
                        .ToList();

            // Total sales
            decimal totalSales = orders.Sum(o => o.total_amount);

            // Total paid orders
            int totalOrders = orders.Count;

            // Today's sales
            DateTime today = DateTime.Today;

            decimal todaySales = _context.tbl_order
                .Where(o =>
                    o.status == "Paid" &&
                    o.order_date >= today &&
                    o.order_date < today.AddDays(1))
                .Sum(o => o.total_amount);

            // Send data to View
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TodaySales = todaySales;

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }
        // Blog For //
        public IActionResult Blogs()
        {
            var blogs = _context.tbl_blog
                                .OrderByDescending(b => b.created_date)
                                .ToList();
            return View(blogs);
        }

        public IActionResult AddBlog()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddBlog(Blog blog)
        {
            blog.created_date = DateTime.Now;
            _context.tbl_blog.Add(blog);
            _context.SaveChanges();
            return RedirectToAction("Blogs");
        }

        public IActionResult DeleteBlog(int id)
        {
            var blog = _context.tbl_blog.Find(id);
            if (blog != null)
            {
                _context.tbl_blog.Remove(blog);
                _context.SaveChanges();
            }
            return RedirectToAction("Blogs");
        }




    }
}
