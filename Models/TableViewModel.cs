using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace game_trends_explorer.Models;

public class TableViewModel
{
    public PaginatedList<Game>? Games { get; set; }
    public SelectList? Platforms { get; set; }
    public string? GamePlatform { get; set; }
    public SelectList? Publishers { get; set; }
    public string? GamePublisher{ get; set; }
    public string? GameName { get; set; }
    public string? DevName { get; set; }
}