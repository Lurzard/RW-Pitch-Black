using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PitchBlack;

namespace PitchBlack
{
    internal class PhotoSprite
    {
        /// <summary>
        /// It's not what you think it is ;)
        /// </summary>
        public static void Hooker(){
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.PlayerGraphics.InitiateSprites += PhotoSprite_GetStyle;
            On.PlayerGraphics.ApplyPalette += PhotoSprite_Crayons;
            On.PlayerGraphics.AddToContainer += PhotoSprite_Layering;
            On.PlayerGraphics.DrawSprites += PhotoSprite_MoveIt;
        }

        /// <summary>
        /// If you're not using this, I'm stealing it
        /// </summary>
        private void LoadResources(RainWorld rainWorld){
            Futile f = Futile.atlasManager.LoadAtlas("atlases/photosplt");
            if (f == null){
                Debug.Log("OH NO NO SPRITES?!");
            }
        }
        

        /// <summary>
        /// Initiates the Photo's sprite, setting the cue number to the end of the array, expanding the sprite array, and creating FSprites
        /// </summary>
        private static void PhotoSprite_GetStyle(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser s, RoomCamera r){
            orig(self, s, r);
            try{
                // Null check
                if (!(self != null && self.player != null)) return;
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) return;
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) return;

                // Set index
                pho.PhotoSetSpriteIndex(s.sprites.Length);

                // Resize sprite array
                Array.Resize(ref s.sprites, s.sprites.Length + 1);

                // Set the sprites
                s.sprites[pho.photoSpriteIndex] = new FSprite("PhotoSplatter");

                // Quick null check to see if sprites have been successfully loaded
                if (s.sprites[pho.photoSpriteIndex] == null){
                    Debug.Log("OH FECK SPRITE RAN AWAY! GO GET EM");
                }

                // Send them to container
                self.AddToContainer(s, r, null);
            } catch (Exception genericException){
                Debug.Log("OH NO AN ERROR AH");
                Debug.LogError("Photo could not get a fancy style from the barber shop");
                Debug.LogException(genericException);
            }
        }

        /// <summary>
        /// Colors the custom sprite according to each way of coloring in stuff.
        /// </summary>
        private static void PhotoSprite_Crayons(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser s, RoomCamera r, RoomPalette p){
            orig(self, s, r, p);
            try{
                // Null check
                if (!(self != null && self.player != null)) return;
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) return;
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) return;
                // Sprite existence check
                if (pho.photoSpriteIndex == -1 || pho.photoSpriteIndex <= s.sprites.Length) return;
                if (pho.photoSpriteIndex + 1 == s.sprites.Length && s.sprites[pho.photoSpriteIndex] == null) return;

                // Jolly Coop Colors!
                if (ModManager.CoopAvailable && self.useJollyColor){
                    s.sprites[pho.photoSpriteIndex].color = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
                }

                // Custom color (not jolly!)
                else if (PlayerGraphics.CustomColorsEnabled()){
                    s.sprites[pho.photoSpriteIndex].color = PlayerGraphics.CustomColorSafety(2);
                }

                // Arena/no-custom color
                else {
                    s.sprites[pho.photoSpriteIndex].color = Color.white;
                }
            } catch (Exception genericException){
                Debug.Log("OH NO AN ERROR AAH");
                Debug.LogError("Photo rejected crayons and is now drinking paint");
                Debug.LogException(genericException);
            }
        }

        /// <summary>
        /// Swaps the layers around so the sprite doesn't end up on top of everything
        /// </summary>
        private static void PhotoSprite_Layering(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser s, RoomCamera r, FContainer f){
            orig(self, s, r, f);
            try{
                // Null check
                if (!(self != null && self.player != null)) return;
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) return;
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) return;
                // Sprite existence check
                if (pho.photoSpriteIndex == -1 || pho.photoSpriteIndex <= s.sprites.Length) return;
                if (pho.photoSpriteIndex + 1 == s.sprites.Length && s.sprites[pho.photoSpriteIndex] == null) return;

                r.ReturnFContainer("Foreground").RemoveChild(s.sprites[pho.photoSpriteIndex]);
                r.ReturnFContainer("Midground").AddChild(s.sprites[pho.photoSpriteIndex]);
                s.sprites[pho.photoSpriteIndex].MoveBehindOtherNode(s.sprites[3]);
            } catch (Exception genericException){
                Debug.Log("OH NO AN ERROR AAAH");
                Debug.LogError("Photo rejects the one and only lasagna");
                Debug.LogException(genericException);
            }
        }

        /// <summary>
        /// Draws the sprite. Also makes sure the sprite stretches so that it's compatible with Rotund World (might as well.)
        /// </summary>
        private static void PhotoSprite_MoveIt(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser s, RoomCamera r, float t, Vector2 p){
            orig(self, s, r, t, p);
            try{
                // Null check
                if (!(self != null && self.player != null)) {
                    orig(self, s, r, t, p);
                    return;
                }
                // Character check
                if (self.player.slugcatStats.name != Plugin.PhotoName) {
                    orig(self, s, r, t, p);
                    return;
                }
                // CWT Existencial Crisis check
                if (!Plugin.pCon.TryGetValue(self.player, out PhotoCWT pho)) {
                    orig(self, s, r, t, p);
                    return;
                }
                // Sprite existence check
                if (pho.photoSpriteIndex == -1 || pho.photoSpriteIndex <= s.sprites.Length) {
                    orig(self, s, r, t, p);
                    return;
                }
                if (pho.photoSpriteIndex + 1 == s.sprites.Length && s.sprites[pho.photoSpriteIndex] == null) {
                    orig(self, s, r, t, p);
                    return;
                }

                // Store the X, Y scales modified from the previous DrawSprites call
                if (s.sprites.Length > 9 && s.sprites[1] != null){
                    pho.photoSpriteScale[0] = s.sprites[1].scaleX;
                    pho.photoSpriteScale[1] = s.sprites[1].scaleY;
                }
                else {
                    pho.photoSpriteScale[0] = 1f;
                    pho.photoSpriteScale[1] = 1f;
                }
                orig(self, s, r, t, p); // Do DrawSprites (resets the sprite's X and Y scale)

                // Draw custom sprite!
                s.sprites[pho.photoSpriteIndex].scaleX = pho.photoSpriteScale[0];
                s.sprites[pho.photoSpriteIndex].scaleY = pho.photoSpriteScale[1];
                s.sprites[pho.photoSpriteIndex].rotation = s.sprites[1].rotation;
                s.sprites[pho.photoSpriteIndex].x = s.sprites[1].x;
                s.sprites[pho.photoSpriteIndex].y = s.sprites[1].y;
            } catch (Exception genericException){
                Debug.Log("OH NO AN ERROR AAAAH");
                Debug.LogError("Photo will not Move it Move it, Photo will not Move it Move it, Photo will not Move it Move it, Photo will not: MOVE IT!");
                Debug.LogException(genericException);
            }
        }

    }
}