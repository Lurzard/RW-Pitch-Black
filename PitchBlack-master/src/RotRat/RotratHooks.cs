using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;

namespace PitchBlack;

public class RotData
{
    public RotData (int numOfBulbs) {
        this.numOfSprites = numOfBulbs*2;
        bulbs = new bulb[numOfBulbs];
        for (int i = 0; i < numOfBulbs; i++)
        {
            bulbs[i] = new bulb(Custom.RNV() * Random.Range(0f, 10f));
        }
    }
    public int startSprite;
    public int currentSprite;
    public int numOfSprites;
    public bool ready;
    public bulb[] bulbs;
}
public class bulb
{
    public bulb (Vector2 vector2) {
        position = vector2;
    }
    public Vector2 position;
}
public class RotRatHooks
{
    static ConditionalWeakTable<MouseGraphics, RotData> rotratdata = new ConditionalWeakTable<MouseGraphics, RotData>();
    static public void Apply()
    {
        On.MouseAI.ctor += GivePreyTracker;
        On.MouseAI.Update += Hunter;
        On.LanternMouse.Update += LanternMouse_Update;
        On.LanternMouse.ctor += ivars;
        On.MouseAI.IUseARelationshipTracker_ModuleToTrackRelationship += Preyrelationshipfix;
        On.LanternMouse.CarryObject += LanternMouse_CarryObject;
        On.MouseGraphics.ctor += MouseGraphics_ctor;
        On.MouseGraphics.InitiateSprites += MouseGraphics_InitiateSprites;
        On.MouseGraphics.AddToContainer += MouseGraphics_AddToContainer;
        On.MouseGraphics.DrawSprites += MouseGraphics_DrawSprites;
    }

