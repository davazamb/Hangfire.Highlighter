using Hangfire.Highlighter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Hangfire.Highlighter.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private readonly HighlighterDbContext _db = new HighlighterDbContext();

        public ActionResult Index()
        {
            return View(_db.CodeSnippets.ToList());
        }

        public ActionResult Details(int id)
        {
            var snippet = _db.CodeSnippets.Find(id);
            return View(snippet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "SourceCode")] CodeSnippet snippet)
        {
            if (ModelState.IsValid)
            {
                snippet.CreatedAt = DateTime.UtcNow;

                // We'll add the highlighting a bit later.

                _db.CodeSnippets.Add(snippet);
                _db.SaveChanges();

                return RedirectToAction("Details", new { id = snippet.Id });
            }

            return View(snippet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        private static async Task<string> HighlightSourceAsync(string source)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    @"http://hilite.me/api",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                    { "lexer", "c#" },
                    { "style", "vs" },
                    { "code", source }
                    }));

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string HighlightSource(string source)
        {
            // Microsoft.Net.Http does not provide synchronous API,
            // so we are using wrapper to perform a sync call.
            return RunSync(() => HighlightSourceAsync(source));
        }

        private static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return Task.Run<Task<TResult>>(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}