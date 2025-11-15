using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fall2025_Project3_jrborth.Controllers
{
    public class ActorMoviesController : Controller
    {
        // GET: ActorMoviesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ActorMoviesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ActorMoviesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ActorMoviesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ActorMoviesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ActorMoviesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ActorMoviesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ActorMoviesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
