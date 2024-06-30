using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using game_trends_explorer.Data;
using game_trends_explorer.Models;
using ChartJSCore.Models;
using ChartJSCore.Helpers;
using ChartJSCore.Plugins.Zoom;
using ChartJs.Blazor.ChartJS.Common.Axes;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using ChartJSCore.Models.ChartJSCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChartJs.Blazor.ChartJS.ScatterChart;

namespace game_trends_explorer.Controllers
{
    public class QueryData
    {
        public string? xQuery { get; set; }
        public double? yQuery { get; set; }
    }

    public class GraphicController : Controller
    {
        private readonly GameTrendContext _context;

        public GraphicController(GameTrendContext context)
        {
            _context = context;
        }

        // GET: Graphic
        public async Task<IActionResult> Index(
            string chartType,
            string title,
            string xAxis,
            string xLabel,
            string yAxis,
            string yLabel,
            string colorPalette,
            float? minScale,
            float? maxScale,
            string sortOrder,
            string chartCode
        )
        {
            // Get list from specified x-axis.
            List<string>? xQuery = null;
            if (xAxis != null)
            {
                xQuery = GetStringQuery(GetQuery(xAxis.ToLower()));
            }
            

            // Get list from specified y-axis.
            List<double?>? yQuery = null;
            if (yAxis != null)
            {
                yQuery = GetDoubleQuery(GetQuery(yAxis.ToLower()));
            }

            // Get dictionary of color palettes and list of color palette names.
            Dictionary<string, List<string>> colorPalettes = GetColorPalettes();
            List<string> paletteNames = colorPalettes.Keys.ToList();

            // Get from specified color palette.
            List<string> paletteColors = new List<string>();
            if (!String.IsNullOrEmpty(colorPalette))
            {
                paletteColors = colorPalettes[colorPalette];
            }
            else if (chartType != "Advanced")
            {
                paletteColors = colorPalettes["Default"];
            }

            Chart? chart = null;

            // Check if X and Y Axes are specified before proceeding to chart generation.
            if (xQuery != null && yQuery != null)
            {
                if (title == null)
                {
                    title = String.Format("{0}: {1}", xAxis, yAxis);
                }

                // Combine X and Y queries into a list of QueryData objects.
                List<QueryData> queryDataList = xQuery.Zip(yQuery, (x, y) => new QueryData { xQuery = x, yQuery = y }).ToList();

                // Group the data by the X query and sum the Y query to get the combined numbers for each unique X value.
                IEnumerable<dynamic> groupedData = queryDataList
                    .GroupBy(q => q.xQuery)
                    .Select(g => new 
                    { 
                        xQuery = g.Key, 
                        Sum = g.Sum(item => item.yQuery) 
                    });
                
                // Sort the grouped data as specified.
                switch (sortOrder)
                {
                    case "Ascending":
                        groupedData = groupedData.OrderBy(g => g.Sum);
                        break;
                    case "Descending":
                        groupedData = groupedData.OrderByDescending(g => g.Sum);
                        break;
                    default:
                        break;
                }

                if (minScale == null)
                {
                    minScale = 0;
                }

                if (maxScale == null)
                {
                    maxScale = 9999;
                }
                
                // Check if min and max scale values are valid.
                if (minScale >= maxScale)
                {
                    minScale = -1;
                    maxScale = -1;
                }
                
                // Filter the grouped data by the min and max scale values.
                if (!minScale.Equals(-1))
                {
                    groupedData = groupedData.Where(g => g.Sum >= minScale);
                }
                if (!maxScale.Equals(-1))
                {
                    groupedData = groupedData.Where(g => g.Sum <= maxScale);
                }

                // Give default values to the labels if they are not specified.
                if (xLabel == null)
                {
                    xLabel = xAxis ?? "X-Axis";
                }
                if (yLabel == null)
                {
                    yLabel = yAxis ?? "Y-Axis";
                }

                // Create seperate lists for the X and Y queries.
                List<string> xQueryList = new List<string>();
                List<double?> yQueryList = new List<double?>();
                foreach (var data in groupedData)
                {
                    string xQueryValue = data.xQuery;
                    double? sumValue = data.Sum;
                    
                    if (xQueryValue != "All" && xQueryValue != "Series" && sumValue != 0)
                    {
                        xQueryList.Add(xQueryValue);
                        yQueryList.Add(sumValue);
                    }
                }

                // Generate the chart based on the specified chart type.
                if (chartType == "Bar")
                {
                    chart = GenerateVerticalBarChart(title, xQueryList, yQueryList, paletteColors, xLabel, yLabel, minScale ?? -1, maxScale ?? -1);
                }
                else if (chartType == "Area")
                {
                    chart = GenerateAreaChart(title, xQueryList, yQueryList, paletteColors, xLabel, yLabel, minScale ?? -1, maxScale ?? -1);
                }
                else if (chartType == "Line")
                {
                    chart = GenerateLineChart(title, xQueryList, yQueryList, paletteColors, xLabel, yLabel, minScale ?? -1, maxScale ?? -1);
                }
                else if (chartType == "Scatter")
                {
                    chart = GenerateScatterPlot(title, xQueryList, yQueryList, paletteColors, xLabel, yLabel, minScale ?? -1, maxScale ?? -1);
                }
                else if (chartType == "Bubble")
                {
                    chart = GenerateBubblePlot(title, xQueryList, yQueryList, paletteColors, xLabel, yLabel, minScale ?? -1, maxScale ?? -1);
                }
                else if (chartType == "Pie")
                {
                    chart = GeneratePieChart(title, xQueryList, yQueryList, paletteColors);
                }
                else if (chartType == "Doughnut")
                {
                    chart = GenerateDoughnutChart(title, xQueryList, yQueryList, paletteColors);
                }
                else if (chartType == "Radar")
                {
                    chart = GenerateRadarChart(title, xQueryList, yQueryList, paletteColors);
                }
                else if (chartType == "Advanced")
                {
                    chart = GenerateCustomChart(chartCode, xQueryList, yQueryList, paletteColors);
                }
            }

            // Create a new GraphicViewModel object and populate it with the necessary data.
            var graphicVM = new GraphicViewModel
            {
                chartTypes = new SelectList(new List<string> { "Area", "Bar", "Line", "Scatter", "Bubble", "Pie", "Doughnut", "Radar", "Advanced" }),
                xAxes = new SelectList(new List<string> { "Name", "Platform", "Publisher", "Developer" }),
                yAxes = new SelectList(new List<string> { "Shipped", "Total", "America", "Europe", "Japan" }),
                colorPalettes = new SelectList(paletteNames),
                sorts = new SelectList(new List<string> { "None", "Ascending", "Descending" }),
                //Games = _context.Game.FirstOrDefault(),
            };

            // Populate the view with the chart and/or the GraphicViewModel object.
            ViewData["chart"] = chart;
            return View(graphicVM);
        }

