using System.Collections.Generic;
using UnityEngine;

// add this to anything that should fade out in 2D when it's not at the player's depth
// (walls, ceilings, doors). leftover pieces get this automatically
// material needs to be transparent (URP Lit -> Surface Type = Transparent)
[RequireComponent(typeof(Renderer))]
public class Fadeable : MonoBehaviour
{
    public static readonly List<Fadeable> All = new List<Fadeable>();

    Renderer rend;
    Color baseColor;
    bool faded;

    public Bounds Bounds => rend.bounds;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color; // this makes a per object copy of the material
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public void SetFaded(bool value, float alpha)
    {
        if (value == faded) return;
        faded = value;
        Color c = baseColor;
        if (faded) c.a = baseColor.a * alpha;
        rend.material.color = c;
    }
}
