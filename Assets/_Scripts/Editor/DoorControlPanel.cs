using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DoorControlPanel : EditorWindow
{
    [MenuItem("PGSID/Door Controller")]
    static void Init()
    {
        DoorControlPanel window = (DoorControlPanel)GetWindow(typeof(DoorControlPanel));
        window.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Set Door Positions"))
        {
            GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
            foreach (GameObject door in doors)
            {
                Door d = door.GetComponent<Door>();
                d.startPos = d.transform.localPosition;
                if (d.secondDoor != null)
                    d.secondPos = d.secondDoor.localPosition;
            }
        }
    }
}
