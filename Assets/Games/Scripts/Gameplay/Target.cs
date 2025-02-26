using UnityEngine;

public class Target : MonoBehaviour
{
    private int hp = 10;

    public void Hit()
    {
        hp--;
        Debug.Log("Target: " + hp);
    }
}
