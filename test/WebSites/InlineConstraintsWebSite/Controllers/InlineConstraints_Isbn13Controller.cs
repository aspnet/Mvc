using Microsoft.AspNet.Mvc;
using System;

namespace InlineConstraintsWebSite.Controllers
{
    public class InlineConstraints_Isbn13Controller : Controller
    {
        public string Index(string isbnNumber)
        {
            return "13 Digit ISBN Number " + isbnNumber;
        }
    }
}