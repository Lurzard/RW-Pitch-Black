using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PitchBlack
{
    public class Beacon
    {
        public FlareStore storage;


        public class AbstractStoredFlare: AbstractPhysicalObject.AbstractObjectStick{
            public AbstractPhysicalObject Player{
                get{
                    return A;
                }
                set{
                    A = value;
                }
            }
            
            public AbstractPhysicalObject FlareBomb{
                get{
                    return B;
                }
                set{
                    B = value;
                }
            }

            public AbstractStoredFlare(AbstractPhysicalObject player, AbstractPhysicalObject bomb) : base(player, bomb){
            }
        }

        public class FlareStore{
            public Player ownr;
            public Stack<FlareBomb> storedFlares;
            public bool increment;
            public int counter;

            // Change this to increase the number of flares stored
            public int capacity = 4;
            public bool interactionLocked;
            public Stack<AbstractStoredFlare> abstractFlare;

            public FlareStore(Player owner){
                if (storedFlares == null){
                    this.storedFlares = new Stack<FlareBomb>(capacity);
                    this.abstractFlare = new Stack<AbstractStoredFlare>(capacity);
                }
                this.ownr = owner;
                Debug.Log("Flare storage initiated!");
            }

            public void Update(bool eu){
                if (increment){
                    counter++;
                    if (counter > 20 && storedFlares.Count < capacity){
                        if (storedFlares.Count == 0){
                            // Move flare from any hand to store if store is empty
                            for (int i = 0; i < 2; i++){
                                if (ownr.grasps[i] != null && ownr.grasps[i].grabbed is FlareBomb f){
                                    FtoStore(f);
                                    counter = 0;
                                    break;
                                }
                            }
                        }
                        else if (ownr.grasps[0] != null && ownr.grasps[0].grabbed is FlareBomb f) {
                            // Move flare from main paw to store
                            FtoStore(f);
                            counter = 0;
                        }
                    }
                    if (counter > 20 && storedFlares.Count > 0){
                        // Move flare from store to paw
                        FtoPaw(eu);
                        counter = 0;
                    } 

                }
                else{
                    counter = 0;
                }
                if (!ownr.input[0].pckp){
                    interactionLocked = false;
                }
                increment = false;
            }

            public void GraphicsModuleUpdated(bool actuallyViewed, bool eu){
                // Skip drawing if storage is empty
                if (storedFlares.Count > 0){
                    PlayerGraphics pG = (PlayerGraphics)ownr.graphicsModule;
                    Vector2 v1 = pG.drawPositions[0, 0];
                    Vector2 v2 = pG.drawPositions[1, 0];
                    Vector2 va = new Vector2(12f, -8f);
                    Vector2 vb = new Vector2(-12f, -8f);
                    Vector2 n = (v1 - v2).normalized;
                    va = v1 + Custom.RotateAroundOrigo(va, Custom.VecToDeg(n));
                    vb = v1 + Custom.RotateAroundOrigo(vb, Custom.VecToDeg(n));
                    int i = 0;
                    foreach (FlareBomb fb in storedFlares){
                        float num = ((float)i + 1f) / ((float)storedFlares.Count + 1f);
                        Vector2 destination = va + (vb - va) * num;
                        fb.firstChunk.MoveFromOutsideMyUpdate(eu, destination);
                        fb.firstChunk.vel = ownr.mainBodyChunk.vel;
                        fb.rotationSpeed = 0f;
                        i++;
                    }
                }
            }

            public void FtoPaw(bool eu){
                // See if it's possible to add weapon
                for (int i = 0; i < 2; i++){
                    if (ownr.grasps[i] != null && ownr.Grabability(ownr.grasps[i].grabbed) >= Player.ObjectGrabability.TwoHands){
                        return;
                    }
                }
                // Find empty hand
                int toPaw = -1;
                for (int j = 0; j < 2; j++){
                    if (toPaw != -1){
                        break;
                    }
                    if (ownr.grasps[j] == null){
                        toPaw = j;
                    }
                }
                // If empty hand has been detected
                if (toPaw != -1){
                    FlareBomb fb = storedFlares.Pop();
                    AbstractStoredFlare af = abstractFlare.Pop();
                    if (ownr.graphicsModule != null){
                        fb.firstChunk.MoveFromOutsideMyUpdate(eu, (ownr.graphicsModule as PlayerGraphics).hands[toPaw].pos);
                    }
                    if (af != null){
                        af.Deactivate();
                    }
                    fb.CollideWithObjects = true;
                    ownr.SlugcatGrab(fb, toPaw);
                    interactionLocked = true;
                    ownr.noPickUpOnRelease = 20;
                    ownr.room.PlaySound(SoundID.Slugcat_Pick_Up_Flare_Bomb, ownr.mainBodyChunk);
                    Debug.Log("Successfully applied flare to paw! Storage index is now: " + storedFlares.Count);
                }
                else{
                    Debug.Log("Couldn't add flare to paw! Index is now: " + storedFlares.Count);
                }

            }

            public void FtoStore(FlareBomb f){
                // Take off the flare from hand
                for (int i = 0; i < 2; i++){
                    if (ownr.grasps[i] != null && ownr.grasps[i].grabbed == f){
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


        public Beacon(Player player)
        {
            this.storage = new FlareStore(player);
        }
    }
}
