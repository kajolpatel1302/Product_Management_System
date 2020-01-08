using auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace auth.Controllers
{
    public class LoginController : Controller
    {
        SignupLoginEntities2 db = new SignupLoginEntities2();
        // GET: Login

        //httpget method for display the login page
        public ActionResult Index()
        {
            return View();
        }


        //httppost method for display the login page
        [HttpPost]

        public ActionResult Index(user u)
        {
            //match with database value
            var user = db.users.Where(model => model.email == u.email && model.password == u.password && model.username==u.username).FirstOrDefault(); 
            if(user!=null)
            {
                //store the value in session
                Session["UserId"] = u.Id.ToString();
                Session["Username"] = u.username.ToString();
                TempData["LoginSucessMessage"] = "<script>alert('Login done sucessfully!!')</script>";
                return RedirectToAction("Index","User");
            }
            else
            {
                ViewBag.ErrorMessage = "<script>alert('Email Id or password or username is incorrect')</script>";
                return View();
            }
        }

        //logout method for come back to login page
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }


        //httpget method for signup page
        public ActionResult Signup()
        {
            return View();
        }


        //httppost method for signup page
        [HttpPost]
        public ActionResult Signup(user u)
        {
            if(ModelState.IsValid==true)
            {
                //save the value in database
                db.users.Add(u);
                int a = db.SaveChanges();

                
                //for checking that data in sucessfully inserted in database or not
                if(a>0)
                {
                    ViewBag.InsertMessage = "<script>alert('Registration done sucessfully!!')</script>";
                    ModelState.Clear();
                    


                }
                else
                {
                    ViewBag.InsertMessage="<script>alert('Registration is Not done!!')</script>";
                    
                }
            }
            return View();
        }


        
    }
}