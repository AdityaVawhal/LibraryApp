using Microsoft.AspNetCore.Mvc;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly JsonFileService<Book> _bookService;
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "books.json");

        public BooksController()
        {
            _bookService = new JsonFileService<Book>(_path);
        }

        public IActionResult Index(string search)
        {
            var books = _bookService.ReadAll();
            if (!string.IsNullOrEmpty(search))
                books = books.Where(b => b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                                      || b.Author.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            return View(books);
        }
        // GET: /Books/Add
        [HttpGet]
        public IActionResult Add()
        {
            // Check if the logged-in user is Admin
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                TempData["Error"] = "Only the librarian can add new books.";
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: /Books/Add
        [HttpPost]
        public IActionResult Add(Book book)
        {
            // Check if the logged-in user is Admin
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                TempData["Error"] = "You don’t have permission to add books.";
                return RedirectToAction("Index");
            }

            var books = _bookService.ReadAll();
            book.Id = books.Any() ? books.Max(b => b.Id) + 1 : 1;
            books.Add(book);
            _bookService.WriteAll(books);

            TempData["Success"] = "Book added successfully.";
            return RedirectToAction("Index");
        }


        public IActionResult Details(int id)
        {
            var book = _bookService.ReadAll().FirstOrDefault(b => b.Id == id);
            return View(book);
        }
    }
}
