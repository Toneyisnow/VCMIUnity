using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureMapCoordinate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(int width, int height)
    {
        GameObject imageSprite = gameObject.transform.GetChild(0).gameObject;

        imageSprite.transform.position = new Vector3(0, 0, -1);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
