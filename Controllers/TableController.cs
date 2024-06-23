using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using game_trends_explorer.Data;
using game_trends_explorer.Models;

namespace game_trends_explorer.Controllers
{
    public class TableController : Controller
    {
        private readonly GameTrendContext _context;

        public TableController(GameTrendContext context)
        {
            _context = context;
        }

        // GET: Table
        public async Task<IActionResult> Index(
            string title,
            string sortOrder,
            string currentFilter,
            string gamePlatform,
            string gamePublisher,
            string nameSearch,
            string devSearch,
            int? pageNumber)
        {
            if (_context.Game == null)
            {
                return Problem("Entity set 'GameTrendContext.Game'  is null.");
            }

            // Use LINQ to get list of platforms.
            IQueryable<string> platformQuery = from g in _context.Game
                                               orderby g.platform
                                               select g.platform;

            // Use LINQ to get list of publishers.
            IQueryable<string> publisherQuery = from g in _context.Game
                                                orderby g.publisher
                                                select g.publisher;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["ShippedSortParm"] = String.IsNullOrEmpty(sortOrder) ? "ship_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["TotalSortParm"] = sortOrder == "Total" ? "total_desc" : "Total";
            ViewData["AmericaSortParm"] = sortOrder == "America" ? "am_desc" : "America";
            ViewData["EuropeSortParm"] = sortOrder == "Europe" ? "eu_desc" : "Europe";
            ViewData["JapanSortParm"] = sortOrder == "Japan" ? "jp_desc" : "Japan";

            if (nameSearch != null || devSearch != null)
            {
                pageNumber = 1;
            }
            else
            {
                nameSearch = currentFilter;
                devSearch = currentFilter;
            }

            ViewData["CurrentFilter"] = nameSearch;

            var games = from g in _context.Game
                        select g;

            if (!String.IsNullOrEmpty(nameSearch))
            {
                games = games.Where(g => g.name!.Contains(nameSearch));
            }

            if (!String.IsNullOrEmpty(devSearch))
            {
                games = games.Where(g => g.developer!.Contains(devSearch));
            }

            if (!string.IsNullOrEmpty(gamePlatform))
            {
                games = games.Where(x => x.platform == gamePlatform);
            }

            if (!string.IsNullOrEmpty(gamePublisher))
            {
                games = games.Where(x => x.publisher == gamePublisher);
            }

            switch (sortOrder)
            {
                case "ship_desc":
                    games = games.OrderByDescending(g => g.shipped);
                    break;
                case "Date":
                    games = games.OrderBy(g => g.date);
                    break;
                case "date_desc":
                    games = games.OrderByDescending(g => g.date);
                    break;
                case "Name":
                    games = games.OrderBy(g => g.name);
                    break;
                case "name_desc":
                    games = games.OrderByDescending(g => g.name);
                    break;
                case "Total":
                    games = games.OrderBy(g => g.total);
                    break;
                case "total_desc":
                    games = games.OrderByDescending(g => g.total);
                    break;
                case "America":
                    games = games.OrderBy(g => g.america);
                    break;
                case "am_desc":
                    games = games.OrderByDescending(g => g.america);
                    break;
                case "Europe":
                    games = games.OrderBy(g => g.europe);
                    break;
                case "eu_desc":
                    games = games.OrderByDescending(g => g.europe);
                    break;
                case "Japan":
                    games = games.OrderBy(g => g.japan);
                    break;
                case "jp_desc":
                    games = games.OrderByDescending(g => g.japan);
                    break;
                default:
                    games = games.OrderByDescending(g => g.shipped);
                    break;
            }

            int pageSize = 100;
            var gameVM = new TableViewModel
            {
                Platforms = new SelectList(await platformQuery.Distinct().ToListAsync()),
                Publishers = new SelectList(await publisherQuery.Distinct().ToListAsync()),
                Games = await PaginatedList<Game>.CreateAsync(games.AsNoTracking(), pageNumber ?? 1, pageSize)
            };

            
            return View(gameVM);
        }

        // GET: Table/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.Id == id);
        }
    }
}
