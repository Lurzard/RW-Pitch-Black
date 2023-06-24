using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.IO;
using System.Linq;
using RWCustom;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Security;
using MonoMod.Cil;
using static Player;
using Fisobs.Core;

namespace PitchBlack
{
    public static class PhotoHooks
    {
        public static void Apply() {
            On.Player.ctor += PhotoPlayerCtor;
            On.Player.Update += PhotoParry;
            On.Player.Update += PhotoCallOverseerToRoom;
        }
        public static void PhotoPlayerCtor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.slugcatStats.name == Plugin.PhotoName)
            {
                self.playerState.isPup = true;
            }
        }
        public static void PhotoParry(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (!Plugin.pCon.TryGetValue(self, out PhotoCWT e))
            {
                return;
            }

            // Parry
            e.UpdateParryCD(self);

            // Parry Input tolerance
            int tolerance = 3;
            bool gParryLeanPckp = false, gParryLeanJmp = false;
            for (int i = 0; i <= tolerance; i++)
            {
                if (self.input[i].pckp)
                {
                    gParryLeanPckp = i < tolerance;
                }
                if (self.input[i].jmp)
                {
                    gParryLeanJmp = i < tolerance;
                }
            }
            bool airParry = gParryLeanPckp && self.wantToJump > 0;
            bool groundParry = self.input[0].y < 0 && self.bodyChunks[1].contactPoint.y < 0 && gParryLeanJmp && gParryLeanPckp;

            if (self.Consious && (airParry || groundParry))
            {
                Debug.Log("Input - Air: " + airParry);
                Debug.Log("Input - Ground: " + groundParry);
                e.PhotoParry(self);
            }

            // Sparking when close to death VFX
            if (self.room != null && e.parryNum > e.parryMax - 5)
            {
                self.room.AddObject(new Spark(self.mainBodyChunk.pos, RWCustom.Custom.RNV(), Color.white, null, 4, 8));
            }
        }
        public static void PhotoCallOverseerToRoom(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
#if false
            if (Input.GetKey(KeyCode.C) && Plugin.pbcooldown == 0 && self.room != null && self.slugcatStats.name == Plugin.PhotoName)
            {
                Plugin.pbcooldown = 400;
                self.room.AddObject(new NeuronSpark(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y + 40f)));

                if (Plugin.Speaking == false)
                {
                    WorldCoordinate worldC = new WorldCoordinate(self.room.world.offScreenDen.index, -1, -1, 0);
                    Plugin.PBOverseer = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, worldC, new EntityID(-1, 5));
                    self.room.world.GetAbstractRoom(worldC).entitiesInDens.Add(Plugin.PBOverseer);
                    Plugin.PBOverseer.ignoreCycle = true;
                    (Plugin.PBOverseer.abstractAI as OverseerAbstractAI).spearmasterLockedOverseer = true;
                    (Plugin.PBOverseer.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(87);
                    (Plugin.PBOverseer.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
                    Debug.Log("sflost Called overseer. now trying to converse!");
                    self.room.game.cameras[0].hud.InitDialogBox();
                    Debug.Log("sflost init'd dialog box");
                    int rng = Random.Range(0, 3);
                    switch (rng)
                    {
                        case 0:
                            Plugin.currentDialog.Add("PB-Ov87 Returned : No further instructions.");
                            break;
                        case 1:
                            Plugin.currentDialog.Add("PB-Ov87 Returned : No further instructions. Continuing FreeRoam.");
                            break;
                        case 2:
                            Plugin.currentDialog.Add("PB-Ov87 Returned : No further instructions. Scanning Possible Threats.");
                            break;
                        case 3:
                            Plugin.currentDialog.Add("PB-Ov87 Returned : No further instructions. Broadcasting location to HOST.");
                            break;
                        default:
                            Plugin.currentDialog.Add("PB-Ov87 Returned : No further instructions. Locating Sources of Food.");
                            break;
                    }
                    Plugin.Speaking = true;
                }
            }
#endif
            if (Plugin.Speaking == true)
            {
                if (Plugin.currentDialog.Count == 0)
                {
                    Plugin.Speaking = false;
                }
                else
                {
                    HUD.DialogBox dialogbox = self.room.game.cameras[0].hud.dialogBox;
                    dialogbox.NewMessage(Plugin.currentDialog[0], 10);
                    Plugin.currentDialog.Remove(Plugin.currentDialog[0]);
                }
            }
            if (Plugin.pbcooldown > 0)
            {
                if (Plugin.pbcooldown > 350)
                {
                    (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (-0.01f);
                    self.room.AddObject(new NeuronSpark(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y + 40f)));
                }
                Plugin.pbcooldown--;
            }
        }
    }
}