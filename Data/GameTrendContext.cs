using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using game_trends_explorer.Models;

namespace game_trends_explorer.Data
{
    public class GameTrendContext : DbContext
    {
        public GameTrendContext (DbContextOptions<GameTrendContext> options)
            : base(options)
        {
        }

        public DbSet<game_trends_explorer.Models.Game> Game { get; set; } = default!;
    }
}
