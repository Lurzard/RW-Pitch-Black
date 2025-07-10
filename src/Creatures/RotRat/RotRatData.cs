using System;
using System.Runtime.CompilerServices;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PitchBlack;

public class RotData
{
    public RotData (int numOfBulbs) {
        numOfSprites = numOfBulbs*2;
        bulbs = new bulb[numOfBulbs];
        for (int i = 0; i < numOfBulbs; i++)
        {
            bulbs[i] = new bulb(Custom.RNV() * Random.Range(0f, 10f));
        }
    }
    public int startSprite;
    public int currentSprite;
    public int numOfSprites;
    public bulb[] bulbs;
}
public class bulb(Vector2 vector2)
{
    public Vector2 position = vector2;
}