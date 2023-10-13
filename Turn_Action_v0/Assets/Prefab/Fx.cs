using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fx : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;
    [SerializeField]
    float fxTime;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.sortingOrder = ((4 - (int)transform.position.y) * 2) + 1;
        Destroy(gameObject, fxTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
