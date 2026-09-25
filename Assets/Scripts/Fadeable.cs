using System.Collections.Generic;
using UnityEngine;

public class Fadeable : MonoBehaviour
{
    public static List<Fadeable> allFadeables = new List<Fadeable>();

    private Renderer rend;
    private Color startColor;
    private bool isFaded;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }

    public Bounds GetBounds()
    {
        return rend.bounds;
    }

    void OnEnable()
    {
        allFadeables.Add(this);
    }

    void OnDisable()
    {
        allFadeables.Remove(this);
    }

    public void SetFade(bool fade, float alpha)
    {
        if (fade != isFaded)
        {
            isFaded = fade;
            Color color = startColor;
            if (isFaded)
            {
                color.a = startColor.a * alpha;
            }
            rend.material.color = color;
        }
    }
}
