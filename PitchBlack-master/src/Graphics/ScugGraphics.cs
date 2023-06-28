using System;
using static PitchBlack.Plugin;
using Debug = UnityEngine.Debug;
using Vector2 = UnityEngine.Vector2;
using Colour = UnityEngine.Color;
using MonoMod.Cil;
using Mono.Cecil.Cil;

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

    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsBeaconOrPhoto)
        {
            cwt.whiskers = new(self);
        }
    }
    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.whiskers != null)
        {
            cwt.whiskers.Update();
        }
    }

    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsBeaconOrPhoto && !cwt.SpritesInitialized)
        {
            cwt.SpritesInitialized = true;

            #region photo splat sprite
            if (cwt.IsPhoto)
            {
                cwt.Photo.PhotoSetUniqueSpriteIndex(sLeaser.sprites.Length);
                Debug.Log($"Length is: {sLeaser.sprites.Length}");
                Debug.Log($"Name is: {sLeaser.sprites[sLeaser.sprites.Length - 1].element.name}");

                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

                sLeaser.sprites[cwt.Photo.photoSpriteIndex] = new FSprite("PhotoSplatter", false);
                Debug.Log($"Length is: {sLeaser.sprites.Length}");
                Debug.Log($"Name is: {sLeaser.sprites[cwt.Photo.photoSpriteIndex].element.name}");
            }
            #endregion

            #region whiskers
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
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsBeaconOrPhoto && cwt.SpritesInitialized)
        {
            cwt.SpritesInitialized = false;
            if (cwt.IsPhoto)
            {
                Debug.Log($"Debuging ATC in Photo >>> spritesLength: {sLeaser.sprites.Length}");
                if (sLeaser.sprites.Length > 13)
                {
                    rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[cwt.Photo.photoSpriteIndex]);
                    rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[cwt.Photo.photoSpriteIndex]);
                    sLeaser.sprites[cwt.Photo.photoSpriteIndex].MoveBehindOtherNode(sLeaser.sprites[5]);
                }
            }
            cwt.whiskers.AddToContainer(sLeaser, rCam);
        }
    }
    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        bool GotCWTData = scugCWT.TryGetValue(self.player, out ScugCWT cwt);

        if (GotCWTData && cwt.IsPhoto)
        {
            // Store the X, Y scales modified from the previous DrawSprites call
            if (sLeaser.sprites.Length > 9 && sLeaser.sprites[1] != null)
            {
                cwt.Photo.photoSpriteScale[0] = sLeaser.sprites[1].scaleX;
                cwt.Photo.photoSpriteScale[1] = sLeaser.sprites[1].scaleY;
            }
            else
            {
                cwt.Photo.photoSpriteScale[0] = 1f;
                cwt.Photo.photoSpriteScale[1] = 1f;
            }
        }

        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (GotCWTData && cwt.IsBeaconOrPhoto)
        {
            if (cwt.IsPhoto && cwt.Photo.photoSpriteIndex < sLeaser.sprites.Length)
            {
                //maths will actually make photo's splatter sprite follow the body more accurately
                //trying to set it to another sprite's pos or rotation makes it lag behind
                //its more noticeable the faster photo moves, ie slamming photo around using devtools
                Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                sLeaser.sprites[cwt.Photo.photoSpriteIndex].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
                sLeaser.sprites[cwt.Photo.photoSpriteIndex].y = (vector2.y * 2f + vector.y) / 3f - camPos.y - self.player.sleepCurlUp * 3f;

                sLeaser.sprites[cwt.Photo.photoSpriteIndex].scaleX = cwt.Photo.photoSpriteScale[0];
                sLeaser.sprites[cwt.Photo.photoSpriteIndex].scaleY = cwt.Photo.photoSpriteScale[1];
            }
            cwt.whiskers.DrawSprites(sLeaser, timeStacker, camPos);
        }
    }
    private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (scugCWT.TryGetValue(self.player, out ScugCWT cwt) && cwt.IsPhoto && cwt.Photo.photoSpriteIndex < sLeaser.sprites.Length)
        {
            // Jolly Coop Colors!
            if (ModManager.CoopAvailable && self.useJollyColor)
            {
                sLeaser.sprites[cwt.Photo.photoSpriteIndex].color = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
            }
            // Custom color (not jolly!)
            else if (PlayerGraphics.CustomColorsEnabled())
            {
                sLeaser.sprites[cwt.Photo.photoSpriteIndex].color = PlayerGraphics.CustomColorSafety(2);
            }
            // Arena/no-custom color
            else
            {
                sLeaser.sprites[cwt.Photo.photoSpriteIndex].color = Colour.white;
            }
        }
    }
}
