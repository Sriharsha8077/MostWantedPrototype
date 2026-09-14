using UnityEngine;

public class IntroPanelUI : MonoBehaviour
{
    public GameObject panel;

    void Start()
    {
        // Reads the static flag directly rather than GameManager.CurrentState,
        // so this doesn't depend on Start() execution order between scripts.
        panel.SetActive(!GameManager.HasShownIntroThisSession);
    }

    void Update()
    {
        if (!panel.activeSelf) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            Dismiss();
    }

    public void OnStartButtonPressed() => Dismiss();

    void Dismiss()
    {
        panel.SetActive(false);
        GameManager.Instance.BeginFirstRun();
    }
}