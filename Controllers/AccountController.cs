using Microsoft.AspNetCore.Mvc;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly JsonFileService<User> _userService;
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

        public AccountController()
        {
            _userService = new JsonFileService<User>(_path);
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            var users = _userService.ReadAll();

            // Check if email already exists
            if (users.Any(u => u.Email == user.Email))
            {
                ViewBag.Error = "Email already registered!";
                return View();
            }

            // Auto-generate ID
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;

            // ✅ If no users exist yet, make the first one Admin (Librarian)
            if (!users.Any())
                user.Role = "Admin";
            else
                user.Role = "User";

            users.Add(user);
            _userService.WriteAll(users);

            TempData["Message"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }


        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var users = _userService.ReadAll();
            var user = users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Invalid credentials!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);
            return RedirectToAction("Index", "Books");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
