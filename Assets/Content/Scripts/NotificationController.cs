using TMPro;
using UnityEngine;
using System.Collections;

public class NotificationController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private LocalizationManager localization;

    private Coroutine _currentCoroutine;

    private void Show(string key)
    {
        string message = localization != null ? localization.Get(key) : key;

        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        _currentCoroutine = StartCoroutine(HideAfterSeconds());
    }

    private IEnumerator HideAfterSeconds()
    {
        yield return new WaitForSeconds(displayDuration);
        notificationText.gameObject.SetActive(false);
    }

    // Методы уведомлений

    public void NotifyCubePlaced()
    {
        Show("cube_placed");
    }

    public void NotifyCubeStacked()
    {
        Show("cube_stacked");
    }

    public void NotifyCubeRemoved()
    {
        Show("cube_removed");
    }

    public void NotifyCubeFailed()
    {
        Show("cube_failed");
    }

    public void NotifyMaxHeight()
    {
        Show("cube_max_height");
    }

    public void NotifyCubeFellInHole()
    {
        Show("cube_fell_in_hole");
    }
}