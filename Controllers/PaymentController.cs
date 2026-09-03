using dress_ordering_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace dress_ordering_system.Controllers
{
    public class PaymentController : Controller
    {
        private readonly myContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentController(
            myContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }


        // =========================================================
        // PAYMENT SUCCESS
        // =========================================================

        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentSuccess()
        {
            // Get transaction ID
            string transactionId = GetRequestValue("tran_id");

            // Get validation ID
            string validationId = GetRequestValue("val_id");

            // -----------------------------------------------------
            // Check transaction information
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return Content(
                    "Transaction ID was not received from SSLCOMMERZ.");
            }

            if (string.IsNullOrWhiteSpace(validationId))
            {
                return Content(
                    "Validation ID was not received from SSLCOMMERZ.");
            }


            // =====================================================
            // Extract Order ID from transaction ID
            //
            // Example:
            // DRESS_9_20260827195649
            //
            // Order ID = 9
            // =====================================================

            string[] transactionParts =
                transactionId.Split('_');

            if (transactionParts.Length < 2)
            {
                return Content(
                    "Invalid transaction ID: " + transactionId);
            }

            if (!int.TryParse(
                transactionParts[1],
                out int orderId))
            {
                return Content(
                    "Could not extract Order ID from transaction ID: "
                    + transactionId);
            }


            // =====================================================
            // Find order
            // =====================================================

            var order = await _context.tbl_order
                .FirstOrDefaultAsync(
                    o => o.order_id == orderId);

            if (order == null)
            {
                return Content(
                    "Order not found. Order ID: " + orderId);
            }


            // =====================================================
            // Validate payment with SSLCOMMERZ
            // =====================================================

            JsonDocument? validationResult =
                await ValidateTransaction(validationId);

            if (validationResult == null)
            {
                order.status = "Failed";
                await _context.SaveChangesAsync();

                return Content(
                    "Could not validate payment with SSLCOMMERZ.");
            }


            try
            {
                // -------------------------------------------------
                // Get validation status
                // -------------------------------------------------

                string validationStatus =
                    GetJsonString(
                        validationResult,
                        "status");

                // -------------------------------------------------
                // Get transaction ID returned by SSLCOMMERZ
                // -------------------------------------------------

                string validatedTranId =
                    GetJsonString(
                        validationResult,
                        "tran_id");

                // -------------------------------------------------
                // Get currency
                // -------------------------------------------------

                string currency =
                    GetJsonString(
                        validationResult,
                        "currency");

                // -------------------------------------------------
                // Get paid amount
                // -------------------------------------------------

                decimal paidAmount =
                    GetJsonDecimal(
                        validationResult,
                        "amount");


                // =================================================
                // SECURITY CHECK 1
                // Payment status
                // =================================================

                if (validationStatus != "VALID" &&
                    validationStatus != "VALIDATED")
                {
                    order.status = "Failed";
                    await _context.SaveChangesAsync();

                    return Content(
                        "Payment validation failed. Status: "
                        + validationStatus);
                }


                // =================================================
                // SECURITY CHECK 2
                // Transaction ID
                // =================================================

                if (validatedTranId != transactionId)
                {
                    order.status = "Failed";
                    await _context.SaveChangesAsync();

                    return Content(
                        "Transaction ID verification failed.");
                }


                // =================================================
                // SECURITY CHECK 3
                // Currency
                // =================================================

                if (currency != "BDT")
                {
                    order.status = "Failed";
                    await _context.SaveChangesAsync();

                    return Content(
                        "Currency verification failed. Currency: "
                        + currency);
                }


                // =================================================
                // SECURITY CHECK 4
                // Amount
                // =================================================

                if (paidAmount != order.total_amount)
                {
                    order.status = "Failed";
                    await _context.SaveChangesAsync();

                    return Content(
                        "Payment amount verification failed. " +
                        "Expected: " +
                        order.total_amount +
                        " Paid: " +
                        paidAmount);
                }


                // =================================================
                // PAYMENT SUCCESSFUL
                // REDUCE STOCK + REMOVE CART
                // =================================================

                bool completed =
                    await CompleteSuccessfulOrder(order);

                if (!completed)
                {
                    order.status = "Failed";

                    await _context.SaveChangesAsync();

                    return Content(
                        "Payment was successful, but the stock could not be updated. " +
                        "Please contact the store.");
                }


                // =================================================
                // Clear payment session
                // =================================================

                HttpContext.Session.Remove(
                    "SSLTransactionId");

                HttpContext.Session.Remove(
                    "CurrentOrderId");


                // =================================================
                // Go to customer success page
                // =================================================

                return RedirectToAction(
                    "OrderSuccess",
                    "Customer");
            }
            catch (Exception ex)
            {
                order.status = "Failed";
                await _context.SaveChangesAsync();

                return Content(
                    "Payment processing error: "
                    + ex.Message);
            }
        }


        // =========================================================
        // PAYMENT FAILED
        // =========================================================

        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentFail()
        {
            string orderIdText =
                HttpContext.Session.GetString(
                    "CurrentOrderId");

            if (!string.IsNullOrWhiteSpace(orderIdText))
            {
                if (int.TryParse(
                    orderIdText,
                    out int orderId))
                {
                    var order =
                        await _context.tbl_order
                            .FirstOrDefaultAsync(
                                o => o.order_id == orderId);

                    if (order != null)
                    {
                        order.status = "Failed";

                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Content(
                "Payment failed. Please try again.");
        }


        // =========================================================
        // PAYMENT CANCELLED
        // =========================================================

        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentCancel()
        {
            string orderIdText =
                HttpContext.Session.GetString(
                    "CurrentOrderId");

            if (!string.IsNullOrWhiteSpace(orderIdText))
            {
                if (int.TryParse(
                    orderIdText,
                    out int orderId))
                {
                    var order =
                        await _context.tbl_order
                            .FirstOrDefaultAsync(
                                o => o.order_id == orderId);

                    if (order != null)
                    {
                        order.status = "Cancelled";

                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Content(
                "Payment was cancelled.");
        }


        // =========================================================
        // IPN
        // =========================================================

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> IPN()
        {
            string transactionId =
                GetRequestValue("tran_id");

            string validationId =
                GetRequestValue("val_id");

            string status =
                GetRequestValue("status");


            // -----------------------------------------------------
            // Transaction ID required
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return BadRequest(
                    "Transaction ID missing.");
            }


            // =====================================================
            // Extract Order ID
            //
            // Format:
            // DRESS_ORDERID_DATETIME
            //
            // Example:
            // DRESS_9_20260827195649
            // =====================================================

            string[] transactionParts =
                transactionId.Split('_');

            if (transactionParts.Length < 2)
            {
                return BadRequest(
                    "Invalid transaction ID.");
            }


            if (!int.TryParse(
                transactionParts[1],
                out int orderId))
            {
                return BadRequest(
                    "Invalid order ID.");
            }


            // =====================================================
            // Find order
            // =====================================================

            var order =
                await _context.tbl_order
                    .FirstOrDefaultAsync(
                        o => o.order_id == orderId);

            if (order == null)
            {
                return BadRequest(
                    "Order not found.");
            }


            // =====================================================
            // Don't process an already-paid order
            // =====================================================

            if (order.status == "Paid")
            {
                return Ok(
                    "Already processed.");
            }


            // =====================================================
            // Valid payment
            // =====================================================

            if (status == "VALID" ||
                status == "VALIDATED")
            {
                if (string.IsNullOrWhiteSpace(validationId))
                {
                    return BadRequest(
                        "Validation ID missing.");
                }


                JsonDocument? validationResult =
                    await ValidateTransaction(
                        validationId);


                if (validationResult != null)
                {
                    try
                    {
                        string validationStatus =
                            GetJsonString(
                                validationResult,
                                "status");

                        string validatedTranId =
                            GetJsonString(
                                validationResult,
                                "tran_id");

                        string currency =
                            GetJsonString(
                                validationResult,
                                "currency");

                        decimal paidAmount =
                            GetJsonDecimal(
                                validationResult,
                                "amount");


                        // -------------------------------------------------
                        // Verify everything
                        // -------------------------------------------------

                        if ((validationStatus == "VALID" ||
                             validationStatus == "VALIDATED") &&

                            validatedTranId == transactionId &&

                            currency == "BDT" &&

                            paidAmount == order.total_amount)
                        {
                            bool completed =
                                await CompleteSuccessfulOrder(order);

                            if (!completed)
                            {
                                order.status = "Failed";

                                await _context.SaveChangesAsync();

                                return BadRequest(
                                    "Payment was valid, but the stock could not be updated.");
                            }
                        }
                    }
                    catch
                    {
                        order.status = "Failed";

                        await _context.SaveChangesAsync();
                    }
                }
            }


            // =====================================================
            // Failed payment
            // =====================================================

            else if (status == "FAILED")
            {
                order.status = "Failed";

                await _context.SaveChangesAsync();
            }


            // =====================================================
            // Cancelled payment
            // =====================================================

            else if (status == "CANCELLED")
            {
                order.status = "Cancelled";

                await _context.SaveChangesAsync();
            }


            return Ok(
                "IPN received.");
        }



        // =========================================================
        // COMPLETE SUCCESSFUL ORDER
        // REDUCE STOCK + REMOVE CART
        // =========================================================

        private async Task<bool> CompleteSuccessfulOrder(Order order)
        {
            // If the order is already marked as Paid,
            // do not reduce the stock a second time.
            if (order.status == "Paid")
            {
                return true;
            }

            // Get all cart items belonging to this order's customer.
            var cartItems =
                await _context.tbl_cart
                    .Where(c => c.cust_id == order.cust_id)
                    .Include(c => c.products)
                    .ToListAsync();

            // There must be cart items to complete the order.
            if (cartItems.Count == 0)
            {
                return false;
            }

            // -----------------------------------------------------
            // CHECK STOCK ONE MORE TIME
            // -----------------------------------------------------

            foreach (var item in cartItems)
            {
                if (item.products == null)
                {
                    return false;
                }

                if (item.dress_quantity <= 0)
                {
                    return false;
                }

                if (item.dress_quantity >
                    item.products.dress_quantity)
                {
                    return false;
                }
            }

            // -----------------------------------------------------
            // REDUCE PRODUCT STOCK
            // Example: 10 - 2 = 8
            // -----------------------------------------------------

            foreach (var item in cartItems)
            {
                item.products.dress_quantity -= item.dress_quantity;
            }

            // -----------------------------------------------------
            // MARK ORDER AS PAID
            // -----------------------------------------------------

            order.status = "Paid";

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // REMOVE CART AFTER STOCK HAS BEEN UPDATED
            // -----------------------------------------------------

            _context.tbl_cart.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return true;
        }


        // =========================================================
        // SSLCOMMERZ VALIDATION
        // =========================================================

        private async Task<JsonDocument?> ValidateTransaction(
            string validationId)
        {
            string storeId =
                _configuration[
                    "SSLCommerz:StoreId"] ?? "";

            string storePassword =
                _configuration[
                    "SSLCommerz:StorePassword"] ?? "";

            string validationApi =
                _configuration[
                    "SSLCommerz:ValidationApi"] ?? "";


            if (string.IsNullOrWhiteSpace(storeId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(storePassword))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(validationApi))
            {
                return null;
            }


            // =====================================================
            // Build validation URL
            // =====================================================

            string url =
                validationApi +
                "?val_id=" +
                Uri.EscapeDataString(validationId) +
                "&store_id=" +
                Uri.EscapeDataString(storeId) +
                "&store_passwd=" +
                Uri.EscapeDataString(storePassword) +
                "&format=json";


            // =====================================================
            // Send request
            // =====================================================

            var client =
                _httpClientFactory.CreateClient();


            try
            {
                var response =
                    await client.GetAsync(url);


                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }


                string responseString =
                    await response.Content
                        .ReadAsStringAsync();


                if (string.IsNullOrWhiteSpace(
                    responseString))
                {
                    return null;
                }


                return JsonDocument.Parse(
                    responseString);
            }
            catch
            {
                return null;
            }
        }


        // =========================================================
        // GET VALUE FROM SSLCOMMERZ REQUEST
        // =========================================================

        private string GetRequestValue(
            string key)
        {
            // POST form data
            if (Request.HasFormContentType)
            {
                string formValue =
                    Request.Form[key].ToString();

                if (!string.IsNullOrWhiteSpace(
                    formValue))
                {
                    return formValue;
                }
            }


            // GET query string
            string queryValue =
                Request.Query[key].ToString();

            return queryValue ?? "";
        }


        // =========================================================
        // GET STRING FROM JSON
        // =========================================================

        private string GetJsonString(
            JsonDocument document,
            string propertyName)
        {
            if (!document.RootElement
                .TryGetProperty(
                    propertyName,
                    out JsonElement property))
            {
                return "";
            }


            if (property.ValueKind ==
                JsonValueKind.String)
            {
                return property
                    .GetString() ?? "";
            }


            return property
                .ToString();
        }


        // =========================================================
        // GET DECIMAL FROM JSON
        // =========================================================

        private decimal GetJsonDecimal(
            JsonDocument document,
            string propertyName)
        {
            if (!document.RootElement
                .TryGetProperty(
                    propertyName,
                    out JsonElement property))
            {
                return 0;
            }


            // JSON number
            if (property.ValueKind ==
                JsonValueKind.Number)
            {
                if (property.TryGetDecimal(
                    out decimal number))
                {
                    return number;
                }
            }


            // JSON string
            string text =
                property.ToString();


            if (decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal result))
            {
                return result;
            }


            return 0;
        }
    }
}