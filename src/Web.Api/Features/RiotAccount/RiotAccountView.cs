namespace Web.Api.Features.RiotAccount;

// One linked account on the "Konto" page; Id is the PUUID, used by the remove and make-main calls
public record RiotAccountView(string Id, string RiotId, bool IsMain);
