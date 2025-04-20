using System.Collections.Generic;
using UnityEngine;
using static Pom.Pom;

namespace PitchBlack;

public class PBPOMSunrays
{
	internal class Sunrays : CosmeticSprite
	{
		private readonly PlacedObject placedObject;
		public Sunrays(PlacedObject pObj, Room room) {
			this.room = room;
			this.placedObject = pObj;
			room.AddObject(this);
		}
		public override void Update(bool eu) {
			base.Update(eu);
			// Debug.Log("POM Object is here");
		}
		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White", true);
			sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["Sunrays"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
		}
		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

			ManagedData objData = (ManagedData)placedObject.data;
			Vector2 vector = placedObject.pos-rCam.pos;
			sLeaser.sprites[0]._localVertices[0].Set(objData.GetValue<Vector2>("TopRight").x+vector.x,objData.GetValue<Vector2>("TopRight").y+vector.y);
			sLeaser.sprites[0]._localVertices[1].Set(objData.GetValue<Vector2>("TopLeft").x+vector.x,objData.GetValue<Vector2>("TopLeft").y+vector.y);
			sLeaser.sprites[0]._localVertices[2].Set(objData.GetValue<Vector2>("BottomLeft").x+vector.x,objData.GetValue<Vector2>("BottomLeft").y+vector.y);
			sLeaser.sprites[0]._localVertices[3].Set(objData.GetValue<Vector2>("BottomRight").x+vector.x,objData.GetValue<Vector2>("BottomRight").y+vector.y);

			sLeaser.sprites[0].color = new Color(objData.GetValue<float>("RayR"),objData.GetValue<float>("RayG"),objData.GetValue<float>("RayB"),objData.GetValue<float>("RayA"));

			Shader.SetGlobalFloat("_MovementSpeedLayer1", objData.GetValue<float>("Layer 1 Movement Speed"));
			Shader.SetGlobalFloat("_ThinnessLayer1", objData.GetValue<float>("Layer 1 Ray Thinness"));
			Shader.SetGlobalFloat("_IntensityLayer1", objData.GetValue<float>("Layer 1 Intensity"));

			Shader.SetGlobalFloat("_MovementSpeedLayer2", objData.GetValue<float>("Layer 2 Movement Speed"));
			Shader.SetGlobalFloat("_ThinnessLayer2", objData.GetValue<float>("Layer 2 Ray Thinness"));
			Shader.SetGlobalFloat("_IntensityLayer2", objData.GetValue<float>("Layer 2 Intensity"));

			Shader.SetGlobalFloat("_BloomIntensity", objData.GetValue<float>("Bloom Intensity"));
			Shader.SetGlobalInt("_AdditiveLayers",objData.GetValue<int>("Inter-Ray Blend Mode"));	// Shader uniform name is very not accurate to what it does I think, just didn't want to rebuild it again.
		}
	}
    internal static void RegisterLightrays() {
		List<ManagedField> fields = new List<ManagedField> {
			new FloatField("RayR", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Red"),
			new FloatField("RayG", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Green"),
			new FloatField("RayB", 0f, 1f, 0.96f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Blue"),
			new FloatField("RayA", 0f, 1f, 0.5f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Alpha"),

			new FloatField("Layer 1 Movement Speed", -30f, 30f, 1.7f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Layer 1 Movement Speed"),
			new FloatField("Layer 1 Ray Thinness", 0.01f, 3f, 2f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Layer 1 Ray Thinness"),
			new FloatField("Layer 1 Intensity", 0f, 5f, 1f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Layer 1 Intensity"),

			new FloatField("Layer 2 Movement Speed", -30f, 30f, -1.65f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Layer 2 Movement Speed"),
			new FloatField("Layer 2 Ray Thinness", 0.01f, 3f, 1.4f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Layer 2 Ray Thinness"),
			new FloatField("Layer 2 Intensity", 0f, 5f, 1f, 0.1f, ManagedFieldWithPanel.ControlType.slider, "Layer 2 Intensity"),

			new FloatField("Bloom Intensity", 0f, 1f, 1f, 0.01f, ManagedFieldWithPanel.ControlType.slider, "Bloom Intensity"),
			new IntegerField("Blend Mode", 0, 1, 0, ManagedFieldWithPanel.ControlType.arrows ,"Blend Mode"),

			new Vector2Field("TopLeft", new Vector2(-40,40), Vector2Field.VectorReprType.line),
			new Vector2Field("TopRight", new Vector2(40,40), Vector2Field.VectorReprType.line),
			new Vector2Field("BottomLeft", new Vector2(-40,-40), Vector2Field.VectorReprType.line),
			new Vector2Field("BottomRight", new Vector2(40,-40), Vector2Field.VectorReprType.line)
		};
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(Sunrays), "Light-Rays", "Pitch-Black");
    }
}