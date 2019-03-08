using MVCeTicaret.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCeTicaret.Controllers
{
	public class HomeController : Controller
	{
		Context db;

		public ActionResult Index()
		{
			db = new Context();
			ViewData["Kadin"] = db.Products.Where(x => x.SubCategoryID == 1).OrderBy(x => Guid.NewGuid()).Take(4).ToList();
			ViewData["Laptop"] = db.Products.Where(x => x.SubCategoryID == 2).OrderBy(x => Guid.NewGuid()).Take(4).ToList();
			ViewData["Spor"] = db.Products.Where(x => x.SubCategoryID == 3).OrderBy(x => Guid.NewGuid()).Take(4).ToList();

			return View();
		}
	}
}