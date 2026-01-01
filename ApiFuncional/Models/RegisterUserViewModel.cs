using System.ComponentModel.DataAnnotations;

namespace ApiFuncional.Models;

public class RegisterUserViewModel: UserViewModel
{
    [Compare("Password", ErrorMessage = "As senhas não conferem.")]
    public string ConfirmPassword { get; set; }
}