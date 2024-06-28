using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
//using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;

namespace Ecommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        
        public IActionResult Index(string SearchBy, string search, int id)
        {
            TempData["flag"] = true;
            TempData["search"] = search;
            if (SearchBy == "Name")
            {
                if (search != null)
                {
                    var FindData = _unitOfWork.Product.GetAll(x => x.Name.Contains(search)).ToList();
                    return View(FindData );
                }

            }
            else if(SearchBy == "Title")
            {
                if (search != null)
                {
                    var FindData = _unitOfWork.Product.GetAll(x => x.ISBN.Contains(search)).ToList();
                    return View(FindData);
                }

            }
            if(id != 0)
            {
                var FindData1 = _unitOfWork.Product.GetAll(x => x.CategoryId==id).ToList();
                return View(FindData1);
            }
            
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            TempData["flag"] = false;
            return View(productList);
        }

        [HttpGet]
        
        public IActionResult Details(int productId)
        {
            Cart cart = new Cart()
            {

                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(Cart cart)
        {
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claims.Value;
            
                var cartItem=_unitOfWork.Cart.Get(x=>x.ProductId== cart.ProductId && x.ApplicationUserId==claims.Value);

                if (cartItem == null)
                {
                    _unitOfWork.Cart.Add(cart);
                    _unitOfWork.Save();
                    HttpContext.Session.SetInt32("SessionCart",_unitOfWork.Cart.GetAll(x=>x.ApplicationUserId==claims.Value).ToList().Count);
                }
                else
                {
                    _unitOfWork.Cart.IncrementCartItem(cartItem, cart.Count);
                    _unitOfWork.Save();
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult SearchResult(string SearchBy, string search)
        {
            return View(); 
        }
        //public IActionResult GetCatagory()
        //{
        //    ProductVM productVM = new()
        //    {
        //        CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
        //        {
        //            Text = u.Name,
        //            Value = u.Id.ToString()
        //        }),
        //        Product = new Models.Product()
        //    };
        //    return View(productVM);
        //}
    }
}