using Microsoft.Extensions.DependencyInjection;
using Services.Auth;
using Services.Campaigns;
using Services.Catalog;
using Services.Characters;
using Services.GlobalNpcs;
using Services.NpcNotes;
using Services.Diary;
using Services.DiceRolls;
using Services.GmScreen;
using Services.LockedDays;
using Services.Metas;
using Services.Notes;
using Services.Proposals;
using Services.Sessions;
using Services.Worlds;
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
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IMetaService, MetaService>();
        services.AddScoped<ILockedDayService, LockedDayService>();
        services.AddScoped<IDiaryService, DiaryService>();
        services.AddScoped<IDiceRollService, DiceRollService>();
        services.AddScoped<IGmScreenService, GmScreenService>();
        // World rules: stateless and without DB access, hence singletons
        services.AddSingleton<IWorldRules, StormlightRules>();
        services.AddSingleton<IWorldRules, MistbornRules>();
        services.AddSingleton<IWorldRulesProvider, WorldRulesProvider>();
        return services;
    }
}
