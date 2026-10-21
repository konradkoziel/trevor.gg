namespace Web.Api.Features.Builds;

// What the site shows when the API refuses a build (in Trevor's voice, like the rest of the site)
public static class BuildMessages
{
    public const string NameMissing = "Build bez nazwy? Nawet Trevor nazywa swoje zbrodnie.";
    public const string NameTooLong = "Nazwa za długa. To build, nie wypracowanie.";
    public const string UnknownChampion = "Nie ma takiego championa.";
    public const string BadItems = "Coś nie tak z itemami: maksymalnie 6 i tylko te ze sklepu.";
    public const string BadSummonerSpells = "Dwa różne czary przywoływacza, nie więcej.";
    public const string BadRunes = "Te runy nie pasują do siebie.";
    public const string BadSkillOrder = "Takiej kolejności skilli gra nie pozwala.";
    public const string TooManyBuilds = "Masz już za dużo buildów. Usuń jakiś, zanim zrobisz kolejny.";
    public const string NotYours = "To nie Twój build.";
}
