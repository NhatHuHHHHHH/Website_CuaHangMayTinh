using CuaHangMayTinh2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuaHangMayTinh2.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities(); 

        public ActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message;
            return View();
        }
    }
}