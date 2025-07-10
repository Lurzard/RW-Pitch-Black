using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static System.Reflection.BindingFlags;

namespace PitchBlack;


public abstract class NightTerror(AbstractCreature centi)
{
    private readonly WeakReference<AbstractCreature> abstrNightterrorRef = new(centi);

    private bool _justRevived;
    private int timeUntilRevive;
    public bool diedToSporeCloud;
    private readonly float MaxHP = (centi.state as HealthState).health;
    
    // Spinch: thrown PuffBalls makes NT revive faster because that looks cool
    private int MaxTimeUntilRevive => (8 - ModOptions.pursuerAgro.Value) * (diedToSporeCloud ? 80 : 800);

    public void TryRevive()
    {
        Debug.Log("Trying to revive the Night Terror");
        if (!abstrNightterrorRef.TryGetTarget(out var centi) || !ModOptions.pursuer.Value) return;

        if (centi.state.alive || centi.realizedCreature != null && !centi.realizedCreature.dead) return;

        timeUntilRevive++;

        if (timeUntilRevive >= MaxTimeUntilRevive)
        {
            Revive();
        }
    }
    private void Revive()
    {
        //yoinked from BigSpider.Revive
        if (!abstrNightterrorRef.TryGetTarget(out var centi)) return;

        timeUntilRevive = 0;
        _justRevived = true;

        SetRealizedCreatureDataOnRevive();

        (centi.state as HealthState).health = MaxHP;
        centi.abstractAI.SetDestination(centi.pos);

        for (int i = centi.stuckObjects.Count - 1; i >= 0; i--)
        {
            if (centi.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick
                && centi.realizedCreature.abstractCreature.stuckObjects[i].A.type == AbstractPhysicalObject.AbstractObjectType.Spear
                && centi.stuckObjects[i].A.realizedObject is Spear spear)
            {
                spear.ChangeMode(Weapon.Mode.Free);
            }
        }
        centi.LoseAllStuckObjects();

        Debug.Log("Pitch Black: NightTerror revived!");
    }
    private void SetRealizedCreatureDataOnRevive()
    {
        if (!abstrNightterrorRef.TryGetTarget(out AbstractCreature centi)) return;

        if (centi.realizedCreature != null && _justRevived)
        {
            _justRevived = false;
            centi.realizedCreature.dead = false;
            centi.realizedCreature.killTag = null;
            centi.realizedCreature.killTagCounter = 0;
        }
    }
}