using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTile : MonoBehaviour
{
    public GameObject[] tileOptions;

    MeshRenderer initialRenderer;

    private void Awake() {
        // Disable all meshes on the object, including all mesh options.
        initialRenderer = gameObject.GetComponent<MeshRenderer>();
        initialRenderer.enabled = false;
        foreach(GameObject obj in tileOptions) {
            obj.SetActive(false);
        }

        // Pick a random tile model and rotation to use.
        SetRandomTile();
        SetRandomRotation();
    }

    private void SetRandomTile() {
        int randomIndex = Random.Range(0, tileOptions.Length);
        tileOptions[randomIndex].SetActive(true);
    }

    private void SetRandomRotation() {
        float randomAngle = Random.Range(0, 5) * 90.0f;
        transform.rotation = transform.rotation * Quaternion.Euler(0.0f, randomAngle, 0.0f);
    }
}
