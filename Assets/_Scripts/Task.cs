using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTask", menuName = "ScriptableObjects/NewTask", order = 1)]
public class Task : ScriptableObject
{
    public NodeType targetWorkStation;
    public float taskDuration;

    public void SetTaskUp(NodeType targetWorkStation, float taskDuration)
    {
        this.targetWorkStation = targetWorkStation;
        this.taskDuration = taskDuration;
    }
}
