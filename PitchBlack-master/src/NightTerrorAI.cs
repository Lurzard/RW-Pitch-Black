using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PitchBlack
{
    public class NightTerrorTrain
    {
        public AbstractCreature ac;
        public WorldCoordinate previousRoom;
        public string currentRoom;
        public World world;
        public int slowTick;  // Used for when Centipede's AI is not realized
        public int fastTick;  // Used for when Centipede's AI is realized

        public NightTerrorTrain(AbstractCreature ac, World world)
        {
            this.ac = ac;
            this.world = world;
            On.Centipede.Update += Centipede_Update;
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
                slowTick = 400;  // Updates once per 10 seconds
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
            foreach(AbstractCreature c in world.game.AlivePlayers)
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
                if (ac.abstractAI.destination.room != previousRoom.room)
                {
                    Debug.Log(">>> Nightterror shifts Destination! From " + previousRoom + " to " + firstPlayer.pos.ResolveRoomName());
                    previousRoom = firstPlayer.pos;
                    // Change destination
                    ac.abstractAI.SetDestination(previousRoom);
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
                    foreach(Tracker.CreatureRepresentation tracked in ac.abstractAI.RealAI.tracker.creatures)
                    {
                        Debug.Log("This tracked creature has: " + (tracked is not null? "Tracker " : "") + (tracked?.representedCreature is not null? "RepresentedCreature" : ""));
                        if (tracked?.representedCreature is null)
                        {
                            Debug.Log(">>> Nightterror has an unrealized creature!");
                            continue;
                        }
                        if (tracked.representedCreature != sameRoomPlayer)
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
                    Debug.Log(">>> Nightterror See YOU!");
                    ac.abstractAI.RealAI.tracker.SeeCreature(sameRoomPlayer);
                    Debug.Log(">>> Nightterror WANT TO KILL YOU!");
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
}
