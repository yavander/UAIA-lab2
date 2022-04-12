namespace U2U.Games.Core;

// Each library knows about its own classes which become dependencies for others.
public static class DependencyInjection
{
  public static IServiceCollection AddGameService(this IServiceCollection services)
    => services
    .AddScoped<IGameService, GameService>()
    .AddSingleton<GameSpecificationFactory>()
    .AddSingleton<PublisherSpecificationFactory>()
    .AddSingleton<GameSpecificationFactories>()
    .AddTransient<GamePriceService>();

  public static IServiceCollection AddShoppingBasket(this IServiceCollection services)
  => services
    .AddScoped<IShoppingBasketService, ShoppingBasketService>()
    .AddScoped<ICheckoutService, CheckoutService>()
    .AddSingleton<ShoppingBasketSpecificationFactory>();

  public static IServiceCollection AddEventHandlers(this IServiceCollection services)
  {
    services.AddScoped<IDomainEventHandler<GamePriceHasChanged>, GamePriceHasChangedHandler>();
    services.AddScoped<IDomainEventHandler<ShoppingBasketHasCheckedOut>, ShoppingBasketHasCheckedOutShippingHandler>();
    services.AddScoped<IDomainEventHandler<ShoppingBasketHasCheckedOut>, ShoppingBasketHasCheckedOutBillingHandler>();
    return services;
  }

  public static IServiceCollection AddGameCore(this IServiceCollection services)
  => services.AddGameService()
             .AddShoppingBasket()
             .AddEventHandlers();
}
