using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;
using StoreApi;
using CounterStrikeSharp.API.Modules.Timers;


namespace Store;

public class StoreNoClip : BasePlugin
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "[Store] NoClip";
    public override string ModuleVersion => "1.0";

    private Timer? NoClipTimer;
    private IStoreApi? StoreApi;
    public Dictionary<int, bool> decoyTeleported = new Dictionary<int, bool>();
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        StoreApi = IStoreApi.Capability.Get() ?? throw new Exception("[Store] NoClip: StoreApi couldn't be found.");

        StoreApi.RegisterType("NoClip", OnMapStart, OnServerPrecacheResources, OnEquip, OnUnequip, false, true);
    }
    public void OnMapStart()
    {
    }
    public void OnServerPrecacheResources(ResourceManifest resource)
    {
    }

    public bool OnEquip(CCSPlayerController player, Dictionary<string, string> item)
    {
        SetNoClip(player);

        float timerDuration = item.ContainsKey("timer") && float.TryParse(item["timer"], out float parsedTimer) ? parsedTimer : 5.0f;

        NoClipTimer = AddTimer(0.2f, () =>
        {
            if (NoClipTimer == null)
                return;

            SetNoClip(player);
        }, TimerFlags.REPEAT);

        AddTimer(timerDuration, () =>
        {
            RemoveNoClip(player);
            NoClipTimer?.Kill();
            NoClipTimer = null;
        });

        return true;
    }


    public bool OnUnequip(CCSPlayerController player, Dictionary<string, string> item, bool update)
    {
        return true;
    }
    public void SetNoClip(CCSPlayerController player)
    {
        if (player == null)
            return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) 
            return;

        ChangeMovetype(pawn, MoveType_t.MOVETYPE_NOCLIP);
    }
    public void RemoveNoClip(CCSPlayerController player)
    {
        if (player == null)
            return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null)
            return;

        ChangeMovetype(pawn, MoveType_t.MOVETYPE_WALK);
    }
    public void ChangeMovetype(CBasePlayerPawn pawn, MoveType_t movetype)
    {
        pawn.MoveType = movetype;
        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", movetype);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
}