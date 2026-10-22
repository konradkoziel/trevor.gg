namespace trevor.Infrastructure.Web;

// What switching an account's match reports on the website came to
public enum WebTracking
{
    // The account is linked on the site and its switch now says what was asked
    Switched,

    // Nobody linked that Riot ID on the site: the bot tracks it on its own
    NotLinked,

    // Site not configured or not answering: nothing changed there
    Unavailable,
}
