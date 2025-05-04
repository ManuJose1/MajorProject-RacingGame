using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    Renderer[] rends;
    float alpha = 1;
    float flashSpeed = 2;

    // Start is called before the first frame update
    void Start()
    {
        rends = this.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            if (r.CompareTag("MapArrow")) continue;
            
            // Enable transparency for URP materials
            r.material.SetFloat("_Surface", 1.0f); // 1 = Transparent
            r.material.SetFloat("_Blend", 0.0f);   // Alpha blending
            r.material.SetFloat("_ZWrite", 0);     // Disable depth writing
            r.material.renderQueue = 3000;         // Transparent render queue
            r.material.color = new Color(1, 1, 1, alpha); // Set initial alpha
        }
    }

    void OnDisable()
    {
        foreach (Renderer r in rends)
        {
            if (r.CompareTag("MapArrow")) continue;

            // Reset material to opaque for URP materials
            r.material.SetFloat("_Surface", 0.0f); // 0 = Opaque
            r.material.SetFloat("_Blend", 1.0f);   // Default blending
            r.material.SetFloat("_ZWrite", 1);     // Enable depth writing
            r.material.renderQueue = 2000;         // Opaque render queue
            r.material.color = new Color(1, 1, 1, 1); // Reset alpha to fully opaque
        }
    }

    // Update is called once per frame
    void Update()
    {
        alpha = 0.3f + Mathf.PingPong(Time.time * flashSpeed, 0.7f);
        foreach (Renderer r in rends)
        {
            if (r.CompareTag("MapArrow")) continue;
            r.material.color = new Color(1, 1, 1, alpha);
        }
    }
}
