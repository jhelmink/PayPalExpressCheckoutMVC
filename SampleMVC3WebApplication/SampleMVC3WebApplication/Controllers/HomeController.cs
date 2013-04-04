using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SampleMVC3WebApplication.Models;

namespace SampleMVC3WebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Mode = PayPalMvc.Configuration.Current.Mode;
            ViewBag.MerchantUserName = PayPalMvc.Configuration.Current.MerchantUserName;
            ViewBag.MerchantPassword = PayPalMvc.Configuration.Current.MerchantPassword;
            ViewBag.Signature = PayPalMvc.Configuration.Current.Signature;

            return View();
        }

        public ActionResult CheckOut()
        {
            TicketBasket basket = new TicketBasket
            {
                Id = Guid.NewGuid(),
                Currency = "GBP",
                PurchaseDescription = "2 x Tickets to the Gun show",
                TotalPrice = 1.50M
            };

            // Storing this in session, you might want to store in it a database
            Session["TicketBasket"] = basket;
            
            return View(basket);
        }
    }
}
