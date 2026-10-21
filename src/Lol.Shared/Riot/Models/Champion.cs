namespace Lol.Shared.Riot.Models;

// One champion from Data Dragon's champion list; Id is what icon and data URLs use (e.g. "MonkeyKing" for Wukong),
// Key the number Riot's other APIs use (rotations, mastery). Blurb: the start of the champion's lore
public record Champion(string Id, string Name, string Title, string IconUrl, int Key = 0, string Blurb = "", string SplashUrl = "");
