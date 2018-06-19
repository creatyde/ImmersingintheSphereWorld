using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sphere 
public class Sphere
{
    // drawing
    public int drawIndex;
    public bool seen;

    // personal traits
    public Vector3 worldPosition { get; set; } // relative to parent
    public float worldRadius { get; set; } // size = 2 * radius
    public float rotation { get; set; } // fixed around y-axis

    // parent relevant
    public Sphere parent { get; set; } // parent
    public float radius { get; set; } 
    public Vector3 position { get; set; } // relative to parent
    public Vector3 movement;

    // children relevant
    public int numChildren { get; set; }
    public int numDescendents { get; set; }
    public List<Sphere> children { get; set; }

    // sound
    public int level; // sound distance from player
    public int soundType; // 0 (around), 1 (inside), 2 (outside), 3 (encasing)
    public GameObject sound; // sound source and filters, from different prefabs
}
