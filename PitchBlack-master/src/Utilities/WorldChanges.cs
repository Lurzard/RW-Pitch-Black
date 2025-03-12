using System.IO;
using System.Linq;
using UnityEngine;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Random = UnityEngine.Random;
using RWCustom;
using System.Runtime.CompilerServices;

namespace PitchBlack;
class CicadaCWT
{
    public int glowSprite1;
    public int glowSprite2;
    public int lightBulbSprite;
}
public static class WorldChanges
{
    static ConditionalWeakTable<CicadaGraphics, CicadaCWT> cicadaCWT = new ConditionalWeakTable<CicadaGraphics, CicadaCWT>();
    public static void Apply()
    {
        //On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        On.RoofTopView.ctor += RoofTopView_ctor;
        On.AboveCloudsView.ctor += AboveCloudsView_ctor;
        On.Expedition.NeuronDeliveryChallenge.ValidForThisSlugcat += NeuronDeliveryChallenge_ValidForThisSlugcat;
        On.Expedition.PearlDeliveryChallenge.ValidForThisSlugcat += PearlDeliveryChallenge_ValidForThisSlugcat;
#if PLAYTEST
        //On.Room.Update += Room_Update;
        //On.KarmaFlower.ApplyPalette += KarmaFlower_ApplyPalette;
        //new Hook(typeof(ElectricDeath).GetMethod("get_Intensity", Public | NonPublic | Instance), ElecIntensity);
        On.CicadaGraphics.InitiateSprites += CicadaGraphics_InitiateSprites;
        On.CicadaGraphics.DrawSprites += CicadaGraphics_DrawSprites;
        On.CicadaGraphics.AddToContainer += CicadaGraphics_AddToContainer;
        On.CicadaGraphics.ctor += CicadaGraphics_ctor;
        IL.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatSelectMenu_SlugcatPageContinue_ctor;
#endif
    }
#if PLAYTEST
    static void CicadaGraphics_ctor(On.CicadaGraphics.orig_ctor orig, CicadaGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (MiscUtils.IsBeaconOrPhoto(ow.room.game.session) && !cicadaCWT.TryGetValue(self, out _)) {
            cicadaCWT.Add(self, new CicadaCWT());
        }
    }
    static void CicadaGraphics_AddToContainer(On.CicadaGraphics.orig_AddToContainer orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);
        if (sLeaser.sprites.Length > 14 && MiscUtils.IsBeaconOrPhoto(rCam.game.session) && cicadaCWT.TryGetValue(self, out CicadaCWT cwt)) {
            sLeaser.sprites[cwt.glowSprite1].RemoveFromContainer();
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[cwt.glowSprite1]);
            sLeaser.sprites[cwt.glowSprite2].RemoveFromContainer();
            rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[cwt.glowSprite2]);
        }
    }
    static void CicadaGraphics_DrawSprites(On.CicadaGraphics.orig_DrawSprites orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session) && cicadaCWT.TryGetValue(self, out CicadaCWT cwt)) {
            sLeaser.sprites[cwt.glowSprite1].scale = 8;
            sLeaser.sprites[cwt.glowSprite1].SetPosition(self.cicada.mainBodyChunk.pos-camPos);
            sLeaser.sprites[cwt.glowSprite2].scale = 24;
            sLeaser.sprites[cwt.glowSprite2].SetPosition(self.cicada.bodyChunks[0].pos + (80f*Custom.DirVec(self.cicada.bodyChunks[0].pos, self.cicada.bodyChunks[1].pos)) - camPos);

            Vector3 color = Custom.RGB2HSL(self.shieldColor);
            color.x = Mathf.Lerp(color.x, 20f/360f, 0.9f);
            color.y += (1f-color.y)/2f;
            color.z = Mathf.Lerp(color.z, 0.45f, 0.9f);
            Color rgbColor = Custom.HSL2RGB(color.x, color.y, color.z);
            sLeaser.sprites[cwt.glowSprite1].color = rgbColor;
            sLeaser.sprites[cwt.glowSprite2].color = rgbColor;
            sLeaser.sprites[self.ShieldSprite].color = rgbColor;
            sLeaser.sprites[self.cicada.gender? self.EyesBSprite : self.EyesASprite].color = rgbColor;

            Color lerpColor = new Color(10f/255f, 10f/255f, 10f/255f);
            sLeaser.sprites[self.BodySprite].color = Color.Lerp(sLeaser.sprites[self.BodySprite].color, lerpColor, 0.9f);
            sLeaser.sprites[self.HeadSprite].color = Color.Lerp(sLeaser.sprites[self.HeadSprite].color, lerpColor, 0.9f);
            sLeaser.sprites[self.HighlightSprite].color = Color.Lerp(sLeaser.sprites[self.HighlightSprite].color, lerpColor, 0.8f);
        }
    }
    static void CicadaGraphics_InitiateSprites(On.CicadaGraphics.orig_InitiateSprites orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (MiscUtils.IsBeaconOrPhoto(rCam.game.session) && cicadaCWT.TryGetValue(self, out CicadaCWT cwt)) {
            cwt.glowSprite1 = sLeaser.sprites.Length;
            cwt.glowSprite2 = sLeaser.sprites.Length+1;
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length+2);
            sLeaser.sprites[cwt.glowSprite1] = new FSprite("Futile_White") {
                shader = rCam.game.rainWorld.Shaders["LightSource"]
            };
            sLeaser.sprites[cwt.glowSprite2] = new FSprite("Futile_White") {
                shader = rCam.game.rainWorld.Shaders["LightSource"]
            };
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    private static void SlugcatSelectMenu_SlugcatPageContinue_ctor(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Int32>("ToString")))
        {
            Plugin.logger.LogError($"Pitch Black: Error in {nameof(SlugcatSelectMenu_SlugcatPageContinue_ctor)}");
            return;
        }
        cursor.Emit(OpCodes.Ldarg, 4);
        cursor.EmitDelegate((string cycleNum, SlugcatStats.Name slugcatNumber) =>
        {
            if (MiscUtils.IsBeaconOrPhoto(slugcatNumber))
            {
                int startingRange = 0;
                try
                {
                    startingRange = Convert.ToInt32(cycleNum);
                }
                catch (Exception err)
                {
                    Debug.Log($"Pitch Black: cycle number was not, in fact, a number!\n{err}");
                    startingRange = cycleNum.Length;
                }
                return MiscUtils.GenerateRandomString(startingRange, startingRange+50);
            }
            return cycleNum;
        });
    }
    public static float ElecIntensity(Func<ElectricDeath, float> orig, ElectricDeath self)
    {
        if (MiscUtils.IsBeaconOrPhoto(self.room.game.session))
        {
            return 0.2f;
        }
        return orig(self);
    }
    
    private static void Room_Update(On.Room.orig_Update orig, Room self)
    {
        orig(self);
        // This will probably work, although I wonder if it will override the end-game rain storm. Oh well, do I look like I got time to test that? (don't let me answer that)
        // If the roomRain isn't null, it's a flashcat campaign, and the ElectricDeath setting isn't present
        // We could put the code `&& self.roomSettings.DangerType != RoomRain.DangerType.Thunder` to prevent it from raining in rooms with default no rain at all
        // Also pretty unhappy with using LinQ here, but hopefully it doesn't get tooo laggy. It shouldn't as long as rooms don't have too many effects
        if (self.abstractRoom.AnySkyAccess && self.roomRain != null && MiscUtils.IsBeaconOrPhoto(self.game?.session) && !self.roomSettings.effects.Exists(x => x.type == RoomSettings.RoomEffect.Type.ElectricDeath))
        {
            self.roomRain.intensity = Mathf.Max(0.1f, Mathf.Max(self.roomRain.intensity, self.roomRain.globalRain.Intensity));
        }

    }
    private static void KarmaFlower_ApplyPalette(On.KarmaFlower.orig_ApplyPalette orig, KarmaFlower self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        //this has to be made into a seperate object I think, will come back to it later

        orig(self, sLeaser, rCam, palette);
        if (rCam.room.game.IsStorySession && rCam.room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.color = new HSLColor(0.702f, 96f, 0.53f).rgb;
        }
    }
