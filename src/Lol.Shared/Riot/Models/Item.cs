namespace Lol.Shared.Riot.Models;

// A Summoner's Rift shop item. Finished = builds into nothing (legendaries, boots, starters, consumables)
public record Item(string Id, string Name, string Description, int TotalGold, IReadOnlyList<string> Tags, bool IsFinished, string IconUrl);
