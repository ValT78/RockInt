using UnityEngine;
using UnityEngine.UI;

public class DeplacementQTE : MonoBehaviour
{
    [SerializeField] private float speed = 20f;

    private void Update()
    {
        transform.position += speed * Time.deltaTime * new Vector3(0, -1, 0);

        if (transform.position.y <= -100)
        {
            QTEManager.Instance.SpawnTouch();
            Destroy(gameObject);
        }
    }
}