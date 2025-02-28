using UnityEngine;

public class Target : MonoBehaviour
{
    private int hp = 10;

    public void TakeDamage(float damage)
    {
        hp--;
        Debug.Log("Target: " + hp);
    }
}
