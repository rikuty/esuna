using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {


    [SerializeField] Material[] materials;

    [SerializeField] MeshRenderer cubeRenderer;

    [SerializeField] Transform cubeParent;

    string[] plateNames = {"Plate1", "Plate2", "Plate3", "Plate4", "Plate5", "Plate6", "Plate7", "Plate8"};


    float TIME_STAY = 1f;

    bool enter = false;

    float deltaTime = 0f;


    private void Awake()
    {
        int num = Random.Range(0, materials.Length);
        cubeRenderer.material = materials[num];

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!enter) return;
        deltaTime += Time.deltaTime;
        if(deltaTime> TIME_STAY)
        {
            enter = false;
            deltaTime = 0f;
            GameObject obj = Instantiate(this.gameObject, cubeParent);
            obj.transform.localPosition = Vector3.zero;
            Destroy(this.gameObject);
        
        }

	}

    void OnTriggerEnter(Collider collider)
    {
        bool plate = false;
        foreach(string name in plateNames)
        {
            if (collider.gameObject.name == name)
            {
                plate = true;
            }
        }
        if (plate)
        {
            enter = true;
        }
    }
}
