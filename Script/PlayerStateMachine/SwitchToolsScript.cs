using UnityEngine;
public class SwitchToolsScript : MonoBehaviour
{
    public int selectedTool = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectTool();
    }

    void Update()
    {
        SwitchTool();
    }
    private void selectTool()
    {
        int i = 0;
        foreach (Transform tool in transform)
        {
            if (i == selectedTool)
                tool.gameObject.SetActive(true);
            else
                tool.gameObject.SetActive(false);
            i++;
        }
    }

    public void SwitchTool()
    {
        int previousTool = selectedTool;
        if(Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedTool >= transform.childCount - 1)
            {
                selectedTool = 0;
            }
            else
                selectedTool++;
        }
        if(Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedTool <= 0)
            {
                selectedTool = transform.childCount - 1;
            }
            else
                selectedTool--;
        }

        if (previousTool != selectedTool)
            selectTool();
    }
}