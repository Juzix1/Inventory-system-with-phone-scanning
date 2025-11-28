using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryWeb.Models;

public class Setting
{
    [Key]
    public int Id {get;set;}

    [Required]
    [MaxLength(100)]
    public string Key {get;set;}

    [Required]
    public string Value {get;set;}

}
