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
        public World world;
        public int slowTick;  // Used for when Centipede is not realized
        public int fastTick;  // Used for when Centipede is realized

        public NightTerrorTrain(AbstractCreature ac, World world)
        {
            this.ac = ac;
            this.world = world;
            On.RainWorldGame.Update += Update;
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
                slowTick = 200;  // Updates once per 5 seconds
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


        public void Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            Tick();
            if (ac?.abstractAI is null || world?.game?.session?.Players is null)
            {
                return;
            }
            AbstractCreature firstPlayer = null;  // For tracking a player in different rooms
            AbstractCreature sameRoomPlayer = null;  // For tracking a player in same room
            foreach(AbstractCreature c in world.game.session.Players)
            {
                if (c.realizedCreature is Player player && !player.dead)
                {
                    firstPlayer ??= c;
                    if (ac.abstractAI?.RealAI is not null && ac.pos.room == c.pos.room)
                    {
                        sameRoomPlayer = c;
                        break;
                    }
                }
            }
            
            // Location update
            try
            {
                if (ac.abstractAI is null) return;
                if (ac.abstractAI.destination != previousRoom)
                {
                    Debug.Log(">>> Nightterror moves! From " + previousRoom.ResolveRoomName() + " to " + ac.abstractAI.destination.ResolveRoomName());
                    previousRoom = ac.abstractAI.destination;
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

            // Same Room update
            try
            {
                if (sameRoomPlayer is not null && fastTick == 0)
                {
                    // Focus on one player
                    foreach(Tracker.CreatureRepresentation tracked in ac.abstractAI.RealAI.tracker.creatures)
                    {
                        if (tracked?.representedCreature is null)
                        {
                            continue;
                        }
                        if (tracked.representedCreature != sameRoomPlayer)
                        {
                            ac.abstractAI.RealAI.tracker.ForgetCreature(tracked.representedCreature);
                        }
                        else
                        {
                            ac.abstractAI.RealAI.agressionTracker.SetAnger(tracked, 100f, 100f);
                        }
                    }
                    // Make centipede see creature
                    ac.abstractAI.RealAI.tracker.SeeCreature(sameRoomPlayer);
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


            // Non-same room update
            try
            {
                if (firstPlayer is null || slowTick != 0) return;
                if (ac.abstractAI.destination.room != firstPlayer.pos.room)
                {
                    Debug.Log(">>> Nightterror shifts Destination! From " + ac.abstractAI.destination.ResolveRoomName() + " to " + firstPlayer.pos.ResolveRoomName());

                    // Change destination
                    ac.abstractAI.SetDestination(firstPlayer.pos);
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
        }
    }
}
