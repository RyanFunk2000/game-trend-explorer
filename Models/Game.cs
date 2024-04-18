using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace game_trends_explorer.Models;

public class Game
{
    public int Id { get; set; }

    [Display(Name = "Name")]
    [StringLength(60, MinimumLength = 1)]
    [Required]
    public string? name { get; set; }

    [Display(Name = "Release Date")]
    [DataType(DataType.Date)]
    public DateTime? date { get; set; }

    [Display(Name = "Platform")]
    public string? platform { get; set; }
    
    [Display(Name = "Publisher")]
    public string? publisher { get; set; }

    [Display(Name = "Developer")]
    public string? developer { get; set; }

    [Display(Name = "Total Shipped")]
    [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
    //[Column(TypeName = "decimal(5, 2)")]
    public double? shipped { get; set; }

    [Display(Name = "Total Sold")]
    [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
    //[Column(TypeName = "decimal(5, 2)")]
    public double? total { get; set; }

    [Display(Name = "Sold - America")]
    [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
    //[Column(TypeName = "decimal(5, 2)")]
    public double? america { get; set; }

    [Display(Name = "Sold - Europe")]
    [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
    //[Column(TypeName = "decimal(5, 2)")]
    public double? europe { get; set; }

    [Display(Name = "Sold - Japan")]
    [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
    //[Column(TypeName = "decimal(5, 2)")]
    public double? japan { get; set; }
}