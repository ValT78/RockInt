using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementQTE : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private RectTransform rectTransform;

    public InputActionReference QTETrigger;
    public AudioClip trance_lead;

    bool ReadStopHeld()
    {
        if (QTETrigger != null && QTETrigger.action != null)
        {
            return QTETrigger.action.ReadValue<float>() > 0.1f;
        }
        return false;
    }
    
    private void FixedUpdate()
    {
        if (rectTransform.localPosition.y<-50 && rectTransform.localPosition.y>-52 && gameObject.name!="Touche 7")
        {
            SoundManager.Instance.StopAllSFX();
        }

        rectTransform.localPosition += speed * Time.deltaTime * new Vector3(0, -1, 0);

        if (rectTransform.localPosition.y <= -500)
        {
            QTEManager.Instance.SpawnTouch();
            Destroy(gameObject);
        }

        if (Mathf.Abs(rectTransform.localPosition.y)<=50 && ReadStopHeld())
        {
            GameManager.Instance.AddScore(5);
            QTEManager.Instance.Checkmark(rectTransform.localPosition.y);
            SoundManager.Instance.PlaySFX(trance_lead, volume: 0.9f, pitch: Random.Range(0.9f, 1.1f));
            Destroy(gameObject);
        }
    }
}