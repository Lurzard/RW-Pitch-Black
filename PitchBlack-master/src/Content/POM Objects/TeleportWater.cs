using System.Collections.Generic;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;
using static Pom.Pom;
using System;
using System.Linq;

namespace PitchBlack;

public class RiftWorldPresence
{
    public string roomName;
    public List<AbstractCreature> abstractCreatures;
    public string id;
    public RiftWorldPresence(string roomName, string id, List<AbstractCreature> abstractCreatures) {
        this.roomName = roomName;
        this.id = id;
        this.abstractCreatures = abstractCreatures;
    }
}

// References to world in Rain World's code seem to be regions.
// Could probably reuse this to make seemless gate transitions tbh. Might do that later :3
// Oh wait Vigaro apparently did that already... :(
public class TeleportWater
{
    private enum AssetBundleName
    {
        music_procedural,
        music_songs,
        loadedsoundeffects,
        loadedsoundeffects_ambient
    }
    internal class TeleportWaterObject : CosmeticSprite
    {
        PlacedObject pObj;
        bool startedNoise = false;
        Vector2 closestPlayerPos;
        List<AbstractCreature> abstractCreatures;
        public TeleportWaterObject (PlacedObject pObj, Room room) : base() {
            this.pObj = pObj;
            this.room = room;
            closestPlayerPos = Vector2.positiveInfinity;
            // Debug.Log($"Pitch Black: {nameof(TeleportWaterObject)} room: {room}, game: {room?.game}");
            if (Plugin.riftCWT.TryGetValue(room.game, out List<RiftWorldPresence> riftWorldPrecences)) {
                // Debug.Log("Pitch black: there is a riftCWT");
                foreach (RiftWorldPresence riftWorldPrecence in riftWorldPrecences) {
                    foreach(AbstractCreature absCrit in riftWorldPrecence.abstractCreatures) {
                        Debug.Log($"Pitch Black: {absCrit.creatureTemplate.type}");
                    }
                    if (riftWorldPrecence.roomName == room.abstractRoom.name && riftWorldPrecence.id == (pObj.data as ManagedData).GetValue<string>("id")) {
                        this.abstractCreatures = riftWorldPrecence.abstractCreatures;
                        return;
                    }
                }
                // If it does not find a match, create a new Precence
                Debug.Log($"Pitch Black: Created new {nameof(RiftWorldPresence)}");
                riftWorldPrecences.Add(new RiftWorldPresence(room.abstractRoom.name, (pObj.data as ManagedData).GetValue<string>("id"), new List<AbstractCreature>()));
            }
            else {
                abstractCreatures = new List<AbstractCreature>();
            }
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            sLeaser.sprites = new FSprite[5]{new FSprite("Futile_White"), new FSprite("Futile_White"), new FSprite("Futile_White"), new FSprite("Futile_White"), new FSprite("Futile_White")};
            sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["GhostDistortion"];
            sLeaser.sprites[1].shader = room.game.rainWorld.Shaders["GravityDisruptor"];
            sLeaser.sprites[2].shader = room.game.rainWorld.Shaders["CellDist"];
            sLeaser.sprites[3].shader = room.game.rainWorld.Shaders["LocalBloom"];
            sLeaser.sprites[4].shader = room.game.rainWorld.Shaders["RoomTransition"];
            AddToContainer(sLeaser, rCam, null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[0]);
		    rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[1]);
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[2]);
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[3]);
            rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[4]);
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
            for (int i = 0; i < sLeaser.sprites.Length-1; i++) {
                // Top left corner
                sLeaser.sprites[i]._localVertices[0] = pos + Vector2.up*area.y + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[7]) * area.magnitude * 0.175f);
                // Top right corner
                sLeaser.sprites[i]._localVertices[1] = pos + area + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[6]) * area.magnitude * 0.175f);
                // Bottom right corner
                sLeaser.sprites[i]._localVertices[2] = pos + Vector2.right*area.x + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[5]) * area.magnitude * 0.175f);
                // Bottom left corner
                sLeaser.sprites[i]._localVertices[3] = pos + (Custom.IntVector2ToVector2(Custom.eightDirectionsDiagonalsLast[4]) * area.magnitude * 0.175f);
                sLeaser.sprites[i].color = new Color(r, g, b, a);
            }
            Vector2 objCenter = Vector2.Lerp(pos, pos+area, 0.5f);
            sLeaser.sprites[sLeaser.sprites.Length-1].SetPosition(objCenter);
            sLeaser.sprites[sLeaser.sprites.Length-1].scale = 400f;
            sLeaser.sprites[sLeaser.sprites.Length-1].color = new Color(0.5f, 0.5f, Mathf.Lerp(0, 1, (1+(pObj.data as ManagedData).GetValue<float>("fadeStartDist"))-(0.0025f*Vector2.Distance(objCenter, closestPlayerPos-camPos))), 0.99f);
			if (slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
        }
        // This method would play sounds, but nothing works like I want it to.
        public void PlaySounds(ManagedData managedData)
        {
            if (room != room.game.cameras[0].room || startedNoise) {
                return;
            }
            #nullable enable
            AssetBundle? loadedAssetBundle = AssetBundle.GetAllLoadedAssetBundles().First(x => x.name == managedData.GetValue<AssetBundleName>("bundleName").ToString());
            AudioClip? audio = loadedAssetBundle?.LoadAsset<AudioClip>(managedData.GetValue<string>("songName"));
            if (audio == null) {
                Debug.LogError($"Pitch Black {nameof(TeleportWater)}: Could not find an asset of name {managedData.GetValue<string>("songName")} in asset bundle {managedData.GetValue<AssetBundleName>("bundleName")}");
                return;
            }
            startedNoise = true;
            SoundLoader.SoundData soundData = room.game.cameras[0].virtualMicrophone.GetSoundData(SoundID.Slugcat_Stash_Spear_On_Back, -1);
            VirtualMicrophone.StaticPositionSound staticPositionSound = new (room.game.cameras[0].virtualMicrophone, soundData, Vector2.Lerp(pObj.pos, pObj.pos+managedData.GetValue<Vector2>("Area"), 0.5f), managedData.GetValue<float>("volume"), managedData.GetValue<float>("pitch"), false);
            staticPositionSound.audioSource.clip = audio;
            staticPositionSound.audioSource.loop = true;
            staticPositionSound.audioSource.dopplerLevel = managedData.GetValue<float>("doppler");
            room.game.cameras[0].virtualMicrophone.soundObjects.Add(staticPositionSound);
            room.game.manager.musicPlayer.FadeOutAllSongs(40);
            staticPositionSound.Play();
            #nullable disable
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            // Get the object's data
            ManagedData managedData = (ManagedData)pObj.data;
            // Play sound
            PlaySounds(managedData);
            if (room.PlayersInRoom == null || room.PlayersInRoom.Count == 0) {
                closestPlayerPos = Vector2.positiveInfinity;
            }
            // If it is disabled, return and don't do any teleporty majiks
            if (!managedData.GetValue<bool>("Enabled") || room == null || room.PlayersInRoom == null) {
                return;
            }
            // Get the object's parameter's and things to make it go vroom vroom
            Vector2 area = managedData.GetValue<Vector2>("Area");
            string roomName = managedData.GetValue<string>("DestinationRoom");
            Vector2 dest = pObj.pos+new Vector2(managedData.GetValue<float>("DestinationPosX"), managedData.GetValue<float>("DestinationPosY"));
            Vector2 objCenter = Vector2.Lerp(pObj.pos, pObj.pos+area, 0.5f);
            // Unity object moment (lmao RIP Cactus)
            Bounds rect = new Bounds(objCenter, new Vector2(Mathf.Abs(area.x), Mathf.Abs(area.y)));
            closestPlayerPos = Vector2.positiveInfinity;
            try {
                foreach (Player player in room.PlayersInRoom) {
                    if (player?.mainBodyChunk == null) {
                        continue;
                    }
                    if (Vector2.Distance(objCenter, player.mainBodyChunk.pos) < Vector2.Distance(objCenter, closestPlayerPos)) {
                        closestPlayerPos = player.mainBodyChunk.pos;
                        // Debug.Log(Vector2.Distance(closestPlayerPos, objCenter));
                    }
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
                            game.overWorld.LoadWorld(Regex.Split(roomName, "_")[0], (game.session as StoryGameSession).saveStateNumber, SlugcatStats.SlugcatToTimeline((game.session as StoryGameSession).saveStateNumber), false); //gona leave this one for ya Moon -Lur
                            // Set the current world to the new world that just got loaded.
                            currentWorld = game.overWorld.activeWorld;
                            Debug.Log($"Pitch Black: Loaded new world: {currentWorld}, {currentWorld.name}");
                            // Set the player's world to the new region
                            plrAbsCrt.world = currentWorld;
                            // Set up a new RoomRealizer, so rooms in the new region get properly realized
                            if (game.roomRealizer != null)
                            {
                                Debug.Log("Pitch Black: Reset RoomRealizer");
                                game.roomRealizer = new RoomRealizer(game.roomRealizer.followCreature, currentWorld);
                            }
                        }
                        Debug.Log("Pitch Black: Getting new abstract room");
                        // Get the new abstract room from the destination world
                        AbstractRoom newRoom = null;
                        try {newRoom = currentWorld.GetAbstractRoom(roomName);} catch (Exception err) {
                                    Debug.Log("Oh no, there was an error!\n" + err);
                                }
                        // Add all the creatures to be teleported to the new room, so that they can be realized correctly.
                        foreach (AbstractCreature absCrit in abstractCreatures) {
                            Debug.Log($"Pitch Black: Adding {absCrit.creatureTemplate.type} to the room");
                            absCrit.world = currentWorld;
                            absCrit.pos.abstractNode = 0;
                            absCrit.abstractAI.world = currentWorld;
                            // absCrit.abstractAI.RealAI.pathFinder.world = currentWorld;
                            absCrit.pos.room = newRoom.index;
                            newRoom.AddEntity(absCrit);
                        }
                        Debug.Log("Pitch Black: Realizing the new room");
                        // Realize the new room.
                        newRoom.RealizeRoom(currentWorld, game);
                        // Clear the list, remove references.
                        abstractCreatures.Clear();
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
                            Debug.Log($"Pitch Black: Transfering entity {i} of {objs.Count}, which is a {objs[i].type}");
                            objs[i].world = currentWorld;
                            objs[i].pos = plrAbsCrt.pos;
                            room.abstractRoom.RemoveEntity(objs[i]);
                            newRoom.AddEntity(objs[i]);
                            objs[i].realizedObject.sticksRespawned = true;
                        }
                        Debug.Log("Pitch Black: Sending player to their new room");
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
                        Debug.Log("Pitch Black: Moving camera");
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
                            Debug.Log($"Pitch Black: Reseting camera {i}'s map");
                            // Reset the hud map, which makes it sync to the new world the player is in if they moved between regions.
                            game.cameras[0].hud.ResetMap(new HUD.Map.MapData(currentWorld, game.rainWorld));
                            if (game.cameras[i].hud.textPrompt.subregionTracker != null)
                            {
                                game.cameras[i].hud.textPrompt.subregionTracker.lastShownRegion = 0;
                            }
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
                            Debug.Log("Pitch Black: Re-attach back-spear");
                            newPlayer.spearOnBack.SpearToBack(hasSpear);
                            newPlayer.abstractPhysicalObject.stuckObjects.Add(newPlayer.spearOnBack.abstractStick);
                        }
                        //Re-add any stomach objects
                        if (stomachObject != null && newPlayer.objectInStomach == null)
                        {
                            Debug.Log("Pitch Black: Adding stomach object back");
                            newPlayer.objectInStomach = stomachObject;
                        }
                        Debug.Log("Pitch Black: Restoring velocity and animation");
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
            } catch (Exception err) {
                Debug.Log("WOWIE another ERRRROOOORRRR");
                Debug.Log(err);
                //Debug.LogError(err); Debug errors
                Plugin.logger.LogDebug(err);
            }
            // If a creature wanders into the portal, queue it for teleportation.
            try {
                foreach (AbstractCreature abstractCreature in room.abstractRoom.creatures) {
                    // Debug.Log($"Pitch Black: {abstractCreatures}");
                    if (abstractCreature.realizedCreature is Creature crit && crit is not Player && rect.Contains(crit.mainBodyChunk.pos) && !abstractCreatures.Contains(abstractCreature)) {
                        Debug.Log($"Pitch Black: New abstract creature added to list {abstractCreature.creatureTemplate.type}");
                        abstractCreatures.Add(abstractCreature);
                        abstractCreature.Abstractize(abstractCreature.pos);
                        crit.Destroy();
                    }
                }
            } catch (Exception err) {
                Debug.Log("Oh nooooo its the error again");
                //Debug.LogError(err); Debug errors
                Plugin.logger.LogDebug(err);
            }
        }
    }
    internal static void Register() {
        List<ManagedField> fields = new List<ManagedField> {
            new Vector2Field("Area", Vector2.one, Vector2Field.VectorReprType.rect, "Area"),
            new StringField("DestinationRoom", "", "Destination Room"),
            new FloatField("DestinationPosX", int.MinValue, int.MaxValue, 0, 0.01f, ManagedFieldWithPanel.ControlType.text, "DestinationX"),
            new FloatField("DestinationPosY", int.MinValue, int.MaxValue, 0, 0.01f, ManagedFieldWithPanel.ControlType.text, "DestinationY"),
            new BooleanField("Enabled", false, ManagedFieldWithPanel.ControlType.arrows, "Enabled"),
            new FloatField("r", 0, 100, 3.7f, 0.1f, ManagedFieldWithPanel.ControlType.text, "R"),
            new FloatField("g", 0, 100, 0f, 0.1f, ManagedFieldWithPanel.ControlType.text, "G"),
            new FloatField("b", 0, 100, 1f, 0.1f, ManagedFieldWithPanel.ControlType.text, "B"),
            new FloatField("a", 0, 100, 0.9f, 0.1f, ManagedFieldWithPanel.ControlType.text, "A"),
            new FloatField("volume", 0, 10, 1, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Volume"),
            new FloatField("pitch", -10, 10, 1, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Pitch"),
            new FloatField("doppler", 0, 1, 0.5f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Doppler"),
            new StringField("songName", "vs_sa_pulse", "Song Name"),
            new EnumField<AssetBundleName>("bundleName", AssetBundleName.music_procedural, null, ManagedFieldWithPanel.ControlType.arrows, "Asset Bundle"),
            new FloatField("fadeStartDist", -5, 5, 0f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Fade Start Distance"),
            new StringField("id", "0", "ID")
		};
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(TeleportWaterObject), "TeleportWater", "Pitch-Black");
    }
}