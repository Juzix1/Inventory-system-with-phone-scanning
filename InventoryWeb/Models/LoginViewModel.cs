using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;

namespace InventoryLibrary.Model.Accounts;

[AllowAnonymous]
public class LoginViewModel
{
    [Required(AllowEmptyStrings =false, ErrorMessage = "Please enter your album index")]
    public int Id {get;set;}

    [Required(AllowEmptyStrings =false, ErrorMessage = "Please enter your password")]
    public string? Password {get;set;}
}
