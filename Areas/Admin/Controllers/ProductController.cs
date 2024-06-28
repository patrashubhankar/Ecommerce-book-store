using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        
        public IActionResult Index() 
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
           
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u=>u.Id==id);
                return View(productVM);
            }
            
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file, IFormFile? file1, IFormFile? file2)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = 
                            Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (file1 != null)
                { 
                    string fileName1 = Guid.NewGuid().ToString() + Path.GetExtension(file1.FileName);
                    string productPath1 = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl1))
                    {
                        //delete the old image
                        var oldImagePath1 = 
                            Path.Combine(wwwRootPath, productVM.Product.ImageUrl1.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath1))
                        {
                            System.IO.File.Delete(oldImagePath1);
                        }
                    }

                    using (var fileStream1 = new FileStream(Path.Combine(productPath1, fileName1),FileMode.Create))
                    {
                        file1.CopyTo(fileStream1);
                    }

                    productVM.Product.ImageUrl1 = @"\images\product\" + fileName1;
                }
                if (file2 != null)
                {
                    string fileName2 = Guid.NewGuid().ToString() + Path.GetExtension(file2.FileName);
                    string productPath2 = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl2))
                    {
                        //delete the old image
                        var oldImagePath2 = 
                            Path.Combine(wwwRootPath, productVM.Product.ImageUrl2.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath2))
                        {
                            System.IO.File.Delete(oldImagePath2);
                        }
                    }

                    using (var fileStream2 = new FileStream(Path.Combine(productPath2, fileName2),FileMode.Create))
                    {
                        file2.CopyTo(fileStream2);
                    }

                    productVM.Product.ImageUrl2 = @"\images\product\" + fileName2;
                }

                productVM.Product.Discount =Convert.ToInt32(((productVM.Product.ListPrice - productVM.Product.Price) * 100) / productVM.Product.ListPrice);
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            if(productToBeDeleted.ImageUrl != null)
            {
                var oldImagePath =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           productToBeDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            if(productToBeDeleted.ImageUrl1 != null)
            {
                var oldImagePath1 =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           productToBeDeleted.ImageUrl1.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath1))
                {
                    System.IO.File.Delete(oldImagePath1);
                }
            }
            if(productToBeDeleted.ImageUrl2 != null)
            {
                var oldImagePath2 =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           productToBeDeleted.ImageUrl2.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath2))
                {
                    System.IO.File.Delete(oldImagePath2);
                }
            }
            
            
            

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
