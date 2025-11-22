using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DeplacementQTE : MonoBehaviour
{
    [SerializeField] private float speed = 20f;

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
        transform.position += speed * Time.deltaTime * new Vector3(0, -1, 0);

        if (transform.position.y <= -100)
        {
            QTEManager.Instance.SpawnTouch();
            Destroy(gameObject);
        }

        /*if (-50<=transform.position.y && transform.position.y<= 50 && ReadStopHeld())
        {
            Destroy(gameObject);
        }*/
    }
}