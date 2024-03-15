using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using game_trends_explorer.Models;

namespace GameTrends.Data
{
    public class GameTrendsContext : DbContext
    {
        public GameTrendsContext (DbContextOptions<GameTrendsContext> options)
            : base(options)
        {
        }

        public DbSet<game_trends_explorer.Models.Game> Game { get; set; } = default!;
    }
}