        public (ParameterExpression, MemberExpression) GetQuery(string propertyName)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(Game), "g");
            MemberExpression property = Expression.Property(parameter, typeof(Game).GetProperty(propertyName) ?? throw new InvalidOperationException());
            return (parameter, property);
        }

        public List<string> GetStringQuery((ParameterExpression, MemberExpression) expressions)
        {
            ParameterExpression parameter = expressions.Item1;
            MemberExpression property = expressions.Item2;
            Expression<Func<Game, string>> select = Expression.Lambda<Func<Game, string>>(property, parameter);
            return _context.Game.Select(select).ToList();
        }

        public List<double?> GetDoubleQuery((ParameterExpression, MemberExpression) expressions)
        {
            ParameterExpression parameter = expressions.Item1;
            MemberExpression property = expressions.Item2;
            Expression<Func<Game, double?>> select = Expression.Lambda<Func<Game, double?>>(property, parameter);
            return _context.Game.Select(select).ToList();
        }

        private static Chart GenerateVerticalBarChart(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors,
            string xLabel,
            string yLabel,
            float minScale,
            float maxScale
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Bar;

            List<ChartColor> borderColors = new List<ChartColor>();
            List<ChartColor> backgorundColors = new List<ChartColor>();
            for (int i = 0; i < paletteColors.Count; i++)
            {
                borderColors.Add(ChartColor.FromHexString(paletteColors[i]));

                backgorundColors.Add(ChartColor.FromHexString(paletteColors[i]));
                backgorundColors[i].Alpha = 0.2;
            }

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            int labelCount = xQueryData.Count;

            BarDataset dataset = new BarDataset()
            {
                Label = title,
                Data = yQueryData,
                BackgroundColor = backgorundColors,
                BorderColor = borderColors,
                BorderWidth = new List<int>() { 1 },
                BarPercentage = 1,
                MaxBarThickness = 1200 / labelCount,
                MinBarLength = 2
            };

            data.Datasets = [dataset];

            chart.Data = data;

            var options = new Options
            {
                Responsive = true,
                Scales = new Dictionary<string, Scale>()
                {
                    { "y", new CartesianLinearScale()
                        {
                            BeginAtZero = true,
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {yLabel}
                            }
                        }
                    },
                    { "x", new CartesianScale()
                        {
                            Grid = new Grid()
                            {
                                Offset = true
                            },
                            Ticks = new CartesianLinearTick()
                            {
                                AutoSkip = false,
                                MaxRotation = 90,
                                MinRotation = 90
                            },
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {xLabel}
                            }
                        }
                    }
                },
                Plugins = new Plugins()
                {
                    Colors = new ColorPlugin()
                    {
                        Enabled = true,
                        ForceOverride = true
                    }
                }
            };

            chart.Options = options;

            chart.Options.Layout = new Layout
            {
                Padding = new Padding
                {
                    PaddingObject = new PaddingObject
                    {
                        Left = 10,
                        Right = 20
                    }
                }
            };

            if (minScale != -1)
            {
                decimal min = (decimal)minScale;
                chart.Options.Scales["y"].Min = (int)Math.Round(min);
            }
            if (maxScale != -1)
            {
                decimal max = (decimal)maxScale;
                chart.Options.Scales["y"].Max = (int)Math.Round(maxScale);
            }

            return chart;
        }

        private static Chart GenerateAreaChart(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors,
            string xLabel,
            string yLabel,
            float minScale,
            float maxScale
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Line;

            List<ChartColor> borderColors = new List<ChartColor>();
            List<ChartColor> backgorundColors = new List<ChartColor>();
            for (int i = 0; i < 1; i++)
            {
                borderColors.Add(ChartColor.FromHexString(paletteColors[i]));

                backgorundColors.Add(ChartColor.FromHexString(paletteColors[i]));
                backgorundColors[i].Alpha = 0.4;
            }

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            LineDataset dataset = new LineDataset()
            {
                Label = title,
                Data = yQueryData,
                Fill = "true",
                Tension = .01,
                BackgroundColor = backgorundColors,
                BorderColor = borderColors,
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = borderColors,
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = borderColors,
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = [dataset];

            chart.Data = data;

            var options = new Options
            {
                Responsive = true,
                Scales = new Dictionary<string, Scale>()
                {
                    { "y", new CartesianLinearScale()
                        {
                            BeginAtZero = true,
                            Type = "linear",
                            Grace = "50%",
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {yLabel}
                            }
                        }
                    },
                    { "x", new CartesianScale()
                        {
                            Display = true,
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {xLabel}
                            },
                            Ticks = new CartesianLinearTick()
                            {
                                AutoSkip = false,
                                MaxRotation = 90,
                                MinRotation = 90
                            }
                        }
                    }
                },
                Plugins = new Plugins()
                {
                    Colors = new ColorPlugin()
                    {
                        Enabled = true,
                        ForceOverride = true
                    }
                }
            };

            chart.Options = options;

            chart.Options.Layout = new Layout
            {
                Padding = new Padding
                {
                    PaddingObject = new PaddingObject
                    {
                        Left = 10,
                        Right = 20
                    }
                }
            };

            if (minScale != -1)
            {
                decimal min = (decimal)minScale;
                chart.Options.Scales["y"].Min = (int)Math.Round(min);
            }
            if (maxScale != -1)
            {
                decimal max = (decimal)maxScale;
                chart.Options.Scales["y"].Max = (int)Math.Round(maxScale);
            }

            return chart;
        }

        private static Chart GenerateLineChart(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors,
            string xLabel,
            string yLabel,
            float minScale,
            float maxScale
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Line;

            List<ChartColor> borderColors = new List<ChartColor>();
            for (int i = 0; i < 1; i++)
            {
                borderColors.Add(ChartColor.FromHexString(paletteColors[i]));
            }

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            LineDataset dataset = new LineDataset()
            {
                Label = title,
                Data = yQueryData,
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(255, 255, 255, 0) },
                BorderColor = borderColors,
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = borderColors,
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = borderColors,
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = [dataset];

            chart.Data = data;

            var options = new Options
            {
                Responsive = true,
                Scales = new Dictionary<string, Scale>()
                {
                    { "y", new CartesianLinearScale()
                        {
                            BeginAtZero = true,
                            Type = "linear",
                            Grace = "50%",
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {yLabel}
                            }
                        }
                    },
                    { "x", new CartesianScale()
                        {
                            Display = true,
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {xLabel}
                            },
                            Ticks = new CartesianLinearTick()
                            {
                                AutoSkip = false,
                                MaxRotation = 90,
                                MinRotation = 90
                            }
                        }
                    }
                },
                Plugins = new Plugins()
                {
                    Colors = new ColorPlugin()
                    {
                        Enabled = true,
                        ForceOverride = true
                    }
                }
            };

            chart.Options = options;

            chart.Options.Layout = new Layout
            {
                Padding = new Padding
                {
                    PaddingObject = new PaddingObject
                    {
                        Left = 10,
                        Right = 20
                    }
                }
            };

            if (minScale != -1)
            {
                decimal min = (decimal)minScale;
                chart.Options.Scales["y"].Min = (int)Math.Round(min);
            }
            if (maxScale != -1)
            {
                decimal max = (decimal)maxScale;
                chart.Options.Scales["y"].Max = (int)Math.Round(maxScale);
            }

            return chart;
        }

        private static Chart GenerateScatterPlot(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors,
            string xLabel,
            string yLabel,
            float minScale,
            float maxScale
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Line;

            List<ChartColor> pointColors = new List<ChartColor>();
            for (int i = 0; i < paletteColors.Count; i++)
            {
                pointColors.Add(ChartColor.FromHexString(paletteColors[i]));
            }

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            LineDataset dataset = new LineDataset()
            {
                Label = title,
                Data = yQueryData,
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(255, 255, 255, 0) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgba(255, 255, 255, 0) },
                PointBackgroundColor = pointColors,
                PointBorderWidth = new List<int> { 0 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = pointColors,
                PointHoverBorderWidth = new List<int> { 0 },
                PointRadius = new List<int> { 4 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = [dataset];

            chart.Data = data;

            var options = new Options
            {
                Responsive = true,
                Scales = new Dictionary<string, Scale>()
                {
                    { "y", new CartesianLinearScale()
                        {
                            BeginAtZero = true,
                            Type = "linear",
                            Grace = "50%",
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {yLabel}
                            }
                        }
                    },
                    { "x", new CartesianScale()
                        {
                            Display = true,
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {xLabel}
                            },
                            Ticks = new CartesianLinearTick()
                            {
                                AutoSkip = false,
                                MaxRotation = 90,
                                MinRotation = 90
                            }
                        }
                    }
                },
                Plugins = new Plugins()
                {
                    Colors = new ColorPlugin()
                    {
                        Enabled = true,
                        ForceOverride = true
                    }
                }
            };

            chart.Options = options;

            chart.Options.Layout = new Layout
            {
                Padding = new Padding
                {
                    PaddingObject = new PaddingObject
                    {
                        Left = 10,
                        Right = 20
                    }
                }
            };

            if (minScale != -1)
            {
                decimal min = (decimal)minScale;
                chart.Options.Scales["y"].Min = (int)Math.Round(min);
            }
            if (maxScale != -1)
            {
                decimal max = (decimal)maxScale;
                chart.Options.Scales["y"].Max = (int)Math.Round(maxScale);
            }

            return chart;
        }

        private static Chart GenerateBubblePlot(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors,
            string xLabel,
            string yLabel,
            float minScale,
            float maxScale
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Line;

            List<int> pointRadii = new List<int>();
            List<int> hoverRadii = new List<int>();
            List<int> hitRadii = new List<int>();
            for (int i = 0; i < yQueryData.Count; i++)
            {
                int radius = (int)Math.Round((decimal)yQueryData[i] / 50);
                if (radius < 2)
                {
                    radius = 2;
                }

                int hoverRadius = (int)Math.Ceiling(radius * 1.25);

                pointRadii.Add(radius);
                hoverRadii.Add(hoverRadius);
                if (hoverRadius < 10)
                {
                    hitRadii.Add(10);
                }
                else
                {
                    hitRadii.Add(hoverRadius);
                }
            }

            List<ChartColor> pointColors = new List<ChartColor>();
            for (int i = 0; i < paletteColors.Count; i++)
            {
                pointColors.Add(ChartColor.FromHexString(paletteColors[i]));
            }

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            LineDataset dataset = new LineDataset()
            {
                Label = title,
                Data = yQueryData,
                Fill = "true",
                Tension = .01,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(255, 255, 255, 0) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgba(255, 255, 255, 0) },
                PointBackgroundColor = pointColors,
                PointBorderWidth = new List<int> { 0 },
                PointHoverRadius = hoverRadii,
                PointHoverBackgroundColor = pointColors,
                PointHoverBorderWidth = new List<int> { 0 },
                PointRadius = pointRadii,
                PointHitRadius = hitRadii,
                SpanGaps = false
            };

            data.Datasets = [dataset];

            chart.Data = data;

            var options = new Options
            {
                Responsive = true,
                Scales = new Dictionary<string, Scale>()
                {
                    { "y", new CartesianLinearScale()
                        {
                            BeginAtZero = true,
                            Type = "linear",
                            Grace = "50%",
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {yLabel}
                            }
                        }
                    },
                    { "x", new CartesianScale()
                        {
                            Display = true,
                            Title = new Title()
                            {
                                Display = true,
                                Text = new List<string> {xLabel}
                            },
                            Ticks = new CartesianLinearTick()
                            {
                                AutoSkip = false,
                                MaxRotation = 90,
                                MinRotation = 90
                            }
                        }
                    }
                },
                Plugins = new Plugins()
                {
                    Colors = new ColorPlugin()
                    {
                        Enabled = true,
                        ForceOverride = true
                    }
                }
            };

            chart.Options = options;

            chart.Options.Layout = new Layout
            {
                Padding = new Padding
                {
                    PaddingObject = new PaddingObject
                    {
                        Left = 10,
                        Right = 20
                    }
                }
            };

            if (minScale != -1)
            {
                decimal min = (decimal)minScale;
                chart.Options.Scales["y"].Min = (int)Math.Round(min);
            }
            if (maxScale != -1)
            {
                decimal max = (decimal)maxScale;
                chart.Options.Scales["y"].Max = (int)Math.Round(maxScale);
            }

            return chart;
        }

        private static Chart GeneratePieChart(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Pie;

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            List<ChartColor> borderColors = new List<ChartColor>();
            List<ChartColor> backgorundColors = new List<ChartColor>();
            for (int i = 0; i < paletteColors.Count; i++)
            {
                borderColors.Add(ChartColor.FromHexString(paletteColors[i]));

                backgorundColors.Add(ChartColor.FromHexString(paletteColors[i]));
                backgorundColors[i].Alpha = 0.5;
            }

            PieDataset dataset = new PieDataset()
            {
                Label = title,
                Data = yQueryData,
                BackgroundColor = backgorundColors,
                BorderColor = borderColors,
                HoverBackgroundColor = borderColors,
                BorderWidth = new List<int> { 2 }
            };

            data.Datasets = [dataset];

            chart.Data = data;

            return chart;
        }

        private static Chart GenerateDoughnutChart(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Doughnut;

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            List<ChartColor> borderColors = new List<ChartColor>();
            List<ChartColor> backgorundColors = new List<ChartColor>();
            for (int i = 0; i < paletteColors.Count; i++)
            {
                borderColors.Add(ChartColor.FromHexString(paletteColors[i]));

                backgorundColors.Add(ChartColor.FromHexString(paletteColors[i]));
                backgorundColors[i].Alpha = 0.5;
            }

            DoughnutDataset dataset = new DoughnutDataset()
            {
                Label = title,
                Data = yQueryData,
                BackgroundColor = backgorundColors,
                BorderColor = borderColors,
                HoverBackgroundColor = borderColors,
                BorderWidth = new List<int> { 2 }
            };

            data.Datasets = [dataset];

            chart.Data = data;

            return chart;
        }

        private static Chart GenerateRadarChart(
            string title,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors
        )
        {
            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Radar;

            List<ChartColor> borderColor = new List<ChartColor>();
            List<ChartColor> backgorundColor = new List<ChartColor>();
            for (int i = 0; i < 1; i++)
            {
                borderColor.Add(ChartColor.FromHexString(paletteColors[i]));

                backgorundColor.Add(ChartColor.FromHexString(paletteColors[i]));
                backgorundColor[i].Alpha = 0.2;
            }

            ChartJSCore.Models.Data data = new ChartJSCore.Models.Data();
            data.Labels = xQueryData;

            RadarDataset dataset = new RadarDataset()
            {
                Label = title,
                Data = yQueryData,
                BackgroundColor = backgorundColor,
                BorderColor = borderColor,
                PointBackgroundColor = borderColor,
                PointBorderColor = new List<ChartColor> { ChartColor.FromHexString("#fff") },
                PointHoverBackgroundColor = new List<ChartColor>() { ChartColor.FromHexString("#fff") },
                PointHoverBorderColor = borderColor,
            };

            data.Datasets = [dataset];

            chart.Data = data;

            return chart;
        }

        private static Chart? GenerateCustomChart(
            string chartCode,
            List<string> xQueryData,
            List<double?> yQueryData,
            List<string> paletteColors
        )
        {
            Chart chart = new Chart();

            try
            {
                chart = JsonConvert.DeserializeObject<Chart>(chartCode);
            }
            catch (Exception)
            {
                return null;
            }

            chart.Data.Labels = xQueryData;  
            
            chart.Data.Datasets[0].Data = yQueryData;

            List<ChartColor> borderColor = new List<ChartColor>();
            List<ChartColor> backgorundColor = new List<ChartColor>();
            if (paletteColors.Count == 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    borderColor.Add(ChartColor.FromHexString(paletteColors[i]));

                    backgorundColor.Add(ChartColor.FromHexString(paletteColors[i]));
                    backgorundColor[i].Alpha = 0.5;
                }

            chart.Data.Datasets[0].BackgroundColor = backgorundColor;
            chart.Data.Datasets[0].BorderColor = borderColor;
            }

            return chart;
        }

        private static Dictionary<string, List<string>> GetColorPalettes()
        {
            return new Dictionary<string, List<string>>
            {

                { "Default", new List<string> { "#36A2EB", "#FF6384", "#4BC0C0", "#FF9F40", "#9966FF", "#FFCD56" } },
                { "Primary", new List<string> { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF" } },
                { "Vibrant", new List<string> { "#FF6347", "#FFD700", "#FF69B4", "#00BFFF", "#7CFC00", "#FF8C00" } },
                { "Warm", new List<string> { "#780116", "#F7B538", "#DB7C26", "#D8572A", "#C32F27", "#FF4800" } },
                { "Cool", new List<string> { "#CDB4DB", "#FFC8DD", "#FFAFCC", "#BDE0FE", "#A2D2FF", "#8093F1" } },
                { "Autumn", new List<string> { "#264653", "#2A9D8F", "#E9C46A", "#F4A261", "#E76F51", "#001219" } },
                { "Spring", new List<string> { "#70D6FF", "#FF70A6", "#FF9770", "#FFD670", "#E9FF70", "#8CB369" } },
                { "Summer", new List<string> { "#CC444B", "#F4CBC6", "#F4AFAB", "#F4EEA9", "#F4F482", "#FFEA00" } },
                { "Winter", new List<string> { "#F72585", "#7209B7", "#3A0CA3", "#4361EE", "#9F0FFF", "#6FFFE9" } },
                { "Tropical", new List<string> { "#EE6055", "#60D394", "#AAF683", "#FFD97D", "#FF9B85", "#07BEB8" } }
            };
        }
    }
}
