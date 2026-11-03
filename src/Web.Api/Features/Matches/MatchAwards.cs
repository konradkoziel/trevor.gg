namespace Web.Api.Features.Matches;

// The marks of MatchPlayer.Award, the same names as src/web/src/api/matches.ts
public static class MatchAwards
{
    public const string Mvp = "mvp";
    public const string Report = "report";

    // A pair (Arena) has no "best of the team" worth a mark
    public const int MinTeamSize = 3;
}
