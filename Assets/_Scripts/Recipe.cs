using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/NewRecipe", order = 1)]
public class Recipe : ScriptableObject
{
    [Header("Recipes are cooked in phases.")]
    [Header("Every task in a recipe belongs to one phase.")]
    [Header("Every task in the same phase can be worked at the same time.")]
    [Header("Phases are done in ascending order, starting from phase 1.")]
    [Space]
    [Header("How many phases in this recipe:")]

    public int phases;

    private Dictionary<Task, int> allRecipeTasks = new Dictionary<Task, int>();

    [Serializable]
    public class TaskForPhase
    {
        public Task task;
        public int phase;
    }
    public List<TaskForPhase> tasks = new List<TaskForPhase>();

    public Dictionary<Task, int> GetAllTasksWithPhases()
    {
        if (allRecipeTasks.Count == 0)
            foreach (var kvp in tasks)
                allRecipeTasks[kvp.task] = kvp.phase;

        return allRecipeTasks;
    }
}