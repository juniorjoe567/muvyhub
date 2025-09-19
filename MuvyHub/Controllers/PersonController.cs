using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuvyHub.Data;
using MuvyHub.Models;
using PagedList.Core;

namespace MuvyHub.Controllers
{
    [Authorize]
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PersonController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string search, bool? verified, string location, int page = 1, int pageSize = 40)
        {
            var query = _context.People.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.FullName.Contains(search));

            if (verified.HasValue)
                query = query.Where(p => p.IsVerified == verified.Value);

            if (!string.IsNullOrEmpty(location))
            {
                if (Enum.TryParse<MuvyHub.Models.Location>(location, out var selectedLocation))
                {
                    query = query.Where(p => p.Location == selectedLocation);
                }
            }

            query = query.OrderByDescending(p => p.Id);

            var pagedList = new PagedList<Person>(query, page, pageSize);

            ViewBag.SearchQuery = search;
            ViewBag.Verified = verified;
            ViewBag.Location = location;

            return View(pagedList);
        }

        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = _context.People.FirstOrDefault(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // GET: Person/CreatePartial
        [Authorize(Roles = "Admin")]
        public IActionResult CreatePartial()
        {
            return PartialView("_PersonCreatePartial", new Person());
        }

        // POST: Person/Create
        [HttpPost]
        public async Task<IActionResult> Create(Person person)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (person.ProfilePicture != null && person.ProfilePicture.Length > 0)
                person.ProfilePicturePath = await SaveFile(person.ProfilePicture);

            if (person.OtherMedia != null && person.OtherMedia.Count > 0)
            {
                var paths = new List<string>();
                foreach (var file in person.OtherMedia)
                    paths.Add(await SaveFile(file));
                person.OtherMediaPaths = paths;
            }

            _context.People.Add(person);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> DetailsPartial(Guid id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            return PartialView("_PersonDetailsPartial", person);
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            return "/uploads/" + fileName;
        }

        // GET: Person/EditPartial/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPartial(Guid id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            return PartialView("_PersonEditPartial", person);
        }

        // POST: Person/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, Person person)
        {
            if (id != person.Id) return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.People.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FullName = person.FullName;
            existing.DateOfBirth = person.DateOfBirth;
            existing.WhatsappNumber = person.WhatsappNumber;
            existing.Description = person.Description;
            existing.IsVerified = person.IsVerified;
            existing.Location = person.Location;

            if (person.ProfilePicture != null && person.ProfilePicture.Length > 0)
                existing.ProfilePicturePath = await SaveFile(person.ProfilePicture);

            if (person.OtherMedia != null && person.OtherMedia.Count > 0)
            {
                var paths = existing.OtherMediaPaths ?? new List<string>();
                foreach (var file in person.OtherMedia)
                    paths.Add(await SaveFile(file));
                existing.OtherMediaPaths = paths;
            }

            _context.Update(existing);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: Person/DeletePartial/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePartial(Guid id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            return PartialView("_PersonDeletePartial", person);
        }

        // POST: Person/Delete
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            _context.People.Remove(person);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
