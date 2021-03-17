using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public void set_destroyTime(float destroyTime)
    {
        Destroy(gameObject, destroyTime);
    }
    public void destroyNow()
    {
        Destroy(gameObject);
    }

}
