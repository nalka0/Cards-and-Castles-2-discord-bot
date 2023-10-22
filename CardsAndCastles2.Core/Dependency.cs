namespace CardsAndCastles2.Core;
using Unity;

public static class Dependency
{
    private static Lazy<UnityContainer> _container = new(() =>
    {
        UnityContainer ret = new();
        ret.RegisterSingleton<ICardDataProvider, CookieCardDatabase>();
        ret.RegisterSingleton<IDeckCodeParser, DeckCodeParser>();
        return ret;
    });

    public static UnityContainer Container => _container.Value;
}
