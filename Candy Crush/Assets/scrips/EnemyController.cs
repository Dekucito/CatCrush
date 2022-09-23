using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("sprites")]
    public GameObject[] sprites;

    public void Animacion()
    {
        sprites[0].SetActive(false);
        sprites[1].SetActive(true);
    }
}
