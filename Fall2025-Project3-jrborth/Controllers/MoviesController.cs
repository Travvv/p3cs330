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
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureOpenAIService _openAi;
        private readonly SentimentIntensityAnalyzer _analyzer = new SentimentIntensityAnalyzer();

        public MoviesController(ApplicationDbContext context, IAzureOpenAIService openAi)
        {
            _context = context;
            _openAi = openAi;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            // get actors for the movie
            var actors = await _context.ActorMovies
                .Where(am => am.MovieId == movie.Id)
                .Include(am => am.Actor)
                .Select(am => am.Actor)
                .ToListAsync();

            // Generate reviews (use AzureOpenAIService)
            var (reviewResults, averageSentiment) = await _openAi.GetThreeAiReviewsAsync(movie.Title, movie.Year.ToString(), string.Empty);

            var reviews = reviewResults.Select(r => r.Text).ToList();
            var sentiments = reviewResults.Select(r => r.Compound.ToString("F3")).ToList();

            var vm = new MovieDetailsViewModel
            {
                Movie = movie,
                Actors = actors,
                Reviews = reviews,
                Sentiments = sentiments,
                AverageSentiment = averageSentiment
            };

            return View(vm);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,Year,ImdbLink")] Movie movie)
        {
            var file = Request.Form.Files["posterFile"];
            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                movie.Poster = ms.ToArray();
            }

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Year,ImdbLink")] Movie movie)
        {
            if (id != movie.Id) return NotFound();

            var existing = await _context.Movies.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = movie.Title;
            existing.Genre = movie.Genre;
            existing.Year = movie.Year;
            existing.ImdbLink = movie.ImdbLink;

            var file = Request.Form.Files["posterFile"];
            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                existing.Poster = ms.ToArray();
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
                    if (!MovieExists(existing.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(existing);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null) _context.Movies.Remove(movie);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id) => _context.Movies.Any(e => e.Id == id);
    }
}
