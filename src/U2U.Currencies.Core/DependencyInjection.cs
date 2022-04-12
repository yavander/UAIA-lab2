[assembly: InternalsVisibleTo("U2U.Currencies.Core.Tests")]

namespace U2U.Currencies.Core;

public static class DependencyInjection
{

  static IServiceCollection AddCurrencyServices(this IServiceCollection services)
    => services.AddSingleton<ICurrencyConverterService, CurrencyConverterService>()
               .AddSingleton<CurrencySpecificationFactory>()
               .AddSingleton<ICultureToCurrencyService, CultureToCurrencyService>()
               .AddTransient<CurrencyFacade>();

  public static IServiceCollection AddCurrencyCore(this IServiceCollection services)
    => services.AddCurrencyServices();
}
