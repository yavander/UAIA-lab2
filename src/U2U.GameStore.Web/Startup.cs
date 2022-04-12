using Microsoft.AspNetCore.Localization;
using U2U.Currencies.Core;
using U2U.Currencies.Core.Globalization;
using U2U.Currencies.Infra;
using U2U.Games.Core;
using U2U.Games.Infra;
using U2U.GameStore.Web.Sessions;

namespace U2U.GameStore.Web
{
  public class Startup
  {
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public virtual void ConfigureServices(IServiceCollection services)
    {
      // Add In-memory caching for certain services
      services.AddMemoryCache();
      // Register EFCore with global service provider
      //services.AddEntityFrameworkSqlServer();
      services.AddEntityFrameworkInMemoryDatabase();

      // Add services and repositories
      services.AddCurrencyCore();
      services.AddCurrencyInfra();
      services.AddGameCore();
      services.AddGameInfra();

      // Add session state
      services.AddDistributedMemoryCache();

      services.AddSession(options =>
      {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.Name = "U2UGameStore_Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
      });

      services.AddHttpContextAccessor();
      services.AddTransient<ISessionService, SessionService>();

      // Add MVC Controllers and Views
      services.AddControllersWithViews()
        .AddSessionStateTempDataProvider();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      using (IServiceScope? serviceScope = app.ApplicationServices
                                   .GetRequiredService<IServiceScopeFactory>()
                                   .CreateScope())
      {
        CurrencyDb currencyDb = serviceScope.ServiceProvider.GetRequiredService<CurrencyDb>();
        currencyDb.Database.EnsureCreated();
        GamesDb gamesDb = serviceScope.ServiceProvider.GetRequiredService<GamesDb>();
        gamesDb.Database.EnsureCreated();
      }

      // Get early access to the  browser's culture
      app.UseRequestLocalization(new RequestLocalizationOptions
      {
        DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCulture),
        SupportedCultures = SupportedCultures.Instance,
        SupportedUICultures = SupportedCultures.Instance
      });

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthorization();

      app.UseSession();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
