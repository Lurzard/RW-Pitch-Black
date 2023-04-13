using MonoMod.RuntimeDetour;
using System.Runtime.CompilerServices;
using UnityEngine;
//using System;
using static System.Reflection.BindingFlags;

public class NightTerrorData
{
    public int fleeing = 0;
    public Vector2 fleeTo = Vector2.zero;
}

public class ChillTheFUCKOut
{
    public int timesZapped = 0;
}

namespace PitchBlack
{
    static class Hooks
    {
        public static ConditionalWeakTable<Centipede, NightTerrorData> NightTerrorInfo = new();
        public static ConditionalWeakTable<AbstractCreature, ChillTheFUCKOut> KILLIT = new();

        internal static void Apply()
        {
            new Hook(typeof(Centipede).GetMethod("get_Red", Public | NonPublic | Instance), (System.Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.NightTerror || orig(self));
            On.Centipede.ctor += Centipede_ctor;
            On.CentipedeGraphics.ctor += CentipedeGraphics_ctor;
            On.CentipedeAI.Update += CentipedeAI_Update;
            On.Centipede.Violence += Centipede_Violence;
            On.FlareBomb.Update += FlareBomb_Update;
            On.CentipedeAI.DoIWantToShockCreature += CentipedeAI_DoIWantToShockCreature;
            On.Centipede.Shock += Centipede_Shock;
        }

        private static void Centipede_Shock(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
        {
            orig(self, shockObj);
            if (shockObj != null && shockObj.abstractPhysicalObject != null && shockObj.abstractPhysicalObject is AbstractCreature)
            {
                if (!KILLIT.TryGetValue(shockObj.abstractPhysicalObject as AbstractCreature, out var victim))
                { KILLIT.Add(shockObj.abstractPhysicalObject as AbstractCreature, victim = new ChillTheFUCKOut()); }
                victim.timesZapped++;
            }
        }

        private static bool CentipedeAI_DoIWantToShockCreature(On.CentipedeAI.orig_DoIWantToShockCreature orig, CentipedeAI self, AbstractCreature critter)
        {
            if (self.centipede.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                if (critter is Player)
                {
                    if (!KILLIT.TryGetValue(critter, out var victim))
                    { KILLIT.Add(critter, victim = new ChillTheFUCKOut()); }
                    if (victim.timesZapped < 6)
                    {
                        return true;
                    }
                }
            }
            return orig(self, critter);
        }

        private static void FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
        {
            orig(self, eu);
            if (self.burning > 0f)
            {
                for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
                {
                    if (self.room.abstractRoom.creatures[i].realizedCreature != null && (RWCustom.Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.LightIntensity * 600f) || (RWCustom.Custom.DistLess(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.LightIntensity * 1600f) && self.room.VisualContact(self.firstChunk.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos))))
                    {
                        if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplateType.NightTerror)
                        {
                            if (NightTerrorInfo.TryGetValue(self.room.abstractRoom.creatures[i].realizedCreature as Centipede, out var NTInfo))
                            {
                                NTInfo.fleeing = 40 * 18;

                                Vector2 displacement = self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos - self.firstChunk.pos;
                                NTInfo.fleeTo = self.firstChunk.pos + 9999999 * displacement;
                            }
                            if (self.thrownBy != null)
                            {
                                self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag(self.thrownBy.abstractCreature);
                            }
                        }
                    }
                }
            }
        }

        private static void Centipede_Violence(On.Centipede.orig_Violence orig, Centipede self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, 0f, stunBonus);
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }

        private static void CentipedeAI_Update(On.CentipedeAI.orig_Update orig, CentipedeAI self)
        {
            orig(self);
            if (self.centipede.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                if (NightTerrorInfo.TryGetValue(self.centipede, out var NTInfo))
                {
                    if (NTInfo.fleeing == 0)
                    {
                        for (int i = 0; i < self.centipede.room.game.Players.Count; i++)
                        {
                            if (!(self.centipede.room.game.Players[i].realizedCreature as Player).dead)
                            {
                                self.tracker.SeeCreature(self.centipede.room.game.Players[i]);
                                self.SetDestination(self.centipede.room.game.Players[i].pos);
                            }
                        }
                    }
                    else
                    {
                        NTInfo.fleeing--;

                        self.SetDestination(new WorldCoordinate(self.centipede.abstractCreature.pos.room, (int)NTInfo.fleeTo.x, (int)NTInfo.fleeTo.y, self.centipede.abstractCreature.pos.abstractNode));
                    }
                }
            }
        }

        private static void Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                if (!NightTerrorInfo.TryGetValue(self, out var _))
                { NightTerrorInfo.Add(self, _ = new NightTerrorData()); }

                self.bodyChunks = new BodyChunk[28];
                for (int i = 0; i < self.bodyChunks.Length; i++)
                {
                    float num = (float)i / (float)(self.bodyChunks.Length - 1);
                    float num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, self.size), Mathf.Lerp(4f, 6.5f, self.size), Mathf.Pow(Mathf.Clamp(Mathf.Sin(3.1415927f * num), 0f, 1f), Mathf.Lerp(0.7f, 0.3f, self.size)));
                    num2 += 1.5f;

                    self.bodyChunks[i] = new BodyChunk(self, i, new Vector2(0f, 0f), num2, Mathf.Lerp(0.042857144f, 0.32352942f, Mathf.Pow(self.size, 1.4f)));
                    self.bodyChunks[i].loudness = 0f;
                }

                for (int j = 0; j < self.bodyChunks.Length; j++)
                {
                    self.bodyChunks[j].mass += 0.02f + 0.08f * Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0f, (float)(self.bodyChunks.Length - 1), (float)j) * 3.1415927f));
                }

                self.mainBodyChunkIndex = self.bodyChunks.Length / 2;
                if (!self.Small && (self.CentiState.shells == null || self.CentiState.shells.Length != self.bodyChunks.Length))
                {
                    self.CentiState.shells = new bool[self.bodyChunks.Length];
                    for (int k = 0; k < self.CentiState.shells.Length; k++)
                    {
                        self.CentiState.shells[k] = false;
                    }
                }

                self.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[self.bodyChunks.Length * (self.bodyChunks.Length - 1) / 2];
                int num3 = 0;
                for (int l = 0; l < self.bodyChunks.Length; l++)
                {
                    for (int m = l + 1; m < self.bodyChunks.Length; m++)
                    {
                        float num4 = 0f;
                        self.bodyChunkConnections[num3] = new PhysicalObject.BodyChunkConnection(self.bodyChunks[l], self.bodyChunks[m], self.bodyChunks[l].rad + self.bodyChunks[m].rad, PhysicalObject.BodyChunkConnection.Type.Push, 1f - num4, -1f);
                        num3++;
                    }
                }
            }
        }

        private static void CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if ((ow as Centipede).abstractCreature.creatureTemplate.type == CreatureTemplateType.NightTerror)
            {
                self.hue = 0.702f;
                self.saturation = 0.96f;
            }
        }
    }
}