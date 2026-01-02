using System.Text;
using ApiFuncional.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ApiFuncional.Configuration;

public static class IdentityConfig
{
    public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApiDbContext>();
        
        //Pegando o Token e gerando a chave encodada
        var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
        //Popula a configuração //Isso me permite injetar IOptions<JwtSettings> no construtor de uma controller
        builder.Services.Configure<JwtSettings>(jwtSettingsSection);
        //Busca a instancia já configurada
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
        //Criando a chave
        var key = Encoding.ASCII.GetBytes(jwtSettings.Segredo);

        //Adicionando a autenticação
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true; //Preciso trabalhar com HTTPS
            options.SaveToken = true; //Permite que token seja salvo após uma autenticação bem sucedida

            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,//Valida o Emissor
                ValidateAudience = true,//Valida o Audiencia
                ValidAudience = jwtSettings.Audiencia,//Audiencia que pode acessar
                ValidIssuer = jwtSettings.Emissor//Emissor que emitiu o token
            };
        }) ;
        
        return builder;
    }
}