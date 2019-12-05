using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour {

    public LayerMask targetMask;
    public Color dotHightlightColour;
    Color originalDotColour;
    public SpriteRenderer dot;

    private void Start()
    {
        Cursor.visible = false;
        originalDotColour = dot.color;

    }

    private void Update()
    {
        transform.Rotate(Vector3.forward * -40 * Time.deltaTime);

    }

    public void DetectTargets(Ray ray)
    {
        if (Physics.Raycast(ray, 100, targetMask))
        {
            dot.color = dotHightlightColour;
        }
        else
        {
            dot.color = originalDotColour;
        }
    }
}
