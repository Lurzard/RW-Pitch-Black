using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using System.Runtime.CompilerServices;
using System;

namespace PitchBlack;

public static class Extension
{
    public static readonly ConditionalWeakTable<Player, BeaconCWT> _cwtbc = new();

    public static BeaconCWT Beacon(this Player player) => _cwtbc.GetValue(player, _ => new BeaconCWT(player));

    public static bool IsBeacon(this Player player) => player.Beacon().IsBeacon;

    public static bool IsBeacon (this Player player, out BeaconCWT beacon)
    {
        beacon = player.Beacon();
        return beacon.IsBeacon;
    }
}

public class BeaconCWT
{
    public Color BeaconColor;

    public WeakReference<Player> PlayerRef;

    public readonly bool IsBeacon;

    public ScugCWT scugCWTData; //for if you need to get any variables from ScugCWT while accessing BeaconCWT
    public FlareStore storage;
    public int dontThrowTimer = 0;
    public bool heldCraft = false;
    public int brightSquint = 0;
    public Vector2 eyePos = new Vector2(0, 0);
    public BeaconCWT(ScugCWT cwtData)
    {
        scugCWTData = cwtData;
        cwtData.playerRef.TryGetTarget(out Player player);
        storage = new FlareStore(player);
    }

    public BeaconCWT(Player player)
    {
        PlayerRef = new WeakReference<Player>(player);

        IsBeacon = player.slugcatStats.name == Plugin.BeaconName;

        if (!IsBeacon)
        {
            return;
        }
    }

    public class AbstractStoredFlare : AbstractPhysicalObject.AbstractObjectStick
    {
        public AbstractPhysicalObject Player
        {
            get
            {
                return A;
            }
            set
            {
                A = value;
            }
        }

        public AbstractPhysicalObject FlareBomb
        {
            get
            {
                return B;
            }
            set
            {
                B = value;
            }
        }

        public AbstractStoredFlare(AbstractPhysicalObject player, AbstractPhysicalObject bomb) : base(player, bomb) { }
    }

    public class FlareStore
    {
        public Player ownr;
        public Stack<FlareBomb> storedFlares;
        public bool increment;
        public int counter;

        // Change this to increase the number of flares stored
        public int capacity = PBOptions.maxFlashStore.Value;
        public bool interactionLocked;
        public Stack<AbstractStoredFlare> abstractFlare;

        public FlareStore(Player owner)
        {
            if (storedFlares == null)
            {
                storedFlares = new Stack<FlareBomb>(capacity);
                abstractFlare = new Stack<AbstractStoredFlare>(capacity);
            }
            ownr = owner;
            // Debug.log("Flare storage initiated!");
        }

        public void Update(bool eu)
        {
            if (increment)
            {
                counter++;
                if (counter > 20 && storedFlares.Count < capacity)
                {
                    // Move flare from any hand to store if store is empty
                    //WW- WHY ONLY MAIN HAND IF STORAGE IS NOT FULL? SEEMS LIKE THIS SHOULD WORK FROM ANY HAND
                    for (int i = 0; i < 2; i++)
                    {
                        if (ownr.grasps[i] != null && ownr.grasps[i].grabbed is FlareBomb f)
                        {
                            FlarebombtoStorage(f);
                            counter = 0;
                            break;
                        }
                    }
                }
                if (counter > 20 && storedFlares.Count > 0)
                {
                    // Move flare from store to paw
                    FlarebombFromStorageToPaw(eu);
                    counter = 0;
                }
            }
            else
            {
                counter = 0;
            }
            if (!ownr.input[0].pckp)
            {
                interactionLocked = false;
            }
            increment = false;
        }

