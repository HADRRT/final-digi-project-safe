using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpensesSystem.Data;
using ExpensesSystem.Models;

namespace ExpensesSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Supervisor")]
        public IActionResult GetExpenses()
        {
            var expenses = _context.Expenses.ToList();
            return Ok(expenses);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Child")]
        public IActionResult GetMyExpenses()
        {
            var name = User.Identity?.Name;
            var expenses = _context.Expenses
                .Where(e => e.ChildName == name)
                .ToList();
            return Ok(expenses);
        }

        [HttpPost]
        [Authorize(Roles = "Child")]
        public IActionResult AddExpense(Expense expense)
        {
            expense.Status = "Pending";
            _context.Expenses.Add(expense);
            _context.SaveChanges();
            return Ok(expense);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Supervisor")]
        public IActionResult ApproveExpense(int id)
        {
            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id);
            if (expense == null) return NotFound();
            expense.Status = "Approved";
            _context.SaveChanges();
            return Ok(expense);
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Supervisor")]
        public IActionResult RejectExpense(int id)
        {
            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id);
            if (expense == null) return NotFound();
            expense.Status = "Rejected";
            _context.SaveChanges();
            return Ok(expense);
        }
    }
}