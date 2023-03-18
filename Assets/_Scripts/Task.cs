using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TaskAction
{
    CHOP, WASH, SPREAD, ADD_TOPPING, GET_INGREDIENT, PLACE_INGREDIENT
}

[CreateAssetMenu(fileName = "NewTask", menuName = "ScriptableObjects/NewTask", order = 1)]
public class Task : ScriptableObject
{
    public NodeType targetWorkStation;
    public TaskAction taskAction;
    public float taskDuration;

    public GameObject IngredientPrefab;

    public GameObject TaskEnded()
    {
        GameObject r = null;
        switch (taskAction)
        {
            case TaskAction.CHOP:
                break;
            case TaskAction.WASH:
                break;
            case TaskAction.SPREAD:
                break;
            case TaskAction.ADD_TOPPING:
                break;
            case TaskAction.GET_INGREDIENT:
                r = IngredientPrefab;
                break;
            case TaskAction.PLACE_INGREDIENT:
                break;
            default:
                break;
        }
        return r;
    }
}
