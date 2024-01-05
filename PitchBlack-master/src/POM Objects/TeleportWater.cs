using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RWCustom;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using static Pom.Pom;
using System.IO;
using AssetBundles;

namespace PitchBlack;

// References to world in Rain World's code seem to be regions.
// Could probably reuse this to make seemless gate transitions tbh. Might do that later :3
public class TeleportWater
{
    internal class TeleportWaterObject : CosmeticSprite
    {
        PlacedObject pObj;
        int soundTimer = 0;
        public TeleportWaterObject (PlacedObject pObj, Room room) {
            this.pObj = pObj;
            // room.PlayCustomSound("VS_SA_PULSE", Vector2.Lerp(pObj.pos, pObj.pos + (pObj.data as ManagedData).GetValue<Vector2>("Area"), 0.5f), 1, 1);
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[4]{new FSprite("Futile_White"), new FSprite("Futile_White"), new FSprite("Futile_White"), new FSprite("Futile_White")};
            sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["GhostDistortion"];
            sLeaser.sprites[1].shader = room.game.rainWorld.Shaders["GravityDisruptor"];
            sLeaser.sprites[2].shader = room.game.rainWorld.Shaders["CellDist"];
            sLeaser.sprites[3].shader = room.game.rainWorld.Shaders["LocalBloom"];
            AddToContainer(sLeaser, rCam, null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
		    rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[1]);
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[2]);
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[3]);
            sLeaser.sprites[2].MoveToBack();
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            var pos = pObj.pos - camPos;
            var area = (pObj.data as ManagedData).GetValue<Vector2>("Area");
            float r = (pObj.data as ManagedData).GetValue<float>("r");
            float g = (pObj.data as ManagedData).GetValue<float>("g");
            float b = (pObj.data as ManagedData).GetValue<float>("b");
            float a = (pObj.data as ManagedData).GetValue<float>("a");
            foreach (var sprite in sLeaser.sprites) {
                // Top left corner
                sprite._localVertices[0] = pos + Vector2.up*area.y + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[7]) * area.magnitude * 0.175f);
                // Top right corner
                sprite._localVertices[1] = pos + area + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[6]) * area.magnitude * 0.175f);
                // Bottom right corner
                sprite._localVertices[2] = pos + Vector2.right*area.x + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[5]) * area.magnitude * 0.175f);
                // Bottom left corner
                sprite._localVertices[3] = pos + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[4]) * area.magnitude * 0.175f);
                sprite.color = new Color(r, g, b, a);
            }
			if (slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
        }
        // This method would play sounds, but nothing works like I want it to.
        public void PlaySounds()
        {
            if (soundTimer > 0) {
                soundTimer--;
            }
            if (room != room.game.cameras[0].room) {
                return;
            }
            if (soundTimer == 0) {
                Debug.Log("Pitch Black Teleporter: I PLayed a sound!");
                soundTimer = 160;
                // room.PlayCustomSound("VS_SA_PULSE", Vector2.Lerp(pObj.pos, pObj.pos+(pObj.data as ManagedData).GetValue<Vector2>("Area"), 0.5f), 2, 1);
                string text = string.Concat(new string[]
                {
                    "music",
                    Path.DirectorySeparatorChar.ToString(),
                    "songs",
                    Path.DirectorySeparatorChar.ToString(),
                    "rw_42 - kayava",
                    ".ogg"
                });
                string trackName = AssetManager.ResolveFilePath(text);
		        SoundLoader.SoundData soundData = room.game.cameras[0].virtualMicrophone.GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
                soundData.dontAutoPlay = false;
                soundData.soundName = trackName;
                VirtualMicrophone.StaticPositionSound staticPositionSound = new (room.game.cameras[0].virtualMicrophone, soundData, Vector2.Lerp(pObj.pos, pObj.pos+(pObj.data as ManagedData).GetValue<Vector2>("Area"), 0.5f), 3, 1, false);
                Debug.Log(staticPositionSound.audioSource);
                staticPositionSound.singleUseSound = true;
                staticPositionSound.audioSource.loop = true;
                staticPositionSound.autoPlayAfterLoad = true;
                string discardText;
                LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle("music_songs", out discardText);
                Debug.Log($"Error message: {discardText}");
                staticPositionSound.audioSource.clip = loadedAssetBundle.m_AssetBundle.LoadAsset<AudioClip>("rw_42 - kayava.mp3");
                room.game.cameras[0].virtualMicrophone.soundObjects.Add(staticPositionSound);
                foreach (string name in loadedAssetBundle.m_AssetBundle.GetAllAssetNames())
                    Debug.Log(name);
                Debug.Log(loadedAssetBundle);
                Debug.Log(loadedAssetBundle.m_AssetBundle);
                Debug.Log(loadedAssetBundle.m_AssetBundle.LoadAsset<AudioClip>("rw_42 - kayava.mp3"));
            }
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            PlaySounds();
            // Get the object's data
            ManagedData managedData = (ManagedData)pObj.data;
            // If it is disabled, return and don't do any teleporty majiks
            if (!managedData.GetValue<bool>("Enabled")) {
                return;
            }
            // Get the object's parameter's and things to make it go vroom vroom
            Vector2 area = managedData.GetValue<Vector2>("Area");
            string roomName = managedData.GetValue<string>("DestinationRoom");
            Vector2 dest = pObj.pos+managedData.GetValue<Vector2>("DestinationPos");
            // Unity object moment (lmao RIP Cactus)
            Bounds rect = new Bounds(Vector2.Lerp(pObj.pos, pObj.pos+area, 0.5f), new Vector2(Mathf.Abs(area.x), Mathf.Abs(area.y)));
            foreach (Player player in room.PlayersInRoom) {
                if (rect.Contains(player.mainBodyChunk.pos)) {
                    // Get the data needed to move the player into the new room, and data to restore the state of the original player
                    AbstractCreature plrAbsCrt = player.abstractCreature;
                    World currentWorld = room.world;    // The current world to do transition stuff with.
                    RainWorldGame game = room.game;
                    Vector2 oldVel0 = player.bodyChunks[0].vel;
                    Vector2 oldVel1 = player.bodyChunks[1].vel;
                    Vector2 chunk1RelativePosition = player.bodyChunks[1].pos - player.bodyChunks[0].pos;
                    Player.AnimationIndex oldAnimation = player.animation;
                    Player.BodyModeIndex oldBodyMode = player.bodyMode;
                    Spear hasSpear = null;
                    AbstractPhysicalObject stomachObject = null;
                    
                    // Move the player into the new room
                    if (room.world.GetAbstractRoom(roomName) == null) { // If the current world cannot find a room with the destination room name, then it must be in a different region. This method returns null if it cannot find a room with the matching string name.
                        // Release the grasps, as holding objects while changing regions results in a crash. Will fix later.
                        foreach(var grasp in player.grasps) {
                            if (grasp != null) {
                                grasp.Release();
                            }
                        }
                        // Load the new region, based on the first part of the room name
                        // NOTE: THIS WILL BREAK WITH GATES NOT IN THE CURRENT REGION
                        game.overWorld.LoadWorld(Regex.Split(roomName, "_")[0], (game.session as StoryGameSession).saveStateNumber, false);
                        // Set the current world to the new world that just got loaded.
                        currentWorld = game.overWorld.activeWorld;
                        Debug.Log($"Pitch Black: Loaded new world: {currentWorld}, {currentWorld.name}");
                        // Set the player's world to the new region
                        plrAbsCrt.world = currentWorld;
                        // Set up a new RoomRealizer, so rooms in the new region get properly realized
                        if (game.roomRealizer != null)
                        {
                            game.roomRealizer = new RoomRealizer(game.roomRealizer.followCreature, currentWorld);
                        }
                    }
                    // Get the new abstract room from the destination world
                    AbstractRoom newRoom = currentWorld.GetAbstractRoom(roomName);
                    // Realize the new room.
                    newRoom.RealizeRoom(currentWorld, game);
                    // Set the abstract player's WorldCoordinate room index to be the new room's index.
                    // This is IMPORTANT! It stops the Move method from crashing when calling the ChangeRooms method.
                    // The crash specifically happens in the `World.GetAbstractRoom(int room)` method, since the room associated with the old index does not exist anymore.
                    plrAbsCrt.pos.room = newRoom.index;
                    // Prepare the backspear and stomachObject for teleportation.
                    if (plrAbsCrt.realizedCreature is Player plyr)
                    {
                        if (plyr.objectInStomach != null) {
                            plyr.objectInStomach.world = currentWorld;
                            stomachObject = plyr.objectInStomach;
                        }
                        if (plyr.spearOnBack?.spear != null) {
                            plyr.spearOnBack.spear.abstractSpear.world = currentWorld;
                            hasSpear = plyr.spearOnBack.spear;
                        }
                    }
                    //Transfer connected objects to new world/room
                    List<AbstractPhysicalObject> objs = plrAbsCrt.GetAllConnectedObjects();
                    for (int i = 0; i < objs.Count; i++)
                    {
                        objs[i].world = currentWorld;
                        objs[i].pos = plrAbsCrt.pos;
                        room.abstractRoom.RemoveEntity(objs[i]);
                        newRoom.AddEntity(objs[i]);
                        objs[i].realizedObject.sticksRespawned = true;
                    }
                    // Remove the player from their start room
                    room.RemoveObject(player);
                    // Move the player to the new room (no idea how important the x, y, and abstractNode args are)
                    plrAbsCrt.Move(new WorldCoordinate(newRoom.index, (int)dest.x, (int)dest.y, -1));
                    // Realize the player in the new room.
                    plrAbsCrt.RealizeInRoom();
                    // A reference to the new player, in case the old Player object is destroyed.
                    Player newPlayer = (Player)plrAbsCrt.realizedCreature;
                    // Set their realized position in the room to the destination coordinates of the devtool object
                    newPlayer.SuperHardSetPosition(dest);
                    // Only do this stuff if Jolly is disabled, as Jolly seems to be able to handle moving the camera by itself (which makes sense, as players can move the camera between rooms by themselves).
                    if (!ModManager.JollyCoop) {
                        // Quite the camera's microphone, I think that stops all sound being played.
                        game.cameras[0].virtualMicrophone.AllQuiet();
                        for (int i = 0; i < game.cameras[0].hud.fContainers.Length; i++)
                        {
                            game.cameras[0].hud.fContainers[i].RemoveAllChildren();
                        }
                        game.cameras[0].hud = null;
                        // Move camera to the new room.
                        game.cameras[0].MoveCamera(newRoom.realizedRoom, 0);
                        game.cameras[0].FireUpSinglePlayerHUD(game.AlivePlayers[0].realizedCreature as Player);
                        // newRoom.world.game.roomRealizer.followCreature = plrAbsCrt;
                    }
                    for (int i = 0; i < game.cameras.Length; i++)
                    {
                        // Reset the hud map, which makes it sync to the new world the player is in if they moved between regions.
                        game.cameras[0].hud.ResetMap(new HUD.Map.MapData(currentWorld, game.rainWorld));
                        // if (game.cameras[i].hud.textPrompt.subregionTracker != null)
                        // {
                        //     game.cameras[i].hud.textPrompt.subregionTracker.lastShownRegion = 0;
                        // }
                    }
                    // Move camera's microphone to the new room?
                    game.cameras[0].virtualMicrophone.NewRoom(game.cameras[0].room);
                    // Reset the player's graphicsModule. I found that without doing that their sprites would get messed up, and many would not show.
                    newPlayer.graphicsModule.Reset();

                    // Restore old values.
                    for (int i = 0; i < objs.Count; i++)
                    {
                        int num = 0;
                        for (int s = 0; s < newRoom.realizedRoom.updateList.Count; s++)
                        {
                            if (objs[i].realizedObject == newRoom.realizedRoom.updateList[s])
                            {
                                num++;
                            }
                            if (num > 1)
                            {
                                newRoom.realizedRoom.updateList.RemoveAt(s);
                            }
                        }
                    }
                    //Re-add any backspears
                    if (hasSpear != null && newPlayer.spearOnBack?.spear != hasSpear)
                    {
                        newPlayer.spearOnBack.SpearToBack(hasSpear);
                        newPlayer.abstractPhysicalObject.stuckObjects.Add(newPlayer.spearOnBack.abstractStick);
                    }
                    //Re-add any stomach objects
                    if (stomachObject != null && newPlayer.objectInStomach == null)
                    {
                        newPlayer.objectInStomach = stomachObject;
                    }
                    // Re-set the velocities and position of bodychunks
                    newPlayer.bodyChunks[0].vel = oldVel0;
                    newPlayer.bodyChunks[1].vel = oldVel1;
                    newPlayer.bodyChunks[1].pos = newPlayer.bodyChunks[0].pos + chunk1RelativePosition;
                    // Re-set the animation and bodymode
                    newPlayer.animation = oldAnimation;
                    newPlayer.bodyMode = oldBodyMode;
                    Debug.Log($"Pitch Black Teleported Player to: {roomName}, at coords {plrAbsCrt.realizedCreature.mainBodyChunk.pos}.\nCoors should have been: {dest}");
                    Debug.Log($"Pitch Black: original player? {player}, {player==newPlayer}");
                }
            }
        }
    }
    internal static void Register() {
        List<ManagedField> fields = new List<ManagedField> {
            new Vector2Field("Area", Vector2.one, Vector2Field.VectorReprType.rect, "Area"),
            new StringField("DestinationRoom", "", "Destination Room"),
            new Vector2Field("DestinationPos", Vector2.zero, Vector2Field.VectorReprType.line, "Position"),
            new BooleanField("Enabled", false, ManagedFieldWithPanel.ControlType.arrows, "Enabled"),
            new FloatField("r", 0, 100, 3.7f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "R"),
            new FloatField("g", 0, 100, 0f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "G"),
            new FloatField("b", 0, 100, 1f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "B"),
            new FloatField("a", 0, 100, 0.9f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "A")
		};
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(TeleportWaterObject), "TeleportWater", "Pitch-Black");
    }
}