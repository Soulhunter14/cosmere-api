using Microsoft.Extensions.DependencyInjection;
using Services.Auth;
using Services.Campaigns;
using Services.Catalog;
using Services.Characters;
using Services.GlobalNpcs;
using Services.NpcNotes;
using Services.Notes;
using Services.Sessions;
namespace Services;

public static class Bootstrap
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<IGlobalNpcService, GlobalNpcService>();
        services.AddScoped<INpcNoteService, NpcNoteService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<INoteService, NoteService>();
        return services;
    }
}
