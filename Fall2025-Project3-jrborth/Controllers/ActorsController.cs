using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_jrborth.Data;
using Fall2025_Project3_jrborth.Models;
using Fall2025_Project3_jrborth.Services;
using VaderSharp2;

namespace Fall2025_Project3_jrborth.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureOpenAIService _openAi;
        private readonly SentimentIntensityAnalyzer _analyzer = new SentimentIntensityAnalyzer();

        public ActorsController(ApplicationDbContext context, IAzureOpenAIService openAi)
        {
            _context = context;
            _openAi = openAi;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null) return NotFound();

            // get movies for actor
            var movies = await _context.ActorMovies
                .Where(am => am.ActorId == actor.Id)
                .Include(am => am.Movie)
                .Select(am => am.Movie)
                .ToListAsync();

            // Generate tweets (use AzureOpenAIService)
            var (tweetResults, averageSentiment) = await _openAi.GetFiveFakeTweetsAsync(actor.Name);

            var tweets = tweetResults.Select(t => t.Text).ToList();
            var sentiments = tweetResults.Select(t => t.Compound.ToString("F3")).ToList();

            var vm = new ActorDetailsViewModel
            {
                Actor = actor,
                Movies = movies,
                Tweets = tweets,
                Sentiments = sentiments,
                AverageSentiment = averageSentiment
            };

            return View(vm);
        }

        // GET: Actors/Create
        public IActionResult Create() => View();

        // POST: Actors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,ImdbLink")] Actor actor)
        {
            var file = Request.Form.Files["photoFile"];
            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                actor.Photo = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();
            return View(actor);
        }

        // POST: Actors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,ImdbLink")] Actor actor)
        {
            if (id != actor.Id) return NotFound();

            var existing = await _context.Actors.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = actor.Name;
            existing.Gender = actor.Gender;
            existing.Age = actor.Age;
            existing.ImdbLink = actor.ImdbLink;

            var file = Request.Form.Files["photoFile"];
            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                existing.Photo = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(existing.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(existing);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null) _context.Actors.Remove(actor);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id) => _context.Actors.Any(e => e.Id == id);
    }
}
