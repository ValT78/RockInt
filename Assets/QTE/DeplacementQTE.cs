using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementQTE : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private RectTransform rectTransform;

    public InputActionReference QTETrigger;
    public AudioClip cymbal;
    public FollowerController follower;

    bool ReadStopHeld()
    {
        if (QTETrigger != null && QTETrigger.action != null)
        {
            return QTETrigger.action.ReadValue<float>() > 0.1f;
        }
        return false;
    }

    private void Start()
    {
        SoundManager.Instance.SetMusicVolume(1);
    }
    private void FixedUpdate()
    {

        rectTransform.localPosition += speed * Time.deltaTime * new Vector3(0, -1, 0);

        if (rectTransform.localPosition.y <= -500)
        {
            QTEManager.Instance.SpawnTouch();
            Destroy(gameObject);
        }

        if (Mathf.Abs(rectTransform.localPosition.y) <= 50 && ReadStopHeld() && follower==null)
        {
            GameManager.Instance.AddScore(5);
            QTEManager.Instance.Checkmark(rectTransform.localPosition.y);
            SoundManager.Instance.PlaySFX(cymbal, volume: 20f, pitch: Random.Range(0.9f, 1.1f));
            SoundManager.Instance.SetSFXVolume(1000);
            Destroy(gameObject);
        }
    }
}