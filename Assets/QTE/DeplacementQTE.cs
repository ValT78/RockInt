using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementQTE : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private RectTransform rectTransform;

    public InputActionReference QTETrigger;

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
        rectTransform.localPosition += speed * Time.deltaTime * new Vector3(0, -1, 0);

        if (rectTransform.localPosition.y <= -500)
        {
            QTEManager.Instance.SpawnTouch();
            Destroy(gameObject);
        }

        if (Mathf.Abs(rectTransform.localPosition.y)<=50 && ReadStopHeld())
        {
            GameManager.Instance.AddScore(1);
            Destroy(gameObject);
            /* A FAIRE DEMAIN !!!
            SoundManager.Instance.PlaySFX(footStepClip, volume: 0.9f, pitch: Random.Range(0.9f, 1.1f));*/
        }
    }
}