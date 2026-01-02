namespace ApiFuncional.Configuration;

public static class CorsConfig
{
    public static WebApplicationBuilder AddCorsConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Development", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            options.AddPolicy("Production", builder => builder.WithOrigins("https://localhost:7206").WithMethods("POST").AllowAnyHeader());

        });
        
        return builder;
    }
}