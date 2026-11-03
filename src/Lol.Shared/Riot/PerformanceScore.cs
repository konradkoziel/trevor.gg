using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

// How much a player did for their team, beyond KDA: every part is the player's share of their team's total compared
// with an even share (1 = an average teammate), so a support's heals and a tank's soaked damage count as much as a
// carry's kills. Higher is better: the bot's match reports and the website's match pages pick the MVP (highest of the
// team) and the report candidate (lowest) by it
public static class PerformanceScore
{
    // Weights of the parts; together 1, so an average teammate scores about 1 minus the deaths penalty
    private const double KillParticipationWeight = 0.30;
    private const double DamageWeight = 0.25;
    private const double TankingWeight = 0.15;
    private const double ObjectivesWeight = 0.10;
    private const double TeamplayWeight = 0.10;
    private const double VisionWeight = 0.10;
    // Dying twice as often as an average teammate costs about half a teammate's worth
    private const double DeathsWeight = 0.25;
    // One part can't carry the score alone (e.g. the only one who hit a tower)
    private const double MaxShare = 3.0;

    // team: the player's own team (their pair in Arena), the player included
    public static double Of(RiotMatchParticipant p, IReadOnlyList<RiotMatchParticipant> team) =>
        KillParticipationWeight * Share(p, team, x => x.Kills + x.Assists)
        + DamageWeight * Share(p, team, x => x.TotalDamageDealtToChampions)
        + TankingWeight * Share(p, team, x => x.TotalDamageTaken + x.DamageSelfMitigated)
        + ObjectivesWeight * Share(p, team, x => x.DamageDealtToObjectives)
        + TeamplayWeight * Share(p, team, x => x.TotalHealsOnTeammates + x.TotalDamageShieldedOnTeammates + x.TimeCCingOthers)
        + VisionWeight * Share(p, team, x => x.VisionScore)
        - DeathsWeight * Share(p, team, x => x.Deaths);

    // The player's value against the team's average; 0 when nobody on the team has any (old matches lack some stats)
    private static double Share(RiotMatchParticipant p, IReadOnlyList<RiotMatchParticipant> team, Func<RiotMatchParticipant, double> value)
    {
        var average = team.Average(value);
        return average <= 0 ? 0 : Math.Min(value(p) / average, MaxShare);
    }
}
