using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Intex2.Models;

public partial class Order
{
    [Key]
    public int TransactionId { get; set; }

    [Key]
    public int CustomerId { get; set; }

    public DateOnly? Date { get; set; }

    public string? DayOfWeek { get; set; }

    public int? Time { get; set; }

    public string? EntryMode { get; set; }

    public int? Amount { get; set; }

    public string? TypeOfTransaction { get; set; }

    public string? CountryOfTransaction { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Bank { get; set; }

    public string? TypeOfCard { get; set; }

    public int? Fraud { get; set; }
}
