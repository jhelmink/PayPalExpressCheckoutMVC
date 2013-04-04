using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleMVC3WebApplication.Models
{
    public class TicketBasket
    {
        public Guid Id { get; set; }
        public string Currency { get; set; }
        public decimal TotalPrice { get; set; }
        public string PurchaseDescription { get; set; }
    }
}