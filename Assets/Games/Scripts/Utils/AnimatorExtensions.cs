using UnityEngine;

public static class AnimatorExtensions
{
    public static void SetAnimator<T>(Animator animator, int anim, T value)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null!!!");
            return;
        }
        
        switch (value)
        {
            case bool boolValue:
                animator.SetBool(anim, boolValue);
                break;
            case int intValue:
                animator.SetInteger(anim, intValue);
                break;
            case float floatValue:
                animator.SetFloat(anim, floatValue);
                break;
            case string stringValue:
                animator.SetTrigger(stringValue);
                break;
        }
    }
}
