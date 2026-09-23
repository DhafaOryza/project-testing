using UnityEngine;

public class InputTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space ditekan!");
        }
    }
}