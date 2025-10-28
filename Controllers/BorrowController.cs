using Microsoft.AspNetCore.Mvc;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
    public class BorrowController : Controller
    {
        private readonly JsonFileService<Book> _bookService;
        private readonly JsonFileService<BorrowRecord> _borrowService;
        private readonly JsonFileService<User> _userService;
        private readonly string _booksPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "books.json");
        private readonly string _borrowPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "borrow.json");
        private readonly string _usersPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

        public BorrowController()
        {
            _bookService = new JsonFileService<Book>(_booksPath);
            _borrowService = new JsonFileService<BorrowRecord>(_borrowPath);
            _userService = new JsonFileService<User>(_usersPath);
        }

        public IActionResult Index()
        {
            var records = _borrowService.ReadAll();
            var books = _bookService.ReadAll();
            var users = _userService.ReadAll();

            var result = from r in records
                         join b in books on r.BookId equals b.Id
                         join u in users on r.UserId equals u.Id
                         select new
                         {
                             r.Id,
                             BookTitle = b.Title,
                             Borrower = u.Name,
                             r.BorrowDate,
                             r.ReturnDate
                         };

            return View(result.ToList());
        }

        [HttpPost]
        public IActionResult BorrowBook(int bookId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var books = _bookService.ReadAll();
            var book = books.FirstOrDefault(b => b.Id == bookId);
            if (book == null || book.Quantity <= 0) return RedirectToAction("Index", "Books");

            book.Quantity--;
            _bookService.WriteAll(books);

            var borrows = _borrowService.ReadAll();
            var record = new BorrowRecord
            {
                Id = borrows.Any() ? borrows.Max(r => r.Id) + 1 : 1,
                BookId = bookId,
                UserId = userId.Value,
                BorrowDate = DateTime.Now
            };
            borrows.Add(record);
            _borrowService.WriteAll(borrows);

            return RedirectToAction("MyBooks");
        }

        public IActionResult MyBooks()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var records = _borrowService.ReadAll().Where(r => r.UserId == userId);
            var books = _bookService.ReadAll();

            var result = from r in records
                         join b in books on r.BookId equals b.Id
                         select new
                         {
                             r.Id,
                             b.Title,
                             b.Author,
                             r.BorrowDate,
                             r.ReturnDate
                         };

            return View(result.ToList());
        }

        [HttpPost]
        public IActionResult ReturnBook(int recordId)
        {
            var borrows = _borrowService.ReadAll();
            var record = borrows.FirstOrDefault(r => r.Id == recordId);
            if (record == null || record.ReturnDate != null) return RedirectToAction("MyBooks");

            record.ReturnDate = DateTime.Now;
            _borrowService.WriteAll(borrows);

            var books = _bookService.ReadAll();
            var book = books.FirstOrDefault(b => b.Id == record.BookId);
            if (book != null)
            {
                book.Quantity++;
                _bookService.WriteAll(books);
            }

            return RedirectToAction("MyBooks");
        }
    }
}
