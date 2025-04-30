using System;
using System.Numerics;
using HUD;
using RWCustom;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace PitchBlack;

public class ThanatosisMeter : HudPart
{
    // Accessing BeaconCWT for the thanatosis variables
    public BeaconCWT beaconCWT;
    private Player Player
    {
        get
        {
            return hud.owner as Player;
        }
    }
    public bool Unlocked
    {
        get
        {
            // This checks maxRippleLevel originally
            return Plugin.qualiaLevel > 0f;
        }
    }
    public bool ForceShow
    {
        get
        {
            // Watcher's original line in CamoMeter
            //return this.Unlocked && ((this.Player.isCamo && this.Player.camoCharge >= this.Player.usableCamoLimit * 0.7f) || this.Player.activateCamoTimer > 0 || (this.Player.isCamo && this.Player.inCamoTime < 400));
            return Unlocked && ((beaconCWT.isDead && beaconCWT.thanatosisCharge >= beaconCWT.inThanatosisLimit * 0.7f) || (beaconCWT.isDead && beaconCWT.thanatosisCounter < 400));
        }
    }
    public ThanatosisMeter(HUD.HUD hud, FContainer fContainer) : base(hud)
    {
        pos = new Vector2(80f, 70f);
        lastPos = pos;
        fade = 0f;
        lastFade = 0f;
        meterSprite = new FSprite("Futile_White", true)
        {
            anchorX = 0.11764706f,
            scaleX = 21.25f,
            scaleY = 5f,
            shader = hud.rainWorld.Shaders["CamoMeter"]
        };
        fContainer.AddChild(meterSprite);
    }
    public override void Update()
    {
        lastPos = pos;
        lastFade = fade;
        lastFull = full;
        lastAnimTime = animTime;
        if (ForceShow)
        {
            hud.karmaMeter.forceVisibleCounter = Mathf.Max(hud.karmaMeter.forceVisibleCounter, 60);
        }
        float to;
        // If camocharge needed
        if (beaconCWT.isDead)
        {
            to = 1f;
        }
        else
        {
            to = -0.5f;
        }
        animSpeed = Custom.LerpAndTick(animSpeed, to, 0.02f, 0.01f);
        animTime += animSpeed / 40f;
        pos = hud.karmaMeter.pos;
        fade = Unlocked ? hud.karmaMeter.fade : 0f;
        if (hud.HideGeneralHud)
        {
            fade = 0f;
        }
        full = 1f - beaconCWT.thanatosisCharge / beaconCWT.inThanatosisLimit;
        Room room = Player.room;
        StoryGameSession storyGameSession = room != null ? room.game.GetStorySession : null;
        if (storyGameSession != null)
        {
            // This is how it shrinks your usable amount in CamoMeter based on how many warps you've used consecutively without resting
            percentLimited = 0f;
            if (storyGameSession.warpTraversalsLeftUntilFullWarpFatigue < storyGameSession.warpFatigueDecayLength)
            {
                percentLimited = 1f - (float)storyGameSession.warpTraversalsLeftUntilFullWarpFatigue / (float)storyGameSession.warpFatigueDecayLength;
            }
        }
    }
    public Vector2 DrawPos(float timeStacker)
    {
        return Vector2.Lerp(lastPos, pos, timeStacker);
    }
    public override void Draw(float timeStacker)
    {
        float a = Mathf.Lerp(lastFade, fade, timeStacker);
        float r = Mathf.Lerp(lastFull, full, timeStacker);
        float b = Mathf.Lerp(lastAnimTime, animTime, timeStacker);
        float num = 1f + 0.2f * Mathf.Lerp(hud.foodMeter.lastFade, hud.foodMeter.fade, timeStacker);
        meterSprite.SetPosition(DrawPos(timeStacker));
        meterSprite.color = new Color(r, percentLimited, b, a);
        meterSprite.scaleY = 5f * num;
    }
    public override void ClearSprites()
    {
        base.ClearSprites();
        meterSprite.RemoveFromContainer();
    }
    private const float spriteWidth = 340f;
    private const float spriteHeight = 80f;
    private const float spriteAnchorX = 0.11764706f;
    public Vector2 pos;
    private Vector2 lastPos;
    private FSprite meterSprite;
    private float fade;
    private float lastFade;
    private float full;
    private float lastFull;
    private float percentLimited;
    private float lastAnimTime;
    private float animTime;
    private float animSpeed;
}
