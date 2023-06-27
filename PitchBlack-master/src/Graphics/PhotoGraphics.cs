using System;
using UnityEngine;

namespace PitchBlack
{
    internal class PhotoGraphics
    {
        /// <summary>
        /// It's not what you think it is ;)
        /// </summary>
        public static void Apply(){
            On.PlayerGraphics.InitiateSprites += PhotoSprite_GetStyle;
            On.PlayerGraphics.ApplyPalette += PhotoSprite_Crayons;
            On.PlayerGraphics.AddToContainer += PhotoSprite_Layering;
            On.PlayerGraphics.DrawSprites += PhotoSprite_MoveItMoveIt;
        }
        
        /// <summary>
        /// Initiates the Photo's sprite, setting the cue number to the end of the array, expanding the sprite array, and creating FSprites
        /// </summary>
        private static void PhotoSprite_GetStyle(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam){
            orig(self, sLeaser, rCam);
            try{
#region Null Checks
#if false
                // Null check
                if (!(self != null && self.player != null)) {
                    Debug.LogWarning(">>> PhotoSprite_GetStyle: Null Pass");
                    return;
                }
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) {
                    Debug.LogWarning(">>> PhotoSprite_GetStyle: Photo Pass");
                    return;
                }
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) {
                    Debug.LogWarning(">>> PhotoSprite_GetStyle: CWT Access Fail Pass");
                    return;
                }
#endif
#endregion
                Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho);
                // Set index
                pho.PhotoSetUniqueSpriteIndex(sLeaser.sprites.Length);
                Debug.Log($"Length is: {sLeaser.sprites.Length}");
                Debug.Log($"Name is: {sLeaser.sprites[sLeaser.sprites.Length-1].element.name}");

                // Resize sprite array
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);

                // Set the sprites
                sLeaser.sprites[pho.photoSpriteIndex] = new FSprite("PhotoSplatter", false);
                Debug.Log($"Length is: {sLeaser.sprites.Length}");
                Debug.Log($"Name is: {sLeaser.sprites[pho.photoSpriteIndex].element.name}");

                // Quick null check to see if sprites have been successfully loaded
                if (sLeaser.sprites[pho.photoSpriteIndex] == null){
                    Debug.LogWarning(">>> PhotoSprite_GetStyle: OH FECK SPRITE RAN AWAY! GO GET EM");
                }

                // Send them to container
                self.AddToContainer(sLeaser, rCam, null);
            } catch (Exception genericException){
                Debug.LogWarning(">>> PhotoSprite_GetStyle: OH NO AN ERROR AH");
                Debug.LogError(">>> PhotoSprite_GetStyle: Photo could not get a fancy style from the barber shop");
                Debug.LogException(genericException);
            }
        }

        /// <summary>
        /// Colors the custom sprite according to each way of coloring in stuff.
        /// </summary>
        private static void PhotoSprite_Crayons(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera r, RoomPalette p){
            orig(self, sLeaser, r, p);
            try{
#region Null Checks
                // Null check
                if (self == null || self.player == null) {
                    Debug.LogWarning(">>> PhotoSprite_Crayons: Null Pass");
                    return;
                }
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) {
                    //Debug.LogWarning(">>> PhotoSprite_Crayons: Photo Pass");
                    return;
                }
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) {
                    Debug.LogWarning(">>> PhotoSprite_Crayons: CWT Access Fail Pass");
                    return;
                }
                // Sprite existence check
                if (pho.photoSpriteIndex == -1) {
                    Debug.LogWarning(">>> PhotoSprite_Crayons: PreInit Pass");
                    return;
                }
                if (pho.photoSpriteIndex + 1 == sLeaser.sprites.Length && sLeaser.sprites[pho.photoSpriteIndex] == null) {
                    Debug.LogWarning(">>> PhotoSprite_Crayons: Bad Init Pass");
                    return;
                }
