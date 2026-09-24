using UnityEngine;

public class attack : MonoBehaviour
{
    public Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim.SetBool("isAttack", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetBool("isAttack",true);
        }else
            anim.SetBool("isAttack", false);
    }
}
