using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using auction.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace auction.Controllers
{
    public class HomeController : Controller
    {

        private TestContext dbContext;

        public HomeController(TestContext context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateUser(User user){
            // Generic User creation post process
            // Copypaste this whole boi
            // Good for transplanting into PostController
            if(dbContext.Users.Any(u => u.UserName == user.UserName)){
                ModelState.AddModelError("UserName", "Username already in use!");
            }
            if(user.Confirm != user.Password) {
                ModelState.AddModelError("Confirm", "Password must match Confirmation!");
            }
            if(ModelState.IsValid){
                // Add to DB here.
                //using Microsoft.AspNetCore.Identity;
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                user.Cash = 1000.0;
                dbContext.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetString("LoggedIn", "true");
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToAction("Dashboard", "Home");
            }
            
            return View("Index");
        }

        public IActionResult LoginProcess(Login user){
            if(!dbContext.Users.Any(u => u.UserName == user.UserName)){
                // ModelState.AddModelError("UserName", "Wrong UserName/Password");
                ViewBag.LoginError = "Wrong Username or password.";
                return View("Index");
            }
            if(user.Password == null){
                ViewBag.LoginError = "Password is required";
                return View("Index");
            }else{
                User targetUser = dbContext.Users.FirstOrDefault(u => u.UserName == user.UserName);
                var Hasher = new PasswordHasher<Login>();
                var result = Hasher.VerifyHashedPassword(user, targetUser.Password, user.Password);
                System.Console.WriteLine(result);
                if(result == 0)
                {
                    // ModelState.AddModelError("Password", "Wrong password, you devil.");
                    ViewBag.LoginError = "Wrong password!";
                    return View("Index");
                }
                HttpContext.Session.SetString("LoggedIn", "true");
                HttpContext.Session.SetInt32("UserId", targetUser.UserId);
                return RedirectToAction("Dashboard", "Home");
            }
        }

        public IActionResult Dashboard(){
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            // Add code to change past due entries Current setting to False
            Maintenance();
            List<Auction> pre_auctions = dbContext.Auctions.Include(a => a.User).ToList();
            List<Auction> auctions = new List<Auction>();
            foreach (var item in pre_auctions)
            {
                if(item.IsCurrent == true){
                    auctions.Add(item);
                }
            }
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            List<Auction> auctions1 = new List<Auction>();
            auctions1 = auctions.OrderBy(a => a.EndDate).ToList();
            User user = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
            ViewBag.CurrentUser = user;
            return View(auctions1);
        } 

        public void Maintenance(){
            List<Auction> all_active = dbContext.Auctions.Where(a => a.IsCurrent == true).ToList();
            foreach(var item in all_active){
                if(item.EndDate < DateTime.Now){
                    User seller = dbContext.Users.FirstOrDefault(u => u.UserId == item.UserId);
                    User buyer = dbContext.Users.FirstOrDefault(u => u.UserId == item.HighBidderId);
                    buyer.Cash -= item.CurrentBid;
                    seller.Cash += item.CurrentBid;
                    item.IsCurrent = false;
                    dbContext.SaveChanges();
                }
            }
        }

        public IActionResult NewAuction(){
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult CreateAuction(Auction auction)
        {

            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            if(HttpContext.Session.GetInt32("UserId") == null){
                ModelState.AddModelError("Name","User not currently logged in!");
            }
            if(auction.CurrentBid < 1){
                ModelState.AddModelError("CurrentBid","Nope, can't start negative or 0.");
            }
            if(auction.EndDate < DateTime.Now){
                ModelState.AddModelError("EndDate","You can't put the date in the past!");
            }
            if(ModelState.IsValid) 
            {
                System.Console.WriteLine("Everything looks up to snuff boss. ##########################################################");
                auction.UserId = (int)HttpContext.Session.GetInt32("UserId");
                auction.HighBidderId = auction.UserId;
                auction.IsCurrent = true;
                dbContext.Add(auction);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("NewAuction");
        }

        [HttpGet("auction/{AuctionId}")]
        public IActionResult AuctionPage(int AuctionId){
            Auction auction = dbContext.Auctions.Include(a => a.User).FirstOrDefault(a => a.AuctionId == AuctionId);
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            ViewBag.TargetUser = dbContext.Users.FirstOrDefault(u => u.UserId == auction.HighBidderId);
            return View(auction);
        }

        [HttpGet("delete_auction/{AuctionId}")]
        public IActionResult DeleteAuction(int AuctionId)
        {
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            User current_user = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
            Auction auction = dbContext.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
            if(auction.UserId != current_user.UserId){
                return RedirectToAction("LogOut");
            }
            else{
                dbContext.Remove(auction);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
        }
        

        [HttpGet("make_bid/{AuctionId}")]
        public IActionResult ProcessBid(int AuctionId, int Bid){
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            Auction target_auction = dbContext.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
            ViewBag.TargetUser = dbContext.Users.FirstOrDefault(u => u.UserId == target_auction.HighBidderId);
            User current_user = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
            if(target_auction.UserId == current_user.UserId){
                ViewBag.Error = "You can't bid on your own things!";
                return View("AuctionPage", target_auction);
            }
            if(current_user.Cash < Bid){
                ViewBag.Error = "You can't bid more than you have!";
                return View("AuctionPage", target_auction);
            }
            if(target_auction.CurrentBid > Bid){
                ViewBag.Error = "Nope, can't bid down.  Not how this works.";
                return View("AuctionPage", target_auction);
            }
            target_auction.CurrentBid = Bid;
            target_auction.HighBidderId = current_user.UserId;
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpPost("end_auction/{AuctionId}")]
        public IActionResult EndAuction(int AuctionId){
            if(HttpContext.Session.GetInt32("UserId") == null){
                return RedirectToAction("Index");
            }
            Auction auction = dbContext.Auctions.FirstOrDefault(a => a.AuctionId == AuctionId);
            auction.EndDate = DateTime.Now;
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("logout")]
        public IActionResult LogOut(){
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }











        // premade stuff

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
