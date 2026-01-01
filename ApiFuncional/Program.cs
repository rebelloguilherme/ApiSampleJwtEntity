using System.Text;
using ApiFuncional;
using ApiFuncional.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    options.AddPolicy("Production", builder => builder.WithOrigins("https://localhost:7206").WithMethods("POST").AllowAnyHeader());

});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddDbContext<ApiDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
    // app.UseCors("Production");//Habilitar para testar CORS
}
else
{
    app.UseCors("Production");
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();