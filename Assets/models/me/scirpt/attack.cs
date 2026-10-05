using UnityEngine;

public class attack : MonoBehaviour
{
    [SerializeField] private Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(anim == null)
            anim = GetComponent<Animator>();
        anim.SetBool("isAttack", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            anim.SetBool("isAttack",true);
        }else
            anim.SetBool("isAttack", false);
    }
}
