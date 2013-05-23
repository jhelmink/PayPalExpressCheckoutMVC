using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SampleMVC3WebApplication;
using SampleMVC3WebApplication.Models;
using PayPalMvc;
using SampleMVC3WebApplication.Services;
using PayPalMvc.Enums;

namespace SampleMVC3WebApplication.Controllers
{
    public class PurchaseController : Controller
    {
        private static TransactionService transactionService = new TransactionService();

        #region Set Express Checkout and Get Checkout Details

        public ActionResult PayPalExpressCheckout()
        {
            WebUILogging.LogMessage("Express Checkout Initiated");
            // SetExpressCheckout
            ApplicationCart cart = (ApplicationCart)Session["Cart"];
            string serverURL = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/");
            SetExpressCheckoutResponse transactionResponse = transactionService.SendPayPalSetExpressCheckoutRequest(cart, serverURL);
            // If Success redirect to PayPal for user to make payment
            if (transactionResponse == null || transactionResponse.ResponseStatus != PayPalMvc.Enums.ResponseType.Success)
            {
                SetUserNotification("Sorry there was a problem with initiating a PayPal transaction. Please try again and contact an Administrator if this still doesn't work.");
                string errorMessage = (transactionResponse == null) ? "Null Transaction Response" : transactionResponse.ErrorToString;
                WebUILogging.LogMessage("Error initiating PayPal SetExpressCheckout transaction. Error: " + errorMessage);
                return RedirectToAction("Error", "Purchase");
            }
            return Redirect(string.Format(PayPalMvc.Configuration.Current.PayPalRedirectUrl, transactionResponse.TOKEN));
        }

        public ActionResult PayPalExpressCheckoutAuthorisedSuccess(string token, string PayerID) // Note "PayerID" is returned with capitalisation as written
        {
            // PayPal redirects back to here
            WebUILogging.LogMessage("Express Checkout Authorised");
            // GetExpressCheckoutDetails
            TempData["token"] = token;
            TempData["payerId"] = PayerID;
            GetExpressCheckoutDetailsResponse transactionResponse = transactionService.SendPayPalGetExpressCheckoutDetailsRequest(token);
            if (transactionResponse == null || transactionResponse.ResponseStatus != PayPalMvc.Enums.ResponseType.Success)
            {
                SetUserNotification("Sorry there was a problem with initiating a PayPal transaction. Please try again and contact an Administrator if this still doesn't work.");
                string errorMessage = (transactionResponse == null) ? "Null Transaction Response" : transactionResponse.ErrorToString;
                WebUILogging.LogMessage("Error initiating PayPal GetExpressCheckoutDetails transaction. Error: " + errorMessage);
                return RedirectToAction("Error", "Purchase");
            }
            return RedirectToAction("ConfirmPayPalPayment");
        }

        #endregion

        #region Confirm Payment

        public ActionResult ConfirmPayPalPayment()
        {
            WebUILogging.LogMessage("Express Checkout Confirmation");
            ApplicationCart cart = (ApplicationCart)Session["Cart"];
            return View(cart);
        }

        [HttpPost]
        public ActionResult ConfirmPayPalPayment(bool confirmed = true)
        {
            WebUILogging.LogMessage("Express Checkout Confirmed");
            ApplicationCart cart = (ApplicationCart)Session["Cart"];
            // DoExpressCheckoutPayment
            string token = TempData["token"].ToString();
            string payerId = TempData["payerId"].ToString();
            DoExpressCheckoutPaymentResponse transactionResponse = transactionService.SendPayPalDoExpressCheckoutPaymentRequest(cart, token, payerId);

            if (transactionResponse == null || transactionResponse.ResponseStatus != PayPalMvc.Enums.ResponseType.Success)
            {
                if (transactionResponse != null && transactionResponse.L_ERRORCODE0 == "10486")
                {
                    // Redirect user back to PayPal in case of Error 10486 (bad funding method)
                    // https://www.x.com/developers/paypal/documentation-tools/how-to-guides/how-to-recover-funding-failure-error-code-10486-doexpresscheckout
                    WebUILogging.LogMessage("Redirecting User back to PayPal due to 10486 error (bad funding method - typically an invalid or maxed out credit card)");
                    return Redirect(string.Format(PayPalMvc.Configuration.Current.PayPalRedirectUrl, token));
                }
                SetUserNotification("Sorry there was a problem with taking the PayPal payment, so no money has been transferred. Please try again and contact an Administrator if this still doesn't work.");
                string errorMessage = (transactionResponse == null) ? "Null Transaction Response" : transactionResponse.ErrorToString;
                WebUILogging.LogMessage("Error initiating PayPal DoExpressCheckoutPayment transaction. Error: " + errorMessage);
                return RedirectToAction("Error", "Purchase");
            }

            if (transactionResponse.PaymentStatus == PaymentStatus.Completed)
                return RedirectToAction("PostPaymentSuccess");
            else
            {
                // Something went wrong or the payment isn't complete
                WebUILogging.LogMessage("Error taking PayPal payment. Error: " + transactionResponse.ErrorToString + " - Payment Error: " + transactionResponse.PaymentErrorToString);
                TempData["TransactionResult"] = transactionResponse.PAYMENTREQUEST_0_LONGMESSAGE;
                return RedirectToAction("PostPaymentFailure");
            }
        }

        #endregion

        #region Post Payment and Cancellation

        public ActionResult PostPaymentSuccess()
        {
            WebUILogging.LogMessage("Post Payment Result: Success");
            ApplicationCart cart = (ApplicationCart)Session["Cart"];
            ViewBag.TrackingReference = cart.Id;
            ViewBag.Description = cart.PurchaseDescription;
            ViewBag.TotalCost = cart.TotalPrice;
            ViewBag.Currency = cart.Currency;
            return View();
        }

        public ActionResult PostPaymentFailure()
        {
            WebUILogging.LogMessage("Post Payment Result: Failure");
            ViewBag.ErrorMessage = TempData["TransactionResult"];
            return View();
        }

        public ActionResult CancelPayPalTransaction()
        {
            return View();
        }

        #endregion

        #region Transaction Error

        private void SetUserNotification(string notification)
        {
            TempData["ErrorMessage"] = notification;
        }

        public ActionResult Error()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        #endregion

    }
}
