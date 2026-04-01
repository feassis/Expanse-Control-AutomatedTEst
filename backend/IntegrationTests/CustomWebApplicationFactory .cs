using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<MinhasFinancas.API.Controllers.CategoriasController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(@"C:\Users\felip\Desktop\CSProj\JobApp\EDDT\ExameDesenvolvedorDeTestes\api\MinhasFinancas.API");
        }
    }
}