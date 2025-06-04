using System;
using static PitchBlack.Plugin;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class ScugGraphics
{
    public static void Apply()
    {
        //has hooks for photo's splat sprite and photo & beacon's whiskers
        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
    }

    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow) {
        orig(self, ow);
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt)) {
            cwt.whiskers = new(self);
        }
    }
    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        var GotCWTData = scugCWT.TryGetValue(self.player, out ScugCWT cwt);
        if (GotCWTData)
        {
            if (cwt.whiskers != null)
            {
                cwt.whiskers.Update();
            }

            if (cwt is BeaconCWT beaconCWT)
            {
                if (self.player.room.Darkness(self.player.mainBodyChunk.pos) > 0f)
                {
                    // This (the lightsource) is 100% causing issues. -Lur
                    //Adding lightsource if orig took it
                    if (self.lightSource == null)
                    {
                        Color lerpColor;
                        if (beaconCWT.isDead)
                        {
                            lerpColor = Color.Lerp(Plugin.BeaconDefaultColor, Plugin.NightmareColor, beaconCWT.thanatosisLerp);
                        }
                        // Otherwise use default colors.
                        else
                        {
                            lerpColor = Color.Lerp(Plugin.BeaconDefaultColor, Plugin.BeaconFullColor, beaconCWT.storage.storedFlares.Count / (float)4);
                        }

                        self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, Color.Lerp(new Color(1f, 1f, 1f), lerpColor, 0.5f), self.player);
                        self.lightSource.requireUpKeep = true;
                        self.lightSource.setRad = new float?(300f);
                        self.lightSource.setAlpha = new float?(1f);
                        self.player.room.AddObject(self.lightSource);
                    }
                    int flares = beaconCWT.storage.storedFlares.Count;

                    //DEFAULT, NO FLASHBANGS
                    float rad = flares switch
                    {
                        0 => 200,
                        1 => 300,
                        2 => 400,
                        3 => 475,
                        4 => 550,
                        // +25
                        _ => (float)(550 + (25 * (flares - 4))),
                    };

                    float alpha = flares switch
                    {
                        0 => 0.5f,
                        1 => 0.55f,
                        2 => 0.6f,
                        3 => 0.65f,
                        4 => 0.7f,
                        _ => 0.7f
                    };

                    //ROTUND WORLD SHENANIGANS
                    float baseWeight = 0.7f * self.player.slugcatStats.bodyWeightFac / 2f;
                    rad *= self.player.bodyChunks[0].mass / baseWeight / 2f;
                    //AT +2 BONUS PIPS THIS IS ROUGHLY 125% RAD. CAPPING AT 150%

                    //IF WE HAVE THE_GLOW, DON'T LET OUR GLOW STRENGTH UNDERCUT THAT
                    if (self.player.glowing && rad < 300) rad = 300;
                    if (self.player.dead || beaconCWT.isDead) rad = 200;

                    self.lightSource.setRad = rad;
                    self.lightSource.setAlpha = alpha;
                    self.lightSource.stayAlive = true;
                    self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);

                    if (beaconCWT.brightSquint > 10)
                    {
                        if (self.blink <= 0 && Random.value < 0.35f) self.player.Blink(Mathf.FloorToInt(Mathf.Lerp(3f, 8f, UnityEngine.Random.value)));
                        self.head.vel -= self.lookDirection * 3f;
                    }
                }

                // For petting Friend and Noir... I think -Moon
                // Yeah that's probably it, but I have no idea when I wrote
                //this.events.Add(new Conversation.TextEvent(this, 0, this.Translate("What are you? If I had my memories I would know..."), 0));
                int initptm = beaconCWT.petTimer;
                foreach (AbstractCreature crit in self.player.room.abstractRoom.creatures)
                {
                    if (crit.realizedCreature is Player otherPlayer && otherPlayer != self.player
                        && (otherPlayer.slugcatStats.name == new SlugcatStats.Name("Friend", false)
                        || otherPlayer.slugcatStats.name == new SlugcatStats.Name("NoirCatto", false))
                        && Vector2.Distance(otherPlayer.mainBodyChunk.pos, self.player.mainBodyChunk.pos) <= 35
                        && self.player.bodyMode == Player.BodyModeIndex.Stand
                        && otherPlayer.bodyMode == Player.BodyModeIndex.Crawl)
                    {
                        if (otherPlayer.mainBodyChunk.pos.x < self.player.mainBodyChunk.pos.x)
                        {
                            self.hands[0].absoluteHuntPos = otherPlayer.mainBodyChunk.pos + new Vector2(15 + 5f * Mathf.Sin(beaconCWT.petTimer * (Mathf.PI / 10f)), 10f);
                            self.hands[0].reachingForObject = true;
                        }
                        if (otherPlayer.mainBodyChunk.pos.x > self.player.mainBodyChunk.pos.x)
                        {
                            self.hands[1].absoluteHuntPos = otherPlayer.mainBodyChunk.pos + new Vector2(-15 + 5f * Mathf.Sin(beaconCWT.petTimer * (Mathf.PI / 10f)), 10f);
                            self.hands[1].reachingForObject = true;
                        }
                        if (beaconCWT.petTimer % 640 == 0)
                        {
                            self.player.room.PlaySound(new SoundID("NoirCatto_PurrLoop", false), otherPlayer.mainBodyChunk.pos);
                        }
                        if (beaconCWT.petTimer == initptm)
                        {
                            beaconCWT.petTimer++;
                        }
                    }
                }
            }
        }
    }

    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
        orig(self, sLeaser, rCam);
        if (Plugin.scugCWT.TryGetValue(self.player, out ScugCWT scugCWT) && !scugCWT.SpritesInitialized) {
            scugCWT.SpritesInitialized = true;

            #region photo splat sprite
            // SPLAT.SFX
            if (scugCWT is PhotoCWT photoCWT) {
                photoCWT.PhotoSetUniqueSpriteIndex(sLeaser.sprites.Length);
                // Debug.Log($"Length is: {sLeaser.sprites.Length}");
                // Debug.Log($"Name is: {sLeaser.sprites[sLeaser.sprites.Length - 1].element.name}");

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

                sLeaser.sprites[photoCWT.photoSpriteIndex] = new FSprite("PhotoSplatter", false);
                // Debug.Log($"Length is: {sLeaser.sprites.Length}");
                // Debug.Log($"Name is: {sLeaser.sprites[cwt.Photo.photoSpriteIndex].element.name}");
            }
            #endregion

            if (PBRemixMenu.hazHat.Value && self.player.room.game.session is StoryGameSession session && !MiscUtils.IsBeaconOrPhoto(session.saveStateNumber)) {
                scugCWT.hatIndex = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length+1);
                sLeaser.sprites[scugCWT.hatIndex] = new FSprite("PBHat");
            }

            #region whiskers
            scugCWT.whiskers.initialWhiskerIndex = sLeaser.sprites.Length;
            scugCWT.whiskers.endWhiskerIndex = scugCWT.whiskers.initialWhiskerIndex + scugCWT.whiskers.headScales.Length;
            scugCWT.whiskers.initialLowerWhiskerIndex = scugCWT.whiskers.initialWhiskerIndex + scugCWT.whiskers.headScales.Length / 2;

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + scugCWT.whiskers.headScales.Length);
            scugCWT.whiskers.InitiateSprites(sLeaser);
            #endregion

            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
        orig(self, sLeaser, rCam, newContatiner);
        if (Plugin.scugCWT.TryGetValue(self.player, out ScugCWT scugCWT) && scugCWT.SpritesInitialized) {
            scugCWT.SpritesInitialized = false;
            if (scugCWT is PhotoCWT photoCWT) {
                // Debug.Log($"Debuging ATC in Photo >>> spritesLength: {sLeaser.sprites.Length}");
                if (sLeaser.sprites.Length > 13) {
                    rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[photoCWT.photoSpriteIndex]);
                    rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[photoCWT.photoSpriteIndex]);
                    sLeaser.sprites[photoCWT.photoSpriteIndex].MoveBehindOtherNode(sLeaser.sprites[5]);
                }
            }
            scugCWT.whiskers.AddToContainer(sLeaser, rCam);
            if (PBRemixMenu.hazHat.Value
                && sLeaser.sprites.Length > 13
                && self.player.room.game.session is StoryGameSession session
                && !MiscUtils.IsBeaconOrPhoto(session.saveStateNumber)) {
                rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[scugCWT.hatIndex]);
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[scugCWT.hatIndex]);
                sLeaser.sprites[scugCWT.hatIndex].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
            }
        }
    }
    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
        bool GotCWTData = scugCWT.TryGetValue(self.player, out ScugCWT cwt);

        if (GotCWTData && cwt is PhotoCWT photoCWT) {
            // Store the X, Y scales modified from the previous DrawSprites call
            if (sLeaser.sprites.Length > 9 && sLeaser.sprites[1] != null) {
                photoCWT.photoSpriteScale[0] = sLeaser.sprites[1].scaleX;
                photoCWT.photoSpriteScale[1] = sLeaser.sprites[1].scaleY;
            }
            else {
                photoCWT.photoSpriteScale[0] = 1f;
                photoCWT.photoSpriteScale[1] = 1f;
            }
        }

        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (GotCWTData) {
            if (PBRemixMenu.hazHat.Value && self.player.room != null && self.player.room.game.session is StoryGameSession session && !MiscUtils.IsBeaconOrPhoto(session.saveStateNumber)) {
                Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                Vector2 position = sLeaser.sprites[9].GetPosition()+9f*Vector2.up-4f*self.lookDirection.x*Vector2.right;
                position += 4f*Mathf.Clamp(Mathf.Abs(self.player.mainBodyChunk.vel.x),0,self.player.standing?1:0)*self.player.flipDirection*Custom.PerpendicularVector(Custom.DirVec(vector, vector2))*Mathf.Lerp(1, 0, Mathf.Abs(self.player.mainBodyChunk.lastPos.y - self.player.mainBodyChunk.pos.y)*2f);
                sLeaser.sprites[cwt.hatIndex].SetPosition(position);
                sLeaser.sprites[cwt.hatIndex].scaleX = 1.1f;
                sLeaser.sprites[cwt.hatIndex].scaleY = 0.8f;
                sLeaser.sprites[cwt.hatIndex].rotation = sLeaser.sprites[9].rotation + 0.15f*sLeaser.sprites[3].rotation + Mathf.Abs(self.player.mainBodyChunk.vel.x);
                Color color = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 0);
                sLeaser.sprites[cwt.hatIndex].color = new Color(color.r*0.75f, color.g*0.75f, color.b*0.75f, color.a);
            }
            if (cwt is PhotoCWT photoCWT1 && photoCWT1.photoSpriteIndex < sLeaser.sprites.Length) {
                // Maths will actually make photo's splatter sprite follow the body more accurately
                // Trying to set it to another sprite's pos or rotation makes it lag behind
                // Its more noticeable the faster photo moves, ie slamming photo around using devtools
                Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                sLeaser.sprites[photoCWT1.photoSpriteIndex].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
                sLeaser.sprites[photoCWT1.photoSpriteIndex].y = (vector2.y * 2f + vector.y) / 3f - camPos.y - self.player.sleepCurlUp * 3f;
                sLeaser.sprites[photoCWT1.photoSpriteIndex].scaleX = photoCWT1.photoSpriteScale[0];
                sLeaser.sprites[photoCWT1.photoSpriteIndex].scaleY = photoCWT1.photoSpriteScale[1];
            }

            if (cwt is BeaconCWT beaconCWT)
            {
                beaconCWT.currentEyeColor = BeaconEyeColor;
                // Color sprites in Thanatosis, or if you're not but still colored funky
                if (beaconCWT.isDead || beaconCWT.thanatosisCounter > 0)
                {
                    if (qualiaLevel >= 3f)
                    {
                        beaconCWT.currentSkinColor = Color.Lerp(BeaconDefaultColor, BeaconDeadColor, beaconCWT.thanatosisLerp);
                        beaconCWT.currentEyeColor = Color.Lerp(BeaconEyeColor, NightmareColor, beaconCWT.thanatosisLerp);
                    }
                    else
                    {
                        beaconCWT.currentSkinColor = Color.Lerp(BeaconDefaultColor, BeaconStarveColor, beaconCWT.thanatosisLerp);
                        beaconCWT.currentEyeColor = BeaconEyeColor;
                    }
                }
                // Otherwise use default colors.
                else
                {
                    int flares = beaconCWT.storage.storedFlares.Count;
                    beaconCWT.currentSkinColor = Color.Lerp(BeaconDefaultColor, BeaconFullColor, flares / (float)4);
                }
                // Sprites
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (i != 9)
                    {
                        sLeaser.sprites[i].color = beaconCWT.currentSkinColor;
                    }
                    switch (i)
                    {
                        // Face
                        case 9:
                        {
                            if (beaconCWT.isDead)
                            {
                                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("FaceDead");
                            }
                            sLeaser.sprites[i].color = beaconCWT.currentEyeColor;
                            break;
                        }
                        // Mark + Glow
                        case 10:
                        case 11:
                            sLeaser.sprites[i].color = beaconCWT.currentSkinColor;
                            break;
                    }
                }
                // Squinting stuff
                if (beaconCWT.brightSquint > (40 * 3.5f))
                {
                    sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
                }
                if (beaconCWT.brightSquint > 10)
                {
                    sLeaser.sprites[9].x -= self.lookDirection.x * 2;
                    sLeaser.sprites[9].y -= self.lookDirection.y * 2;
                }
            }
            cwt.whiskers.DrawSprites(sLeaser, timeStacker, camPos);
        }
    }
    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
        orig(self, sLeaser, rCam, palette);
        if (!scugCWT.TryGetValue(self.player, out ScugCWT cwt)) return;
        // Color Photo's sprite
        if (cwt is PhotoCWT PhotoCWT && PhotoCWT.photoSpriteIndex < sLeaser.sprites.Length) {
            // Slugbase can now just handle this itself, wow amazing what a cool feature
            sLeaser.sprites[PhotoCWT.photoSpriteIndex].color = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);
        }
        // Color Beacon's sprites
        if (cwt is BeaconCWT beaconCWT) {
            BeaconDeadColor = palette.blackColor;
            for (int i = 0; i < sLeaser.sprites.Length; i++) {
                if (i != 9) {
                    sLeaser.sprites[i].color = beaconCWT.currentSkinColor;
                }
                else sLeaser.sprites[9].color = beaconCWT.currentEyeColor;
            }
            sLeaser.sprites[11].color = beaconCWT.currentSkinColor;
            sLeaser.sprites[10].color = beaconCWT.currentSkinColor;
        }
        // Apply whisker palette correctly
        cwt.whiskers.ApplyPalette(self, sLeaser);
    }
}
