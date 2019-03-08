﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCeTicaret.Models;

namespace MVCeTicaret.Controllers
{
	public class ProductController : Controller
	{
		Context db;

		public ProductController()
		{
			db = new Context();
		}

		public ActionResult Product(int id)
		{

			return View(db.Products.Where(x => x.SubCategoryID == id).ToList());
		}
		public ActionResult ProductDetail(int id)
		{
			ViewData["Reviews"] = db.Reviews.Where(x => x.ProductID == id).ToList();
			return View(db.Products.Find(id));
		}

		[HttpPost]
		public ActionResult AddReview(int id, FormCollection frm)
		{
			Review review = new Review();
			review.Comment = frm["review"];
			review.CustomerID = TemporaryUserData.UserID;
			review.DateTime = DateTime.Now;
			review.IsDeleted = false;
			review.Name = frm["name"] == "" ? "Anonymous" : frm["name"];
			review.ProductID = id;
			review.Rate = int.Parse(frm["rate"]);

			db.Reviews.Add(review);
			db.SaveChanges();
			return RedirectToAction("ProductDetail", new { id = id });
		}

	}
}