    static private void LanternMouse_CarryObject(On.LanternMouse.orig_CarryObject orig, LanternMouse self)
    {
        if (!self.safariControlled && self.grasps[0].grabbed is Creature && self.AI.DynamicRelationship((self.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats) 
        {
            self.AI.preyTracker.ForgetPrey((self.grasps[0].grabbed as Creature).abstractCreature);
            self.LoseAllGrasps();
            return;   
        }
        PhysicalObject grabbed = self.grasps[0].grabbed;
        float num = self.mainBodyChunk.rad + self.grasps[0].grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad;
        Vector2 a = -Custom.DirVec(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos));
        float num2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass / (self.mainBodyChunk.mass + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass);
        num2 *= 0.2f * (1f - self.AI.stuckTracker.Utility());
        self.mainBodyChunk.pos += a * num2;
        self.mainBodyChunk.vel += a * num2;
        grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos -= a * (1f - num2);
        grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel -= a * (1f - num2);
        Vector2 vector = self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * num;
        Vector2 vector2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel - self.mainBodyChunk.vel;
        grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel = self.mainBodyChunk.vel;
        if (self.enteringShortCut == null && (vector2.magnitude * grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad)))
        {
            self.LoseAllGrasps();
        }
        else
        {
            grabbed.bodyChunks[self.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(self.abstractCreature.world.game.evenUpdate, vector);
        }
        if (self.grasps[0] != null)
        {
            for (int i = 0; i < 2; i++)
            {
                self.grasps[0].grabbed.PushOutOf(self.bodyChunks[i].pos, self.bodyChunks[i].rad, self.grasps[0].chunkGrabbed);
            }
        }
    }
    static private AIModule Preyrelationshipfix(On.MouseAI.orig_IUseARelationshipTracker_ModuleToTrackRelationship orig, MouseAI self, CreatureTemplate.Relationship relationship)
    {
        if(relationship.type == CreatureTemplate.Relationship.Type.Eats)
        {
            return self.preyTracker;
        }
        return orig(self, relationship);
    }
    static private void ivars(On.LanternMouse.orig_ctor orig, LanternMouse self, AbstractCreature abstractCreature, World world)
    {
        
        orig(self, abstractCreature, world);
        if(self.Template.type == CreatureTemplateType.Rotrat)
        {
            Random.State state = Random.state;
            Random.InitState(self.abstractCreature.ID.RandomSeed);
            float hue;
            if (Random.value < 0.01)
            {
                hue = 0.8532407407407407f;
                Debug.Log("the mouse behind the slaughter....");
                // hehe purple mouse.
            }
            else
                        if (Random.value < 0.05)
            {
                hue = Mathf.Lerp(0.444f, 0.527f, Random.value);
                //shock cyans?
            }
            else if (Random.value < 0.2)
            {
                hue = Mathf.Lerp(0f, 0.05f, Random.value);
                //shock reds?
            }
            else
            {
                hue = Mathf.Lerp(0.055f, 0.125f, Random.value);
                //shock oranges + yellows?
            }
            HSLColor color = new HSLColor(hue, 1f, Random.Range(0.4f,0.8f));
            float value = Random.value;
            self.iVars = new LanternMouse.IndividualVariations(value, color);
            Random.state = state;
        }
    }
    static private void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
    {
        orig(self, eu);
        if(self.Template.type == CreatureTemplateType.Rotrat)
        {
            if(self.grasps[0] != null)
            {
                self.CarryObject();
            }
            if (self.AI.behavior == MouseAI.Behavior.Hunt)
            {
                if (self.AI.preyTracker.MostAttractivePrey != null)
                {
                    Tracker.CreatureRepresentation prey = self.AI.preyTracker.MostAttractivePrey;
                    Creature realprey = prey.representedCreature.realizedCreature;
                    if (Custom.DistLess(prey.representedCreature.pos, self.abstractCreature.pos, 4f))
                    {
                        self.Squeak(1f);
                        if (self.grasps[0] == null && (realprey.dead || realprey.Stunned))
                        {
                            self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, 0.5f, false, true);
                            self.AI.behavior = MouseAI.Behavior.ReturnPrey;
                        }
                        else
                        {
                            if(realprey.TotalMass < self.TotalMass*1.5)
                            {
                                realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));
                                self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, Random.Range(0.3f, 0.7f), true, true);
                                
                            }
                            else
                            {
                                if(Random.Range(0f,100f) < 20f)
                                {

                                    realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));

                                }
                                else
                                {
                                    realprey.Stun(realprey.stun);
                                }
                                self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, Random.Range(0.3f, 0.7f), true, false);
                            }
                            
                        }
                    }
                }
            }
        }
    }
    static private void Hunter(On.MouseAI.orig_Update orig, MouseAI self)
    {
        if(self.mouse.Template.type == CreatureTemplateType.Rotrat)
        {
            self.preyTracker.Update();
            self.stuckTracker.Update();
            orig(self);
            AIModule aimoduule = self.utilityComparer.HighestUtilityModule();
            if (aimoduule != null && aimoduule is PreyTracker)
            {
                self.behavior = MouseAI.Behavior.Hunt;
            }
            if (self.behavior == MouseAI.Behavior.Hunt)
            {
                if (self.mouse.grasps[0] != null && self.mouse.grasps[0].grabbed is Creature && self.StaticRelationship((self.mouse.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    self.behavior = MouseAI.Behavior.ReturnPrey;
                }
                else if (self.preyTracker.MostAttractivePrey != null && !self.mouse.safariControlled)
                {
                    self.creature.abstractAI.SetDestination(self.preyTracker.MostAttractivePrey.BestGuessForPosition());
                    self.mouse.runSpeed = Mathf.Lerp(self.mouse.runSpeed, 1f, 0.08f);
                }
            }
            if (self.behavior == MouseAI.Behavior.ReturnPrey)
            {
                if (self.denFinder.GetDenPosition() != null)
                {
                    self.creature.abstractAI.SetDestination(self.denFinder.GetDenPosition().Value);
                    Debug.Log($"rorat number {self.mouse.abstractCreature.ID.number.ToString()}: YIPPE! i found a den!");
                }
                else
                {
                    Debug.Log($"rorat number {self.mouse.abstractCreature.ID.number.ToString()}: FUCK! no den found :[");
                }
            }
            if (Input.GetKey(KeyCode.T) && Input.GetKey(KeyCode.M))
            {
                self.behavior = MouseAI.Behavior.Hunt;
                Debug.Log("RRs have been forced to hunt.");
            }
        }
        else
        {
            orig(self);
        }
    }
    static private void GivePreyTracker(On.MouseAI.orig_ctor orig, MouseAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if(self.mouse.Template.type == CreatureTemplateType.Rotrat)
        {
            self.AddModule(new PreyTracker(self, 3, 2f, 10f, 70f, 0.5f));
            self.utilityComparer.AddComparedModule(self.preyTracker, null, 1f, 1.5f);
            self.AddModule(new StuckTracker(self,true,false));
        }
        
    }
    static private void MouseGraphics_ctor(On.MouseGraphics.orig_ctor orig, MouseGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if(self.mouse.Template.type == CreatureTemplateType.Rotrat)
        {
            rotratdata.Add(self,new RotData(Random.Range(3, 7)));
        }
    }
    static private void MouseGraphics_InitiateSprites(On.MouseGraphics.orig_InitiateSprites orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.mouse.Template.type == CreatureTemplateType.Rotrat && rotratdata.TryGetValue(self, out RotData rotData))
        {
            rotData.startSprite = sLeaser.sprites.Length;
            System.Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + rotData.numOfSprites);
            for (int i = rotData.startSprite; i < rotData.startSprite+rotData.numOfSprites; i+=2)
            {
                // rotData.bulbs[i].firstsprite = i;
                sLeaser.sprites[i] = new FSprite("Futile_White");
                sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"];
                sLeaser.sprites[i].scale = 0.6f;
                sLeaser.sprites[i + 1] = new FSprite("mouseEyeB5");
                sLeaser.sprites[i + 1].scale = 0.6f;
            }
            rotData.ready = true;
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    static private void MouseGraphics_AddToContainer(On.MouseGraphics.orig_AddToContainer orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);
        if (self.mouse.Template.type == CreatureTemplateType.Rotrat && rotratdata.TryGetValue(self,out RotData rotData) && rotData.ready == true)
        {
            FContainer container = rCam.ReturnFContainer("Midground");
            for(int i = rotData.startSprite + 1; i < rotData.startSprite + rotData.numOfSprites; i++)
            {
                FSprite bulb = sLeaser.sprites[i];
                bulb.RemoveFromContainer();
                container.AddChild(bulb);
            }
            rotData.ready = false;
        }
    }
    static private void MouseGraphics_DrawSprites(On.MouseGraphics.orig_DrawSprites orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.mouse.Template.type == CreatureTemplateType.Rotrat && rotratdata.TryGetValue(self, out RotData rotData))
        {
            float zRotation = Mathf.Max(Mathf.Abs(Mathf.Lerp(self.lastProfileFac, self.profileFac, timeStacker)), Mathf.InverseLerp(0.5f, 0.7f, Mathf.Lerp(self.lastBackToCam, self.backToCam, timeStacker)));
            Debug.Log($"Pitch Black: {zRotation}");
            sLeaser.sprites[self.EyeBSprite(0)].element = Futile.atlasManager.GetElementWithName("mouseEyeB5");
            sLeaser.sprites[self.EyeBSprite(1)].element = Futile.atlasManager.GetElementWithName("mouseEyeB5");
            for (int i = rotData.startSprite; i < rotData.startSprite + rotData.numOfSprites; i+=2)
            {
                if (zRotation < 0.1f) {
                    sLeaser.sprites[i].isVisible = false;
                    sLeaser.sprites[i+1].isVisible = false;
                }
                else {
                    sLeaser.sprites[i].isVisible = true;
                    sLeaser.sprites[i+1].isVisible = true;
                }
                sLeaser.sprites[i].color = self.DecalColor;
                sLeaser.sprites[i].scale = Mathf.Lerp(0, 0.6f, zRotation);
                sLeaser.sprites[i].SetPosition(sLeaser.sprites[self.BodySprite(0)].GetPosition() + rotData.bulbs[(i-rotData.startSprite)/2].position);
                sLeaser.sprites[i + 1].color = self.mouse.iVars.color.rgb;
                sLeaser.sprites[i + 1].SetPosition(sLeaser.sprites[i].GetPosition());
            }
        }
    }
}