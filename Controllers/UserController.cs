using auth.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;
using System.Linq.Dynamic;
using System.Data.Entity;
using System.Net;

namespace auth.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        SignupLoginEntities2 db = new SignupLoginEntities2();


        //Method for display the welcome screen
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {

                return View();
            }
        }

        //for displaying the list of created item ,search result and i variable taken for displaying the page number at the bottom of the page
        public ActionResult List(string search, int? i)
        {
            if (Session["UserId"] != null)
            {
                SignupLoginEntities2 db = new SignupLoginEntities2();

                List<product> listemp = db.products.ToList();

                return View(db.products.Where(x => x.product_name.StartsWith(search) || x.category_name.StartsWith(search) || search == null).ToList().ToPagedList(i ?? 1, 5));
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }


        //httpget method for adding the product
        public ActionResult Create()
        {
            if(Session["UserId"] != null)
            {
                return View();
            }
            else
            {
               return RedirectToAction("Index", "Login");
            }
            
        }


        //httppost method for adding the product in database
        [HttpPost]
        public ActionResult Create(HttpPostedFileBase file, HttpPostedFileBase limgfile, product emp)
        {
            if (Session["UserId"] != null)
            {
                //image file can not be null
                if ((file == null && limgfile == null) || (file != null && limgfile == null) || (file == null && limgfile != null))
                {
                    TempData["sempty"] = "<script>alert('Choose the image')</script>";
                    return View();
                }

                //for small image
                string filename = Path.GetFileName(file.FileName);
                string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                string extension = Path.GetExtension(file.FileName);
                string path = Path.Combine(Server.MapPath("~/simages/"), _filename);

                //for large image
                string filename1 = Path.GetFileName(limgfile.FileName);
                string _filename1 = DateTime.Now.ToString("yymmssfff") + filename1;
                string extension1 = Path.GetExtension(limgfile.FileName);
                string path1 = Path.Combine(Server.MapPath("~/limages/"), _filename1);

                //for storing the image in database
                emp.simg = "~/simages/" + _filename;
                emp.limg = "~/limages/" + _filename1;


                //checking for the extension of the image
                if ((extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png") && (extension1.ToLower() == ".jpg" || extension1.ToLower() == ".jpeg" || extension1.ToLower() == ".png"))
                {
                    //checking the size of the image which is less than 1 Mb
                    if (file.ContentLength <= 1000000 && limgfile.ContentLength <= 1000000)
                    {
                        db.products.Add(emp);
                        try
                        {
                            //checking that the product is sucessfully added in the database or not
                            if (db.SaveChanges() > 0)
                            {
                                //save the imagepath in database
                                file.SaveAs(path);
                                limgfile.SaveAs(path1);
                                TempData["Sucess"] = "<script>alert('Sucessfully inserted')</script>";
                                ModelState.Clear();
                                return RedirectToAction("List", "User");
                            }
                            else
                            {
                                TempData["Sucess"] = "<script>alert('Sucessfully not inserted')</script>";
                            }
                        }
                        //for displaying the property if  any error is ocuured 
                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {
                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {
                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                }
                            }
                        }


                    }
                    else
                    {
                        TempData["Size"] = "<script>alert('Size is greter than 1 MB')</script>";

                    }
                }
                else
                {
                    TempData["Format"] = "<script>alert('Please choose the image with format like: jpg ,png and jpeg')</script>";
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();

        }
        


        //for deleting the selected items
        public ActionResult DelSelected(String[] empids)
        {
           
            int[] getid = null;
            if (empids != null)
            {
                getid = new int[empids.Length];
                int j = 0;
                foreach (string i in empids)
                {
                    int.TryParse(i, out getid[j++]);
                }

                List<product> getempids = new List<product>();
                SignupLoginEntities2 sd = new SignupLoginEntities2();
                getempids = sd.products.Where(x => getid.Contains(x.Id)).ToList();
                foreach (var s in getempids)
                {
                    sd.products.Remove(s);
                }
                sd.SaveChanges();
            }
            return RedirectToAction("List", "User");


        }

        //httpget method for edit to display the database data
        public ActionResult Edit(int id)
        {
            if (Session["UserId"] != null)
            {
                var emp = db.products.Where(x => x.Id == id).FirstOrDefault();
                //session for storing the path of image
                Session["Image"] = emp.simg;
                Session["Image1"] = emp.limg;
                return View(emp);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        //httppost method for edit

        [HttpPost]
        public ActionResult Edit(HttpPostedFileBase file, HttpPostedFileBase limgfile,product emp)
        {
            if (Session["UserId"] != null)
            {
                if (ModelState.IsValid == true)
                {
                    //small image is choose for edit but large image is not choose
                    if (file != null && limgfile == null)
                    {
                        string filename = Path.GetFileName(file.FileName);
                        string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                        string extension = Path.GetExtension(file.FileName);
                        string path = Path.Combine(Server.MapPath("~/simages/"), _filename);
                        emp.simg = "~/simages/" + _filename;

                        //get the large image path from session
                        emp.limg = Session["Image1"].ToString();

                        if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png")
                        {
                            if (file.ContentLength <= 1000000)
                            {
                                db.Entry(emp).State = EntityState.Modified;
                                try
                                {
                                    if (db.SaveChanges() > 0)
                                    {
                                        file.SaveAs(path);
                                        TempData["update"] = "<script>alert('Sucessfully updated')</script>";
                                        ModelState.Clear();
                                        return RedirectToAction("List", "User");
                                    }
                                    else
                                    {
                                        TempData["update"] = "<script>alert('Sucessfully not updated')</script>";
                                    }
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                    {
                                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                                        {
                                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                        }
                                    }
                                }


                            }
                            else
                            {
                                TempData["Size"] = "<script>alert('Size is greter than 1 MB')</script>";

                            }
                        }
                        else
                        {
                            TempData["Format"] = "<script>alert('Please choose the image with format like: jpg ,png and jpeg')</script>";
                        }


                    }

                    //small image is not choosen for edit but large image is choosen
                    else if (file == null && limgfile != null)
                    {
                        //get the small image path from session
                        emp.simg = Session["Image"].ToString();

                        string filename1 = Path.GetFileName(limgfile.FileName);
                        string _filename1 = DateTime.Now.ToString("yymmssfff") + filename1;
                        string extension1 = Path.GetExtension(limgfile.FileName);
                        string path1 = Path.Combine(Server.MapPath("~/limages/"), _filename1);
                        emp.limg = "~/limages/" + _filename1;

                        if (extension1.ToLower() == ".jpg" || extension1.ToLower() == ".jpeg" || extension1.ToLower() == ".png")
                        {
                            if (limgfile.ContentLength <= 1000000)
                            {
                                db.Entry(emp).State = EntityState.Modified;
                                try
                                {
                                    if (db.SaveChanges() > 0)
                                    {

                                        limgfile.SaveAs(path1);
                                        TempData["update"] = "<script>alert('Sucessfully updated')</script>";
                                        ModelState.Clear();
                                        return RedirectToAction("List", "User");
                                    }
                                    else
                                    {
                                        TempData["update"] = "<script>alert('Sucessfully not updated')</script>";
                                    }
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                    {
                                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                                        {
                                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                        }
                                    }
                                }


                            }
                            else
                            {
                                TempData["Size"] = "<script>alert('Size is greter than 1 MB')</script>";

                            }
                        }
                        else
                        {
                            TempData["Format"] = "<script>alert('Please choose the image with format like: jpg ,png and jpeg')</script>";
                        }

                    }
                    //both image is choosen for edit
                    else if (file != null && limgfile != null)
                    {

                        string filename = Path.GetFileName(file.FileName);
                        string _filename = DateTime.Now.ToString("yymmssfff") + filename;
                        string extension = Path.GetExtension(file.FileName);
                        string path = Path.Combine(Server.MapPath("~/simages/"), _filename);

                        string filename1 = Path.GetFileName(limgfile.FileName);
                        string _filename1 = DateTime.Now.ToString("yymmssfff") + filename1;
                        string extension1 = Path.GetExtension(limgfile.FileName);
                        string path1 = Path.Combine(Server.MapPath("~/limages/"), _filename1);

                        emp.simg = "~/simages/" + _filename;
                        emp.limg = "~/limages/" + _filename1;

                        if ((extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png") && (extension1.ToLower() == ".jpg" || extension1.ToLower() == ".jpeg" || extension1.ToLower() == ".png"))
                        {
                            if (file.ContentLength <= 1000000 && limgfile.ContentLength <= 1000000)
                            {
                                db.Entry(emp).State = EntityState.Modified;
                                try
                                {
                                    if (db.SaveChanges() > 0)
                                    {
                                        file.SaveAs(path);
                                        limgfile.SaveAs(path1);
                                        TempData["update"] = "<script>alert('Sucessfully updated')</script>";
                                        ModelState.Clear();
                                        return RedirectToAction("List", "User");
                                    }
                                    else
                                    {
                                        TempData["update"] = "<script>alert('Sucessfully not updated')</script>";
                                    }
                                }
                                catch (DbEntityValidationException ex)
                                {
                                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                    {
                                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                                        {
                                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                        }
                                    }
                                }


                            }
                            else
                            {
                                TempData["Size"] = "<script>alert('Size is greter than 1 MB')</script>";

                            }
                        }
                        else
                        {
                            TempData["Format"] = "<script>alert('Please choose the image with format like: jpg ,png and jpeg')</script>";
                        }


                    }
                    //none of the image is choosen for edit other field is choosen for edit
                    else
                    {
                        emp.simg = Session["Image"].ToString();
                        emp.limg = Session["Image1"].ToString();
                        db.Entry(emp).State = EntityState.Modified;
                        try
                        {
                            if (db.SaveChanges() > 0)
                            {

                                TempData["update"] = "<script>alert('Sucessfully updated')</script>";
                                ModelState.Clear();
                                return RedirectToAction("List", "User");
                            }
                            else
                            {
                                TempData["update"] = "<script>alert('Sucessfully not updated')</script>";
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            foreach (var entityValidationErrors in ex.EntityValidationErrors)
                            {
                                foreach (var validationError in entityValidationErrors.ValidationErrors)
                                {
                                    Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                }
                            }
                        }
                    }

                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}