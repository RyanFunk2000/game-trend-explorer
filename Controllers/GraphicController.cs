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
    public class GraphicController : Controller
    {
        private readonly GameTrendContext _context;

        public GraphicController(GameTrendContext context)
        {
            _context = context;
        }

        // GET: Graphic
        public async Task<IActionResult> Index()
        {
            return View(await _context.Game.ToListAsync());
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.Id == id);
        }
    }
}