#endregion

                if (pho.photoSpriteIndex < sLeaser.sprites.Length){
                    // Jolly Coop Colors!
                    if (ModManager.CoopAvailable && self.useJollyColor){
                        sLeaser.sprites[pho.photoSpriteIndex].color = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
                    }

                    // Custom color (not jolly!)
                    else if (PlayerGraphics.CustomColorsEnabled()){
                        sLeaser.sprites[pho.photoSpriteIndex].color = PlayerGraphics.CustomColorSafety(2);
                    }

                    // Arena/no-custom color
                    else {
                        sLeaser.sprites[pho.photoSpriteIndex].color = Color.white;
                    }
                }
            } catch (Exception genericException){
                Debug.LogWarning(">>> PhotoSprite_Crayons: OH NO AN ERROR AAH");
                Debug.LogError(">>> PhotoSprite_Crayons: Photo rejected crayons and is now drinking paint");
                Debug.LogException(genericException);
            }
        }

        /// <summary>
        /// Swaps the layers around so the sprite doesn't end up on top of everything
        /// </summary>
        private static void PhotoSprite_Layering(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer fContainer){
            orig(self, sLeaser, rCam, fContainer);
            try{
                #region Null Checks
                // Null check
                if (!(self != null && self.player != null)) {
                    Debug.LogWarning(">>> PhotoSprite_Lasagna: Null Pass");
                    return;
                }
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) {
                    Debug.LogWarning(">>> PhotoSprite_Lasagna: Photo Pass");
                    return;
                }
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) {
                    Debug.LogWarning(">>> PhotoSprite_Lasagna: CWT Access Fail Pass");
                    return;
                }
                // Sprite existence check
                /*if (pho.photoSpriteIndex == -1) {
                    Debug.LogWarning(">>> PhotoSprite_Lasagna: PreInit Pass");
                    return;
                }
                if (pho.photoSpriteIndex + 1 == sLeaser.sprites.Length && sLeaser.sprites[pho.photoSpriteIndex] == null) {
                    Debug.LogWarning(">>> PhotoSprite_Lasagna: Bad Init Pass");
                    return;
                }*/
                #endregion
                Debug.Log($"Debuging ATC in Photo >>> spritesLength: {sLeaser.sprites.Length}");
                if (sLeaser.sprites.Length > 13){
                    rCam.ReturnFContainer("Foreground").RemoveChild(sLeaser.sprites[pho.photoSpriteIndex]);
                    //Debug.Log(">>> PhotoSprite_Lasagna: Removed the child");
                    rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[pho.photoSpriteIndex]);
                    //Debug.Log(">>> PhotoSprite_Lasagna: Re-added the child");
                    sLeaser.sprites[pho.photoSpriteIndex].MoveBehindOtherNode(sLeaser.sprites[5]);
                }
            } catch (Exception genericException){
                Debug.LogWarning(">>> PhotoSprite_Lasagna: OH NO AN ERROR AAAH");
                Debug.LogError(">>> PhotoSprite_Lasagna: Photo rejects the one and only lasagna");
                Debug.LogException(genericException);
            }
        }

        /// <summary>
        /// Draws the sprite. Also makes sure the sprite stretches so that it's compatible with Rotund World (might as well.)
        /// </summary>
        private static void PhotoSprite_MoveItMoveIt(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos){
            orig(self, sLeaser, rCam, timeStacker, camPos);
            try{
                #region Null Checks
                // Null check
                if (!(self != null && self.player != null)) {
                    Debug.LogWarning(">>> PhotoSprite_MoveIt: Null Pass");
                    orig(self, sLeaser, rCam, timeStacker, camPos);
                    return;
                }
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) {
                    //Debug.LogWarning(">>> PhotoSprite_MoveIt: Photo Pass");
                    orig(self, sLeaser, rCam, timeStacker, camPos);
                    return;
                }
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) {
                    Debug.LogWarning(">>> PhotoSprite_MoveIt: CWT Access Fail Pass");
                    orig(self, sLeaser, rCam, timeStacker, camPos);
                    return;
                }
                // Sprite existence check
                if (pho.photoSpriteIndex == -1) {
                    Debug.LogWarning(">>> PhotoSprite_MoveIt: PreInit Pass");
                    orig(self, sLeaser, rCam, timeStacker, camPos);
                    return;
                }
                if (pho.photoSpriteIndex + 1 == sLeaser.sprites.Length && sLeaser.sprites[pho.photoSpriteIndex] == null) {
                    Debug.LogWarning(">>> PhotoSprite_MoveIt: Bad Init Pass");
                    orig(self, sLeaser, rCam, timeStacker, camPos);
                    return;
                }
                #endregion

                // Store the X, Y scales modified from the previous DrawSprites call
                if (sLeaser.sprites.Length > 9 && sLeaser.sprites[1] != null){
                    pho.photoSpriteScale[0] = sLeaser.sprites[1].scaleX;
                    pho.photoSpriteScale[1] = sLeaser.sprites[1].scaleY;
                }
                else {
                    pho.photoSpriteScale[0] = 1f;
                    pho.photoSpriteScale[1] = 1f;
                }
                orig(self, sLeaser, rCam, timeStacker, camPos); // Do DrawSprites (resets the sprite's X and Y scale)

                if (pho.photoSpriteIndex < sLeaser.sprites.Length){
                    //maths will actually make photo's splatter sprite follow the body more accurately
                    //trying to set it to another sprite's pos or rotation makes it lag behind
                    //its more noticeable the faster photo moves, ie slamming photo around using devtools
                    Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
                    Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
                    sLeaser.sprites[pho.photoSpriteIndex].x = (vector2.x * 2f + vector.x) / 3f - camPos.x;
                    sLeaser.sprites[pho.photoSpriteIndex].y = (vector2.y * 2f + vector.y) / 3f - camPos.y - self.player.sleepCurlUp * 3f;

                    sLeaser.sprites[pho.photoSpriteIndex].scaleX = pho.photoSpriteScale[0];
                    sLeaser.sprites[pho.photoSpriteIndex].scaleY = pho.photoSpriteScale[1];

                    // Draw custom sprite!
                    //sLeaser.sprites[pho.photoSpriteIndex].scaleX = pho.photoSpriteScale[0];
                    //sLeaser.sprites[pho.photoSpriteIndex].scaleY = pho.photoSpriteScale[1];
                    //sLeaser.sprites[pho.photoSpriteIndex].rotation = sLeaser.sprites[1].rotation;
                    //sLeaser.sprites[pho.photoSpriteIndex].x = sLeaser.sprites[1].x;
                    //sLeaser.sprites[pho.photoSpriteIndex].y = sLeaser.sprites[1].y;
                }
            } catch (Exception genericException){
                Debug.LogWarning(">>> PhotoSprite_MoveIt: OH NO AN ERROR AAAAH");
                Debug.LogError(">>> PhotoSprite_MoveIt: Photo will not Move it Move it, Photo will not Move it Move it, Photo will not Move it Move it, Photo will not: MOVE IT!");
                Debug.LogException(genericException);
            }
        }

    }
}