#endif
    public static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
    {
        string text = baseAcronym;

        if (MiscUtils.IsBeaconOrPhoto(character))
        {
            switch (text)
            {
                case "SB":
                    text = "UD";
                    break;
            }

            foreach (var path in AssetManager.ListDirectory("World", true, false)
                .Select(p => AssetManager.ResolveFilePath($"World{Path.DirectorySeparatorChar}{Path.GetFileName(p)}{Path.DirectorySeparatorChar}equivalences.txt"))
                .Where(File.Exists)
                .SelectMany(p => File.ReadAllText(p).Trim().Split(',')))
            {
                var parts = path.Contains("-") ? path.Split('-') : new[] { path };
                if (parts[0] == baseAcronym && (parts.Length == 1 || character.value.Equals(parts[1], System.StringComparison.OrdinalIgnoreCase)))
                {
                    text = Path.GetFileName(path).ToUpper();
                    break;
                }
            }
            return text;
        }

        return orig(character, baseAcronym);
    }

    private static void AboveCloudsView_ctor(On.AboveCloudsView.orig_ctor orig, AboveCloudsView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.atmosphereColor = new Color(14f/255f, 19f/255f, 28f/255f);
            Color atmocolor = new Color(55f/255f, 68f/255f, 89f/255f);
            Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
            Shader.SetGlobalVector("_MultiplyColor", atmocolor);
        }
    }

    private static void RoofTopView_ctor(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
    {
        orig(self, room, effect);
        if (room.game.GetStorySession.saveStateNumber == Plugin.BeaconName)
        {
            self.atmosphereColor = new Color(14f / 255f, 19f / 255f, 28f / 255f);
            Color atmocolor = new Color(14f / 255f, 19f / 255f, 28f / 255f);
            Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", self.atmosphereColor);
            Shader.SetGlobalVector("_MultiplyColor", atmocolor);
        }
    }

    public static bool PearlDeliveryChallenge_ValidForThisSlugcat(On.Expedition.PearlDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.PearlDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        return orig(self, slugcat) && !MiscUtils.IsBeaconOrPhoto(slugcat);
    }

    public static bool NeuronDeliveryChallenge_ValidForThisSlugcat(On.Expedition.NeuronDeliveryChallenge.orig_ValidForThisSlugcat orig, Expedition.NeuronDeliveryChallenge self, SlugcatStats.Name slugcat)
    {
        return orig(self, slugcat) && !MiscUtils.IsBeaconOrPhoto(slugcat);
    }
}