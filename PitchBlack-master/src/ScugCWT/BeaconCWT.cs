using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace PitchBlack
{
    public class BeaconCWT
    {
        public ScugCWT scugCWTData; //for if you need to get any variables from ScugCWT while accessing BeaconCWT
        public FlareStore storage;
        public BeaconCWT(ScugCWT cwtData)
        {
            scugCWTData = cwtData;
            cwtData.playerRef.TryGetTarget(out Player player);
            storage = new FlareStore(player);
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
            public int capacity = 4;
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
                Debug.Log("Flare storage initiated!");
            }

            public void Update(bool eu)
            {
                if (increment)
                {
                    counter++;
                    if (counter > 20 && storedFlares.Count < capacity)
                    {
                        if (storedFlares.Count == 0)
                        {
                            // Move flare from any hand to store if store is empty
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
                        else if (ownr.grasps[0]?.grabbed is FlareBomb f)
                        {
                            // Move flare from main paw to store
                            FlarebombtoStorage(f);
                            counter = 0;
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

                Vector2 v1 = pG.drawPositions[0, 0];
                Vector2 v2 = pG.drawPositions[1, 0];
                float n = Custom.VecToDeg((v1 - v2).normalized);
                Vector2 va = v1 + Custom.RotateAroundOrigo(new(12f, -8f), n);
                Vector2 vb = v1 + Custom.RotateAroundOrigo(new(-12f, -8f), n);

                int i = 0;
                foreach (FlareBomb fb in storedFlares)
                {
                    float num = (i + 1f) / (storedFlares.Count + 1f);
                    Vector2 destination = va + (vb - va) * num;
                    
                    fb.firstChunk.MoveFromOutsideMyUpdate(eu, destination);
                    fb.firstChunk.vel = ownr.mainBodyChunk.vel;
                    fb.rotationSpeed = 0f;
                    i++;
                }

#if false
//doesnt work lol
                //spinch: below is correcting the pos of the middle flarebombs
                //only needs to be corrected if theres more than 2 flarebombs
                if (storedFlares.Count <= 2) return;

                var flareArr = storedFlares.ToArray();

                Vector2 leftEndOfCollarPos = flareArr[0].firstChunk.pos;
                Vector2 rightEndOfCollarPos = flareArr[storedFlares.Count - 2].firstChunk.pos;
                Vector2 dirVectorNormalized = (leftEndOfCollarPos - rightEndOfCollarPos).normalized;

                float padding = leftEndOfCollarPos.x - rightEndOfCollarPos.x;
                if (storedFlares.Count >= 4)
                    padding = Mathf.Max(padding - 1f, 0);

                i = 1;
                foreach (FlareBomb fb in storedFlares)
                {
                    if (i == 0) continue;

                    float num = (i + 1f) / (storedFlares.Count + 1f);
                    Vector2 destination = va + (vb - va) * num + padding * i * dirVectorNormalized;

                    fb.firstChunk.MoveFromOutsideMyUpdate(eu, destination);
                    fb.firstChunk.vel = ownr.mainBodyChunk.vel;
                    fb.rotationSpeed = 0f;
                    i++;
                }
#endif
            }

            public void FlarebombFromStorageToPaw(bool eu)
            {
                // See if it's possible to add weapon
                for (int i = 0; i < 2; i++)
                {
                    if (ownr.grasps[i] != null && ownr.Grabability(ownr.grasps[i].grabbed) >= Player.ObjectGrabability.TwoHands)
                    {
                        return;
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
                    ownr.SlugcatGrab(fb, toPaw);
                    interactionLocked = true;
                    ownr.noPickUpOnRelease = 20;
                    ownr.room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, ownr.mainBodyChunk);
                    Debug.Log("Successfully applied flare to paw! Storage index is now: " + storedFlares.Count);
                }
                else
                {
                    Debug.Log("Couldn't add flare to paw! Index is now: " + storedFlares.Count);
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
                storedFlares.Push(f);
                interactionLocked = true;
                ownr.noPickUpOnRelease = 20;
                ownr.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, ownr.mainBodyChunk);
                abstractFlare.Push(new AbstractStoredFlare(ownr.abstractPhysicalObject, f.abstractPhysicalObject));
                Debug.Log("Applied flare into storage! Storage index is now: " + storedFlares.Count);
            }
        }
    }
}
