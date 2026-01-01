using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiFuncional.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiFuncional.Controllers;

[ApiController]
[Route("api/conta")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    
    public AuthController(
        SignInManager<IdentityUser> signInManager, 
        UserManager<IdentityUser> userManager, 
        IOptions<JwtSettings> jwtSettings)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
    }

    
    [HttpPost("registrar")]
    public async Task<ActionResult> Registrar(RegisterUserViewModel registerUser)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        var user = new IdentityUser
        {
            UserName = registerUser.Email,
            Email = registerUser.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, registerUser.Password);

        if (!result.Succeeded)
        {
            var erros = result.Errors
                .Select(e => e.Description)
                .ToArray();

            return BadRequest(new
            {
                mensagem = "Falha ao registrar o usuário.",
                erros
            });
        }

        await _signInManager.SignInAsync(user, false);

        return Ok(await GerarJwt(registerUser.Email));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginUserViewModel loginUser)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        //lockoutOnFailure: bloqueia o login do usuário por 15 minutos se ele errar a password 5 vezes.
        var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);
        
        // if(!result.Succeeded) return Problem("Usuário ou senha incorretos");
        if (!result.Succeeded) return Unauthorized("Usuário ou senha incorretos");
        return Ok(await GerarJwt(loginUser.Email));
    }

    private async Task<string> GerarJwt(string email)
    {
        var user =  await _userManager.FindByEmailAsync(email);
        var roles= await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName)
        };
        
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Segredo);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtSettings.Emissor,
            Audience = _jwtSettings.Audiencia,
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });
        
        var encodedToken = tokenHandler.WriteToken(token);
        
        return encodedToken;
    }
}