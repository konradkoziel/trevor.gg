namespace Web.Api.Features.RiotAccount;

// One linked account on the "Konto" page; Id is the PUUID, used by the remove, make-main and tracking calls
public record RiotAccountView(string Id, string RiotId, bool IsMain, bool Tracked = false);
