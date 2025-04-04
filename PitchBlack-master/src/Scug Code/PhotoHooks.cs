namespace PitchBlack;
public static class PhotoHooks
{
    public static void Apply() {
        On.Player.Update += PhotoParry;
        On.Creature.Violence += Creature_Violence;
    }
    public static void PhotoParry(On.Player.orig_Update orig, Player self, bool eu) {
        orig(self, eu);
        if (Plugin.scugCWT.TryGetValue(self, out ScugCWT cwt) && cwt is PhotoCWT photoCWT) {
            photoCWT.Update(self);
        }
    }

    private static void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, UnityEngine.Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus) {
        if (PBRemixMenu.elecImmune.Value && type == Creature.DamageType.Electric && self is Player player && MiscUtils.IsPhoto(player))
            return; //WW- SKIP! ELECTRICITY IMMUNITY!
        //Centipedes with a higher mass will still kill you instantly because they just call Die()
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }


#if false
    public static void PhotoCallOverseerToRoom(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
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
        if (Plugin.Speaking)
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
#endif
}