using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;



namespace Ecommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM VM = new CartVM()
            {
                ListOfCart = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product"),
                orderHeader = new Ecommerce.Models.OrderHeader()
            };


            foreach (var item in VM.ListOfCart)
            {
                VM.orderHeader.OrderTotal += (item.Product.ListPrice * item.Count);
                VM.TotalPay += (item.Product.Price * item.Count);

            }

            return View(VM);
        }
        public IActionResult Plus(int id)
        {
            var cart = _unitOfWork.Cart.Get(x => x.Id == id);
            _unitOfWork.Cart.IncrementCartItem(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int id)
        {
            var cart = _unitOfWork.Cart.Get(x => x.Id == id);
            if (cart.Count <= 1)
            {
                _unitOfWork.Cart.Remove(cart);
                var count = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32("SessionCart", count-1);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _unitOfWork.Cart.DecrementCartItem(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int id)
        {
            var cart = _unitOfWork.Cart.Get(x => x.Id == id);
            _unitOfWork.Cart.Remove(cart);
            _unitOfWork.Save();
            var count = _unitOfWork.Cart.GetAll(x=>x.ApplicationUserId==cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32("SessionCart", count);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            CartVM VM = new CartVM()
            {
                ListOfCart = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product"),
                orderHeader = new Ecommerce.Models.OrderHeader()
            };
            VM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == claims.Value);
            VM.orderHeader.Name = VM.orderHeader.ApplicationUser.Name;
            VM.orderHeader.Address = VM.orderHeader.ApplicationUser.StreetAddress;
            VM.orderHeader.City = VM.orderHeader.ApplicationUser.City;
            VM.orderHeader.State = VM.orderHeader.ApplicationUser.State;
            VM.orderHeader.PinCode = VM.orderHeader.ApplicationUser.PostalCode;
            VM.orderHeader.Phone = VM.orderHeader.ApplicationUser.PhoneNumber;

            foreach (var item in VM.ListOfCart)
            {
                VM.orderHeader.OrderTotal += (item.Product.ListPrice * item.Count);
                VM.TotalPay += (item.Product.Price * item.Count);

            }

            return View(VM);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]

        public IActionResult Summary(CartVM vm)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            vm.ListOfCart = _unitOfWork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, includeProperties: "Product");
            vm.orderHeader.OrderStatus = OrderStatus.StatusPanding;
            vm.orderHeader.PaymentStatus = PaymentStatus.StatusPending;
            vm.orderHeader.DateOfOrder = DateTime.Now;
            vm.orderHeader.ApplicationUserId = claims.Value;

            foreach (var item in vm.ListOfCart)
            {
                vm.orderHeader.OrderTotal += (item.Product.Price) * (item.Count);
            }
            _unitOfWork.OrderHeader.Add(vm.orderHeader);
            _unitOfWork.Save();
            foreach (var item in vm.ListOfCart)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = vm.orderHeader.Id,
                    Count = item.Count,
                    Price = item.Product.Price
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            var domain = "https://localhost:7017/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl =domain+ $"Customer/Cart/orderSuccess?id={vm.orderHeader.Id}",
                CancelUrl =domain +$"Customer/Cart/Index",
            };

            foreach (var item in vm.ListOfCart)
            {

                var lineItemsOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*100),
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(lineItemsOptions);
        
            }


            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.PaymentStatus(vm.orderHeader.Id,session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            _unitOfWork.Cart.RemoveRange(vm.ListOfCart);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult orderSuccess(int id)
        {
            var orderHeader= _unitOfWork.OrderHeader.Get(x=>x.Id==id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.PaymentStatus(orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id, OrderStatus.StatusApporoved, PaymentStatus.StatusApproved);

            }
            List<Cart> cart = _unitOfWork.Cart.GetAll(x=>x.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
            _unitOfWork.Cart.RemoveRange(cart);
            _unitOfWork.Save();
            return View(id);
        }
    }
}
