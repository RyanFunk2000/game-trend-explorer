using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace game_trends_explorer.Models;

public class GameClassViewModel
{
    public PaginatedList<Game>? Games { get; set; }
    public SelectList? Platforms { get; set; }
    public string? GamePlatform { get; set; }
    public SelectList? Publishers { get; set; }
    public string? GamePublisher{ get; set; }
    public string? nameSearch { get; set; }
    public string? devSearch { get; set; }
}