using System;
using System.Collections.Generic;

namespace Intex2.Models;

public partial class Product
{
    public int ProductId { get; set; }
    
    public string Name { get; set; } = null!;

    public int Year { get; set; }

    public int NumParts { get; set; }

    public int Price { get; set; }

    public string ImgLink { get; set; } = null!;

    public string PrimaryColor { get; set; } = null!;

    public string SecondaryColor { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Category { get; set; } = null!;
}
