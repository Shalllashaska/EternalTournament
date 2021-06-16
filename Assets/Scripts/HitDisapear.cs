using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitDisapear : MonoBehaviour
{

    public SpriteRenderer a;

    private float op;
    // Start is called before the first frame update
    void Start()
    {
        op = 1;
    }

    // Update is called once per frame
    void Update()
    {
        a.color = new Color(255, 255, 255, op);
        op -= Time.deltaTime * 0.35f;
        if (op<=0) Destroy(gameObject);
    }
}
