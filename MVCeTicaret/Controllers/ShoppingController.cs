using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCeTicaret.Models;

namespace MVCeTicaret.Controllers
{
	public class ShoppingController : Controller
	{
		Context db;

		public ShoppingController()
		{
			db = new Context();
		}

		[HttpPost]
		public ActionResult AddToCart(int id, FormCollection frm)
		{
			if (Session["OnlineKullanici"] == null)
				return RedirectToAction("Login", "Login");

			int miktar = 1;
			if (frm["miktar"] != null)
				miktar = Convert.ToInt32(frm["miktar"]);

			ControlCart(id, miktar);

			return RedirectToAction("ProductDetail", "Product", new { id = id });
		}

		private void ControlCart(int id, int miktar = 1)
		{
			OrderDetail od = db.OrderDetails.FirstOrDefault(x => x.ProductID == id && x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false);

			if (od == null) // yeni kayıt
			{
				od = new OrderDetail();
				od.ProductID = id;
				od.CustomerID = TemporaryUserData.UserID;
				od.IsCompleted = false;
				od.UnitPrice = db.Products.FirstOrDefault(x => x.ProductID == id).UnitPrice;
				od.Discount = db.Products.Find(id).Discount;
				od.TotalAmount = od.Quantity * od.UnitPrice * (1 - od.Discount);
				od.OrderDate = DateTime.Now;

				if (db.Products.Find(id).UnitsInStock >= miktar)
					od.Quantity = miktar;
				else
					od.Quantity = db.Products.Find(id).UnitsInStock;

				od.TotalAmount = od.Quantity * od.UnitPrice * (1 - od.Discount);
				db.OrderDetails.Add(od);
			}
			else //update işlemi
			{
				if (db.Products.Find(id).UnitsInStock > od.Quantity + miktar)
				{
					od.Quantity += miktar;
					od.TotalAmount = od.Quantity * od.UnitPrice * (1 - od.Discount);
				}
			}
			db.SaveChanges();
		}

		public void ControlWishList(int id)
		{
			Wishlist wl = db.Wishlists.FirstOrDefault(x => x.ProductID == id && x.CustomerID == TemporaryUserData.UserID && x.IsActive == true);

			if (wl == null)
			{
				wl = new Wishlist();
				wl.ProductID = id;
				wl.CustomerID = TemporaryUserData.UserID;
				wl.IsActive = true;

				db.Wishlists.Add(wl);
				db.SaveChanges();
			}
		}

		public ActionResult AddToWishList(int id)
		{
			if (Session["OnlineKullanici"] == null)
				return RedirectToAction("Login", "Login");

			ControlWishList(id);

			return RedirectToAction("ProductDetail", "Product", new { id = id });
		}

		public ActionResult Cart()
		{
			return View(db.OrderDetails.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false).ToList());
		}

		public ActionResult Wish()
		{
			return View(db.Wishlists.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsActive == true).ToList());
		}

		public ActionResult RemoveFromCart(int id)
		{
			RemoveFromCartMethod(id);

			return Redirect(Request.UrlReferrer.ToString());
		}

		private void RemoveFromCartMethod(int id)
		{
			db.OrderDetails.Remove(db.OrderDetails.Find(id));
			db.SaveChanges();
		}

		[HttpPost]
		public ActionResult UpdateQuantity(int id, FormCollection frm)
		{
			OrderDetail od = db.OrderDetails.Find(id);
			od.Quantity = Convert.ToInt32(frm["quantity"]);
			od.TotalAmount = od.Quantity * od.UnitPrice * (1 - od.Discount);
			db.SaveChanges();

			return Redirect(Request.UrlReferrer.ToString());
		}

		public ActionResult AddToWishListFromCart(int id)
		{
			int productID = db.OrderDetails.Find(id).ProductID;
			ControlWishList(productID);

			db.OrderDetails.Remove(db.OrderDetails.Find(id));
			db.SaveChanges();

			return Redirect(Request.UrlReferrer.ToString());
		}

		public ActionResult RemoveFromWishList(int id)
		{
			Wishlist wishlist = db.Wishlists.Find(id);
			wishlist.IsActive = false;

			db.SaveChanges();

			return RedirectToAction("Wish", "Shopping");
		}

		public ActionResult AddToCartFromWishList(int id)
		{
			int productID = db.Wishlists.Find(id).ProductID;
			ControlCart(productID);

			Wishlist wishlist = db.Wishlists.Find(id);
			wishlist.IsActive = false;
			db.SaveChanges();

			return RedirectToAction("Wish", "Shopping");

		}

		public ActionResult GoToPayment()
		{
			List<OrderDetail> cart = db.OrderDetails.Where(x => x.IsCompleted == false && x.CustomerID == TemporaryUserData.UserID).ToList();

			foreach (var item in cart)
			{
				if (item.Product.UnitsInStock < item.Quantity)
					return RedirectToAction("UpdateCart", "Shopping");
			}

			ViewBag.OrderDetails = cart;
			ViewBag.PaymentTypes = db.PaymentTypes.ToList();

			return View(db.Customers.Find(TemporaryUserData.UserID));
		}

		public ActionResult UpdateCart()
		{
			return View(db.OrderDetails.Where(x => x.IsCompleted == false && x.CustomerID == TemporaryUserData.UserID).ToList());
		}

		[HttpPost]
		public ActionResult CompleteShopping(FormCollection frm)
		{
			Payment payment = new Payment();
			payment.Type = int.Parse(frm["paymentType"]);
			payment.CreditAmount = 20000;
			payment.DebitAmount = 20000;
			payment.Balance = 20000;
			payment.PaymentDateTime = DateTime.Now;

			db.Payments.Add(payment);

			if (frm["update"] == "on")
			{
				Customer customer = db.Customers.Find(TemporaryUserData.UserID);
				customer.FirstName = frm["FirstName"];
				customer.LastName = frm["LastName"];
				customer.Address = frm["Address"];
				customer.City = frm["City"];
			}

			ShippingDetail sp = new ShippingDetail();
			sp.FirstName = frm["FirstName"];
			sp.LastName = frm["LastName"];
			sp.Address = frm["Address"];
			sp.City = frm["City"];

			db.ShippingDetails.Add(sp);
			db.SaveChanges();

			List<OrderDetail> cart = db.OrderDetails.Where(x => x.CustomerID == TemporaryUserData.UserID && x.IsCompleted == false).ToList();
			foreach (var item in cart)
			{
				item.IsCompleted = true;
				item.Product.UnitsInStock -= item.Quantity;

				Order order = new Order()
				{
					PaymentID = payment.PaymentID,
					ShippingID = sp.ShippingID,
					OrderDetailID = item.OrderDetailID,
					Discount = item.Discount,
					TotalAmount = item.TotalAmount,
					IsCompleted = true,
					OrderDate = DateTime.Now,
					Dispatched = false,
					DispatchDate = DateTime.Now.AddDays(3),
					Shipped = false,
					ShippedDate = DateTime.Now.AddDays(5),
					Deliver = false,
					DeliveryDate = DateTime.Now.AddDays(5),
					CancelOrder = false
				};
				db.Orders.Add(order);
			}

			db.SaveChanges();
			return RedirectToAction("FinishShopping", "Shopping");
		}

		public ActionResult FinishShopping()
		{
			return View();
		}
	}
}
