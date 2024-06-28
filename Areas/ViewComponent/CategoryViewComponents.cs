using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.Areas.ViewComponents
{
    public class CategoryViewComponent:ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Category> items = new List<Category>(_unitOfWork.Category.GetAll());
            return View(items);
        }
    }
}
