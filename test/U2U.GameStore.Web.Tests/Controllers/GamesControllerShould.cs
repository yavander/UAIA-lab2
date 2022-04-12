using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using U2U.Currencies.Core.Entities;
using U2U.Currencies.Core.ValueObjects;
using U2U.Games.Core.Entities;
using U2U.Games.Core.Services;
using U2U.GameStore.Web.ViewModels.Games;
using Xunit;

namespace U2U.GameStore.Web.Controllers.Tests
{
  public class GamesControllerShould
  {
    [Fact()]
    public async void LetTheIndexActionReturnIndexView()
    {
      // ARRANGE: Create fake dependencies
      Publisher? pub = new Publisher(1, "Test Industries");
      Game? g = pub.CreateGame("The Testing");
      Money? m = new Money(0, CurrencyName.EUR);
      List<(Game, Money)>? list = new List<(Game, Money)>() { (g, m) };
      Mock<IGameService>? fakeGameService = new Mock<IGameService>();
      fakeGameService.Setup(s => s.GamesAsync()).Returns(list.ToAsyncEnumerable());
      //var fakeShoppingBasketService = new Mock<IShoppingBasketService>();
      //fakeShoppingBasketService.Setup(s => s.AddGameToShoppingBasketWithId(It.IsAny<int>(), It.IsAny<int>()))
      //  .Returns(new ValueTask<int>(1));

      Mock<ILogger<GamesController>>? fakeLogger = new Mock<ILogger<GamesController>>();
      // ARRANGE: Create the controller
      using (GamesController? gamesController = new GamesController(fakeGameService.Object, fakeLogger.Object))
      {
        // ACT: Call the action on the sut
        IActionResult actionResult = gamesController.Index();

        // ASSERT: Did we get the expected result
        ViewResult vr = Assert.IsType<ViewResult>(actionResult);
        GamesIndexViewModel vm = Assert.IsType<GamesIndexViewModel>(vr.Model);
        await foreach ((Game game, Money money) in vm.Games)
        {
          Assert.Equal(g, game);
          Assert.Equal(m, money);
        }
      }
    }

    [Fact()]
    public async void LetTheCreateActionReturnCreateView()
    {
      List<Publisher>? publishers = new List<Publisher>()
      {
        new Publisher(1, "Test Industries"),
        new Publisher(2, "Quality Control Co")
      };
      Mock<IGameService>? fakeGameService = new Mock<IGameService>();
      fakeGameService.Setup(s => s.PublishersAsync())
        .Returns(new ValueTask<IEnumerable<Publisher>>(publishers.AsEnumerable()));
      Mock<ILogger<GamesController>>? fakeLogger = new Mock<ILogger<GamesController>>();
      using (GamesController? gamesController = new GamesController(fakeGameService.Object, fakeLogger.Object))
      {
        IActionResult actionResult = await gamesController.Create();
        ViewResult vr = Assert.IsType<ViewResult>(actionResult);
        GamesCreateViewModel vm = Assert.IsType<GamesCreateViewModel>(vr.Model);
        Assert.Equal(publishers, vm.Publishers);
      }
    }

    [Fact()]
    public async void AllowValidGameInTheCreateAction()
    {
      Publisher? publisher = new Publisher(1, "Test Industries");
      GamesCreateViewModel? vm = new GamesCreateViewModel() { PublisherId = publisher.Id, Name = "The Testing" };
      Mock<IGameService>? fakeGameService = new Mock<IGameService>();
      fakeGameService
        .Setup(s => s.PublisherWithIdAsync(It.IsAny<int>()))
        .Returns(new ValueTask<Publisher?>(publisher));
      fakeGameService.Setup(s => s.InsertGameAsync(It.IsAny<Game>())).Returns(new ValueTask());

      Mock<ILogger<GamesController>>? fakeLogger = new Mock<ILogger<GamesController>>();
      using (GamesController? gamesController = new GamesController(fakeGameService.Object, fakeLogger.Object))
      {
        IActionResult actionResult = await gamesController.Create(vm);
        RedirectToActionResult vr = Assert.IsType<RedirectToActionResult>(actionResult);
        Assert.Equal("Index", vr.ActionName);
        fakeGameService.Verify(gs =>
          gs.PublisherWithIdAsync(It.Is<int>(id => id == vm.PublisherId)));
        fakeGameService.Verify(gs =>
          gs.InsertGameAsync(It.Is<Game>(g => g.Id == 0 && g.Name == vm.Name && g.Publisher == publisher)));
      }
    }

    [Fact()]
    public async void ReturnInvalidGameInTheCreateAction()
    {
      List<Publisher>? publishers = new List<Publisher>() { new Publisher(1, "Test Industries"), new Publisher(2, "Quality Control Co") };

      GamesCreateViewModel? vm = new GamesCreateViewModel();
      Mock<IGameService>? fakeGameService = new Mock<IGameService>();
      //fakeGameService
      //  .Setup(s => s.PublishersAsync())
      //  .Returns(Task.FromResult(publishers.AsEnumerable()));
      fakeGameService
        .Setup(s => s.PublishersAsync())
        .ReturnsAsync(publishers.AsEnumerable());
      Mock<ILogger<GamesController>>? fakeLogger = new Mock<ILogger<GamesController>>();
      using (GamesController? gamesController = new GamesController(fakeGameService.Object, fakeLogger.Object))
      {
        gamesController.ModelState.AddModelError("Test", "This Test should trigger the Invalid ModelState Flow");
        IActionResult actionResult = await gamesController.Create(vm);
        ViewResult vr = Assert.IsType<ViewResult>(actionResult);
        GamesCreateViewModel newvm = Assert.IsType<GamesCreateViewModel>(vr.Model);
        Assert.Equal(publishers, vm.Publishers);
        // Assert we did't call Insert on repository
        fakeGameService.Verify(gs =>
          gs.InsertGameAsync(It.IsAny<Game>()), Times.Never);
      }
    }
  }
}