        public void GraphicsModuleUpdated(bool eu)
        {
            // Skip drawing if storage is empty
            if (storedFlares.Count <= 0)
                return;

            PlayerGraphics pG = ownr.graphicsModule as PlayerGraphics;

            if (pG == null) return;


            for (int i = 0; i < storedFlares.Count; i++)
            {
                float necklaceLength = 2; //capacity / 2; //WW- Didn't work well for numbers past 4, changing it.
                // These may be able to be replaced with math involving bodyChunks of the player, which while may be more intuitive to understand, could come with positioning issues.
                Vector2 drawPointLeft = pG.drawPositions[0, 0];
                Vector2 drawPointRight = pG.drawPositions[1, 0];
                // n is the angle created by going from the left draw point to the right draw point, based on a horizontal line as 0 degrees
                float n = Custom.VecToDeg((drawPointLeft - drawPointRight).normalized);
                // These vectors are the limits on the linear position displacement of flarebombs in between them
                Vector2 flarePositionStart = new(-30f, -8f);
                Vector2 flarePositionEnd = new(30f, -8f);
                if (i >= necklaceLength) {
                    flarePositionStart = new Vector2(-8f, -8f);
                    flarePositionEnd = new Vector2(8f, -8f);
                }
                if (ownr.bodyMode == Player.BodyModeIndex.Crawl) {
                    flarePositionStart.y += 3.25f;
                    flarePositionEnd.y += 3.25f;
                }

                // The same as the vectors previously defined, but rotated around with the player's rotation.
                Vector2 vector = drawPointLeft + Custom.RotateAroundOrigo(flarePositionStart, n);
                Vector2 vector1 = drawPointLeft + Custom.RotateAroundOrigo(flarePositionEnd, n);

                // num is a fraction, that essentially determines at what point the flare is in between the flare position caps.
                
                float fractionOfDistance = (i + 1f) / (Mathf.Min(storedFlares.Count, necklaceLength) + 1f);
                if (i >= necklaceLength) {
                    fractionOfDistance = (i - necklaceLength + 1f) / (storedFlares.Count - necklaceLength + 1f);
                }
                Vector2 calculatedDestination = vector + (vector1 - vector) * fractionOfDistance;
                
                storedFlares.ToArray()[i].firstChunk.MoveFromOutsideMyUpdate(eu, calculatedDestination);
                storedFlares.ToArray()[i].firstChunk.vel = Vector2.zero;
                storedFlares.ToArray()[i].rotationSpeed = 0f;
            }
        }

        public int FlarebombFromStorageToPaw(bool eu)
        {
            //spinch: the int return is to find which grasp index the flarebomb is now in

            // See if it's possible to add weapon
            for (int i = 0; i < 2; i++)
            {
                if (ownr.grasps[i] != null && ownr.Grabability(ownr.grasps[i].grabbed) >= Player.ObjectGrabability.TwoHands)
                {
                    return -1;
                }
            }

            int toPaw = ownr.FreeHand();
            // If empty hand has been detected
            if (toPaw != -1)
            {
                FlareBomb fb = storedFlares.Pop();
                AbstractStoredFlare af = abstractFlare.Pop();
                if (ownr.graphicsModule != null)
                {
                    fb.firstChunk.MoveFromOutsideMyUpdate(eu, (ownr.graphicsModule as PlayerGraphics).hands[toPaw].pos);
                }
                
                af?.Deactivate();

                fb.CollideWithObjects = true;
                fb.CollideWithTerrain = true;
                fb.collisionRange = 50f;
                fb.ChangeMode(Weapon.Mode.Free);
                ownr.SlugcatGrab(fb, toPaw);
                interactionLocked = true;
                ownr.noPickUpOnRelease = 20;
                ownr.room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, ownr.mainBodyChunk);
                // Debug.log("Successfully applied flare to paw! Storage index is now: " + storedFlares.Count);

                return toPaw;
            }
            else
            {
                Debug.Log("Pitch Black: Couldn't add flare to paw! Index is now: " + storedFlares.Count);
                return -1;
            }

        }

        public void FlarebombtoStorage(FlareBomb f)
        {
            // Take off the flare from hand
            for (int i = 0; i < 2; i++)
            {
                if (ownr.grasps[i] != null && ownr.grasps[i].grabbed == f)
                {
                    ownr.ReleaseGrasp(i);
                    break;
                }
            }
            f.ChangeMode(Weapon.Mode.OnBack);
            f.CollideWithObjects = false;
            f.CollideWithTerrain = false;
            f.collisionRange = 0f;
            storedFlares.Push(f);
            interactionLocked = true;
            ownr.noPickUpOnRelease = 20;
            ownr.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, ownr.mainBodyChunk);
            abstractFlare.Push(new AbstractStoredFlare(ownr.abstractPhysicalObject, f.abstractPhysicalObject));
            // Debug.log("Applied flare into storage! Storage index is now: " + storedFlares.Count);
        }
    }
}