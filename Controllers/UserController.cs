using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace UserManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // Thread-safe store
        private static readonly ConcurrentDictionary<int, User> users = new();
        private static int currentId = 2;

        static UsersController()
        {
            users.TryAdd(1, new User { Id = 1, Name = "Ali Veli", Email = "ali@example.com", Age = 25 });
            users.TryAdd(2, new User { Id = 2, Name = "Ayşe Fatma", Email = "ayse@example.com", Age = 30 });
        }

        // GET: api/users?skip=0&take=50&search=ali
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers([FromQuery] int skip = 0, [FromQuery] int take = 50, [FromQuery] string? search = null)
        {
            if (take <= 0 || take > 200) take = 50;
            if (skip < 0) skip = 0;

            IEnumerable<User> result = users.Values;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                result = result.Where(u =>
                    (u.Name ?? string.Empty).ToLowerInvariant().Contains(s) ||
                    (u.Email ?? string.Empty).ToLowerInvariant().Contains(s));
            }

            var page = result
                .OrderBy(u => u.Id)
                .Skip(skip)
                .Take(take)
                .ToList();

            return Ok(new
            {
                total = result.Count(),
                count = page.Count,
                items = page
            });
        }

        // GET: api/users/{id}
        [HttpGet("{id:int}")]
        public ActionResult<User> GetUser(int id)
        {
            if (id <= 0) return BadRequest(new { title = "Invalid ID.", status = 400 });

            if (!users.TryGetValue(id, out var user))
                return NotFound(new { title = "User not found.", status = 404, id });

            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public ActionResult<User> CreateUser([FromBody] User newUser)
        {
         
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Prevent duplicate emails
            var emailExists = users.Values.Any(u => string.Equals(u.Email, newUser.Email, StringComparison.OrdinalIgnoreCase));
            if (emailExists)
                return Conflict(new { title = "Email already in use.", status = 409, email = newUser.Email });

            var id = Interlocked.Increment(ref currentId);
            var user = new User
            {
                Id = id,
                Name = newUser.Name.Trim(),
                Email = newUser.Email.Trim(),
                Age = newUser.Age
            };

            users[id] = user;

            return CreatedAtAction(nameof(GetUser), new { id }, user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id:int}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (id <= 0) return BadRequest(new { title = "Invalid ID.", status = 400 });
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (!users.TryGetValue(id, out var existing))
                return NotFound(new { title = "User not found.", status = 404, id });

            // Email uniqueness check (excluding current user)
            var emailExists = users.Values.Any(u =>
                u.Id != id && string.Equals(u.Email, updatedUser.Email, StringComparison.OrdinalIgnoreCase));
            if (emailExists)
                return Conflict(new { title = "Email already in use.", status = 409, email = updatedUser.Email });

            existing.Name = updatedUser.Name.Trim();
            existing.Email = updatedUser.Email.Trim();
            existing.Age = updatedUser.Age;

            users[id] = existing; // replace to ensure concurrency safety
            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id:int}")]
        public IActionResult DeleteUser(int id)
        {
            if (id <= 0) return BadRequest(new { title = "Invalid ID.", status = 400 });

            if (!users.TryRemove(id, out _))
                return NotFound(new { title = "User not found.", status = 404, id });

            return NoContent();
        }
    }
}
