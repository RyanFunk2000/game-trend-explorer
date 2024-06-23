using game_trends_explorer.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace game_trends_explorer.Models;

public class GraphicViewModel
{
    //public Game? Games { get; set; }
    public SelectList? chartTypes { get; set; }
    public string? chartType { get; set; }
    public string? Title { get; set; }
    public SelectList? xAxes { get; set; }
    public string? xAxis { get; set; }
    public string? xLabel { get; set; }
    public SelectList? yAxes { get; set; }
    public string? yAxis { get; set; }
    public string? yLabel { get; set; }
    public float? minScale { get; set; }
    public float? maxScale { get; set; }
    public SelectList? sorts { get; set; }
    public string? sortOrder { get; set; }
    public SelectList? colorPalettes { get; set; }
    public string? colorPalette { get; set; }
    public string? chartCode { get; set; }
}