using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using RWCustom;
using UnityEngine;
using MoreSlugcats;

namespace PitchBlack;
sealed class UmbraMaskFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType AbstrUmbraMask = new("UmbraMask", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID UmbraMaskUnlock = new("Crate", true);

    public UmbraMaskFisob() : base(AbstrUmbraMask)
    {
        Icon = new SimpleIcon("icon_UmbraMask", new Color(0.529f, 0.184f, 0.360f));
        RegisterUnlock(UmbraMaskUnlock);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock)
    {
        // Centi shield data is just floats separated by ; characters.

        UmbraMaskAbstract result = new UmbraMaskAbstract(world, entitySaveData.Pos, entitySaveData.ID);

        // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
        if (unlock is SandboxUnlock u) { } //idk what this means lol

        return result;
    }
}
