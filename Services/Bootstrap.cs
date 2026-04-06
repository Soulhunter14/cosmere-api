using Microsoft.Extensions.DependencyInjection;
using Services.Auth;
using Services.Campaigns;
using Services.Catalog;
using Services.Characters;
using Services.Matches;
using Services.Notes;
using Services.Npcs;
using Services.Sessions;
using Services.SideQuests;

namespace Services;

public static class Bootstrap
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<INpcService, NpcService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ISideQuestService, SideQuestService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<INoteService, NoteService>();
        return services;
    }
}
