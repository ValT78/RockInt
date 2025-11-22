using UnityEngine;

public class Player : MonoBehaviour
{
    protected Vector3 ClampPositionToDancefloor(Vector3 pos, float margin = 0f)
    {
        float minX = GameManager.minX + margin;
        float maxX = GameManager.maxX - margin;
        float minZ = GameManager.minZ + margin;
        float maxZ = GameManager.maxZ - margin;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        return pos;
    }
}
