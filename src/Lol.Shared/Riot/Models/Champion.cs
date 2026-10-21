namespace Lol.Shared.Riot.Models;

// One champion from Data Dragon's champion list; Id is what icon and data URLs use (e.g. "MonkeyKing" for Wukong)
public record Champion(string Id, string Name, string Title, string IconUrl);
