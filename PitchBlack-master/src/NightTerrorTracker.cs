using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using PitchBlack;

namespace PitchBlack
{
    public static class NightTerrorTrackers 
    {

        public class NightTerrorTracker
        {
            public AbstractCreature ac;
            public WorldCoordinate previousRoom;
            public string currentRoom;
            public World world;
            public int slowTick;  // Used for when Centipede's AI is not realized
            public int fastTick;  // Used for when Centipede's AI is realized
            public int doHax;

            public NightTerrorTracker(AbstractCreature ac, World world)
            {
                this.ac = ac;
                this.world = world;
                //On.Centipede.Update += Centipede_Update;
                On.RainWorldGame.Update += RainWorldGame_Update;
                //On.CentipedeAI.Update += CentipedeAIUPDATER;
                Debug.Log(">>> NIGHTTERROR TRACKER INIT! BEWARE OF THE DARK");
            }

            private void CentipedeAIUPDATER(On.CentipedeAI.orig_Update orig, CentipedeAI self)
            {
                orig(self);
                if (ac is not null) Update();
            }

            private void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
            {
                orig(self);
                if (ac is not null) Update();
            }

            private void Centipede_Update(On.Centipede.orig_Update orig, Centipede self, bool eu)
            {
                orig(self, eu);
                if (ac is not null) Update();
            }

            private void Tick()
            {
                // Ticking like this reduces lag!
                if (slowTick > 0)
                {
                    slowTick--;
                }
                else
                {
                    slowTick = 600;  // Updates once per 15 seconds
                }

                if (fastTick > 0)
                {
                    fastTick--;
                }
                else
                {
                    fastTick = 20;  // Updates twice per second
                }
            }


            public void Update()
            {
                if (ac?.abstractAI is null || world?.game?.session?.Players is null)
                {
                    return;
                }
                Tick();
                AbstractCreature firstPlayer = null;  // For tracking a player in different rooms
                AbstractCreature sameRoomPlayer = null;  // For tracking a player in same room
                foreach (AbstractCreature c in world.game.AlivePlayers)
                {
                    if (c.realizedCreature is Player player && player.room?.abstractRoom?.shelter is not null && !player.room.abstractRoom.shelter)
                    {
                        firstPlayer ??= c;
                        if (ac.abstractAI?.RealAI is not null && ac.pos.room == c.pos.room)
                        {
                            firstPlayer = c;
                            sameRoomPlayer = c;
                            break;
                        }
                    }
                }


                // Location update
                try
                {
                    if (ac.Room != null && ac.Room?.name != currentRoom)
                    {
                        Debug.Log(">>> Nightterror moves! From " + currentRoom + " to " + ac.Room.name);
                        currentRoom = ac.Room.name;
                    }
                }
                catch (NullReferenceException nerr)
                {
                    Debug.LogError(">>> Nightterror null error while attempting to update position!");
                    Debug.LogException(nerr);
                }
                catch (Exception err)
                {
                    Debug.LogError(">>> Nightterror generic error while attempting to update position!");
                    Debug.LogException(err);
                }

                // Non-same room update
                try
                {
                    if (ac.abstractAI is null || firstPlayer is null) return;
                    if (ac.pos.room != firstPlayer.pos.room)
                    {
                        //Debug.Log(">>> Nightterror shifts Destination! From " + previousRoom.ResolveRoomName() + " to " + firstPlayer.pos.ResolveRoomName());
                        // previousRoom = firstPlayer.pos;
                        // Change destination
                        if (ac.abstractAI.RealAI is not null)
                        {
                            ac.abstractAI.RealAI.SetDestination(firstPlayer.pos);
                        }
                        else
                        {
                            ac.abstractAI.SetDestination(firstPlayer.pos);
                        }
                    }

                    // Detect if ai is stuck
                    if (ac.abstractAI.path == null || ac.abstractAI.path.Count == 0)
                    {
                        doHax++;
                    }
                    else if (doHax > 0)
                    {
                        doHax--;
                    }

                    // TELEPORT UWU
                    if ((doHax > 400 || ac.abstractAI.strandedInRoom != -1 || (ac.abstractAI.RealAI != null && ac.abstractAI.RealAI.stranded)) && ac.pos.room != firstPlayer.pos.room && fastTick == 0)
                    {
                        Debug.Log(">>> Migrate Nightterror!");
                        ac.realizedCreature?.Abstractize();
                        if (firstPlayer.realizedCreature?.room is not null && ac.realizedCreature is null)
                        {
                            RWCustom.IntVector2 ar = firstPlayer.realizedCreature.room.exitAndDenIndex[UnityEngine.Random.Range(0, firstPlayer.realizedCreature.room.exitAndDenIndex.Length)];
                            if (!(firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar).gate || firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar).shelter))
                            {
                                ac.Move(firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar).RandomNodeInRoom());
                            }
                            doHax = 0;
                        }
                    }

