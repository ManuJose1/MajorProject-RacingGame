using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{

    CanvasGroup HUDCanvasGroup;
    float HUDSetting = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HUDCanvasGroup = this.GetComponent<CanvasGroup>();
        HUDCanvasGroup.alpha = 0;

        if(PlayerPrefs.HasKey("HUD"))
            HUDSetting = PlayerPrefs.GetFloat("HUD");
    }

    // Update is called once per frame
    void Update()
    {
        if(RaceMonitor.racing)
            HUDCanvasGroup.alpha = HUDSetting;

        if(Input.GetKeyDown(KeyCode.H))
        {
            HUDCanvasGroup.alpha = HUDCanvasGroup.alpha == 1 ? 0 : 1;
            HUDSetting = HUDCanvasGroup.alpha;
            PlayerPrefs.SetFloat("HUD", HUDCanvasGroup.alpha);
        }
    }
}
