using System;
using Random = UnityEngine.Random;
using static PitchBlack.Plugin;
using UnityEngine;
using RWCustom;

namespace PitchBlack;

public static class ScugGraphics
{
    /// <summary>
    /// PlayerGraphics code in RW is a [trash heap]. Here's my general understanding so we can keep this tidy.
    /// - CTOR : Where everything is "set up". Inject classes of (certain/complex?) sprites in order to use them (not many player sprites need a class, but Whiskers does)
    /// - Update : Update anything of sprites, in real time
    /// - InitiateSprites : Adding sprites (internal/backend stuff) calling AddToContainer
    /// - AddToContainer : Add sprite to correct container
    /// - DrawSprites : Rendering the added sprites onto the screen (frontend stuff)
    /// - ApplyPalette : Function dedicated to coloring the sprites when drawn by DrawSprites
    ///
    /// - Hat sprite : Initiate, AddToContainer, Draw.
    /// - Whiskers sprites have their own functions from the Whiskers class that are injected into each of these hooks respectively.
    ///
    /// Beacon coloring is done in DrawSprites.
    /// Squinting is done in Update + DrawSprites.
    /// [Lur]
    /// </summary>
    
    public static void Apply()
    {
        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
    }
    
    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);

        // Each player gets whiskers!
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt)) 
        {
            cwt.whiskers = new(self);
        }
    }
    
    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        
        var GotCWTData = scugCWT.TryGetValue(self.player, out ScugCWT c);
        if (GotCWTData)
        {
            if (c is BeaconCWT cwt)
            {
                // Squinting
                if (cwt.brightSquint > 10)
                {
                    if (self.blink <= 0 && Random.value < 0.35f) self.player.Blink(Mathf.FloorToInt(Mathf.Lerp(3f, 8f, Random.value)));
                    self.head.vel -= self.lookDirection * 3f;
                }
            }
            
            // If whiskers exists, update
            if (c.whiskers != null)
            {
                c.whiskers.Update();
            }   
        }
    }
    
    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt) && !cwt.SpritesInitialized)
        {
            cwt.SpritesInitialized = true;

            if (ModOptions.hazHat.Value && self.player.room.game.session is StoryGameSession session && MiscUtils.IsBeacon(session.saveStateNumber)) {
                cwt.hatIndex = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length+1);
                sLeaser.sprites[cwt.hatIndex] = new FSprite("PBHat");
            }
            
            #region Whiskers
            cwt.whiskers.initialWhiskerIndex = sLeaser.sprites.Length;
            cwt.whiskers.endWhiskerIndex = cwt.whiskers.initialWhiskerIndex + cwt.whiskers.headScales.Length;
            cwt.whiskers.initialLowerWhiskerIndex = cwt.whiskers.initialWhiskerIndex + cwt.whiskers.headScales.Length / 2;

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + cwt.whiskers.headScales.Length);
            cwt.whiskers.InitiateSprites(sLeaser);
            #endregion
            
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    
    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (Plugin.scugCWT.TryGetValue(self.player, out ScugCWT scugCWT) && scugCWT.SpritesInitialized)
        {
            scugCWT.SpritesInitialized = false;
            
            if (ModOptions.hazHat.Value
                && sLeaser.sprites.Length > 13
                && self.player.room.game.session is StoryGameSession session
                && !MiscUtils.IsBeacon(session.saveStateNumber)) {
                rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[scugCWT.hatIndex]);
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[scugCWT.hatIndex]);
                sLeaser.sprites[scugCWT.hatIndex].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
            }
            
            scugCWT.whiskers.AddToContainer(sLeaser, rCam);
        }
    }
    
    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        
        bool GotCWTData = scugCWT.TryGetValue(self.player, out ScugCWT cwt);
        if (GotCWTData)
        {
            if (cwt is BeaconCWT bCWT)
            {
                // "gets" slugcat color stuff to then be assigned
                Color color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
                Color skinColor = new Color(color.r, color.g, color.b);
                Color eyeColor = new Color(color.r, color.g, color.b);
                
                int flares = bCWT.storage.storedFlares.Count;
                // Overrite colors with these
                skinColor = Color.Lerp(Colors.BeaconDefaultColor, Colors.BeaconFullColor, flares / (float)4);
                eyeColor = Colors.BeaconEyeColor;

                // Assign to an updating field
                bCWT.currentSkinColor = skinColor;
                bCWT.currentEyeColor = eyeColor;
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    // eyes
                    if (i != 9)
                    {
                        sLeaser.sprites[i].color = bCWT.currentSkinColor;
                    }
                    else
                    {
                        sLeaser.sprites[i].color = bCWT.currentEyeColor;
                    }
                }
                
                // Squinting
                if (bCWT.brightSquint > (40 * 3.5f))
                {
                    sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
                }
                if (bCWT.brightSquint > 10)
                {
                    sLeaser.sprites[9].x -= self.lookDirection.x * 2;
                    sLeaser.sprites[9].y -= self.lookDirection.y * 2;
                }
            }

            if (ModOptions.hazHat.Value && self.player.room != null && self.player.room.game.session is StoryGameSession session && !MiscUtils.IsBeacon(session.saveStateNumber)) {
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
            
            cwt.whiskers.DrawSprites(sLeaser, timeStacker, camPos);
        }
    }
    
    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        
        if (!scugCWT.TryGetValue(self.player, out ScugCWT c)) 
            return;
        
        Colors.playerPaletteBlack = palette.blackColor;

        // Apply whisker palette correctly
        c.whiskers.ApplyPalette(self, sLeaser);
    }
}