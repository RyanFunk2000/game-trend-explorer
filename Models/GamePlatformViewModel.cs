using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace game_trends_explorer.Models;

public class GamePlatformViewModel
{
    public List<Game>? Games { get; set; }
    public SelectList? Platforms { get; set; }
    public string? GamePlatform { get; set; }
    public string? SearchString { get; set; }
}