                    // Migration
                    if (ac.timeSpentHere > 3600 && ac.pos.room != firstPlayer.pos.room && slowTick == 0)
                    {
                        if (firstPlayer.Room != null)
                        {
                            ac.abstractAI.MigrateTo(firstPlayer.Room.RandomNodeInRoom());
                        }
                    }
                }
                catch (NullReferenceException nerr)
                {
                    Debug.LogError(">>> Nightterror null error while attempting to update destination!");
                    Debug.LogException(nerr);
                }
                catch (Exception err)
                {
                    Debug.LogError(">>> Nightterror generic error while attempting to update destination!");
                    Debug.LogException(err);
                }


                // Same Room update
                try
                {
                    if (sameRoomPlayer is not null)
                    {
                        // Focus on one player
                        foreach (Tracker.CreatureRepresentation tracked in ac.abstractAI.RealAI.tracker.creatures)
                        {
                            //Debug.Log("This tracked creature has: " + (tracked is not null ? "Tracker " : "") + (tracked?.representedCreature is not null ? "RepresentedCreature" : ""));
                            if (tracked?.representedCreature is null)
                            {
                                Debug.Log(">>> Nightterror has an unrealized creature!");
                                continue;
                            }
                            if (tracked.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
                            {
                                Debug.Log(">>> Nightterror forgets creature: " + tracked.representedCreature.creatureTemplate.name);
                                ac.abstractAI.RealAI.tracker.ForgetCreature(tracked.representedCreature);
                                Debug.Log("    Nightterror forgor!");
                            }
                            /*
                            else
                            {
                                Debug.Log(">>> Nightterror is angy at player!");
                                ac.abstractAI.RealAI.agressionTracker.SetAnger(tracked, 10f, 10f);
                                Debug.Log("    Nightterror angr!");
                            }*/
                        }
                        // Make centipede see creature
                        //Debug.Log(">>> Nightterror See YOU!");
                        if (fastTick == 0)
                        {
                            ac.abstractAI.SetDestination(sameRoomPlayer.pos);
                        }
                        ac.abstractAI.RealAI.tracker.SeeCreature(sameRoomPlayer);
                        //Debug.Log(">>> Nightterror WANT TO KILL YOU!");
                    }
                }
                catch (NullReferenceException nerr)
                {
                    Debug.LogError(">>> Nightterror null error while attempting to track player in same room!");
                    Debug.LogException(nerr);
                }
                catch (Exception err)
                {
                    Debug.LogError(">>> Nightterror generic error while attempting to track player in same room!");
                    Debug.LogException(err);
                }


            }
        }


        private static readonly ConditionalWeakTable<AbstractCreature, NightTerrorTracker> CWT = new();
        public static NightTerrorTracker NTT(this AbstractCreature ac, World world) => CWT.GetValue(ac, _ => new(ac, world));
    }

}
