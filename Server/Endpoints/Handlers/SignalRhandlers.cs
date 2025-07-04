using SimpleVote.Server.SignalRHubs;

namespace SimpleVote.Server.Endpoints.Handlers;

public static class SignalRHandlers
{
    public static IEndpointRouteBuilder MapSignalRs(this IEndpointRouteBuilder route)
    {
        route.MapHub<ChatHub>("/chat");
        route.MapHub<VoteHub>("/watchvote", configure =>
        {
            configure.CloseOnAuthenticationExpiration = true;
        }).RequireAuthorization();

        return route;
    }
}