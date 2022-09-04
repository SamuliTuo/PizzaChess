using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    private List<Order> orders = new List<Order>();
    private int currentID = 0;
    public int GenerateID() { currentID++; return currentID; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void AddNewOrder(Recipe[] items)
    {
        orders.Add(new Order(items));
    }

    public Tuple<int, int, Task> GetTask()
    {
        Tuple<int, int, Task> r = null;
        for (int i = 0; i < orders.Count; i++)
        {
            r = orders[i].GetTaskFromOrder();
            if (r != null)
            {
                break;
            }
        }
        return r;
    }

    // Tää ois kai hyvä muuttaa käskemään jotenkin...
    //  order: finishTask -> item: finishTask
    //         ...muuuuuttaaa...
    public void FinishTask(int orderID, int itemID, Task task)
    {
        foreach (Order _order in orders)
            if (_order.orderID == orderID)
                foreach (var _item in _order.orderItems)
                    if (_item.itemID == itemID)
                        _item.FinishTask(task);
    }

    public void OrderCompleted(int orderID)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].orderID == orderID)
            {
                orders.RemoveAt(i);
                Debug.Log("Order completed!");
            }
        }
    }
}

// _________________ O R D E R __________________ \\
public class Order
{
    public List<Item> orderItems;
    public int orderID;

    public Order(Recipe[] _orderItems)
    {
        orderItems = new List<Item>();
        orderID = TaskManager.Instance.GenerateID();
        foreach (var recipe in _orderItems)
        {
            orderItems.Add(new Item(this, recipe.phases, recipe.GetAllTasksWithPhases()));
        }
    }

    public Tuple<int, int, Task> GetTaskFromOrder()
    {
        Tuple<int, int, Task> r = null;
        for (int i = 0; i < orderItems.Count; i++)
        {
            var task = orderItems[i].GetTaskFromItem();
            if (task != null)
            {
                r = new Tuple<int, int, Task>(this.orderID, orderItems[i].itemID, task);
                break;
            }
        }
        return r;
    }

    public void ItemCompleted(int itemID)
    {
        for (int i = 0; i < orderItems.Count; i++)
        {
            if (orderItems[i].itemID == itemID)
            {
                orderItems.RemoveAt(i);
                if (orderItems.Count == 0)
                {
                    TaskManager.Instance.OrderCompleted(this.orderID);
                }
            }
        }
    }
}

// _________________ I T E M __________________ \\
public class Item
{
    public Order order;
    public int itemID;
    public int currentPhase;
    public int phases;
    public Dictionary<Task, int> pendingTasks;
    public Dictionary<Task, int> ongoingTasks;

    public Item(Order _order, int _phases, Dictionary<Task, int> _tasks)
    {
        order = _order;
        itemID = TaskManager.Instance.GenerateID();
        currentPhase = 1;
        phases = _phases;
        pendingTasks = _tasks;
        ongoingTasks = new Dictionary<Task, int>();
    }

    public Task GetTaskFromItem()
    {
        var task = CheckIfCurrentPhaseHasPendingTask();

        if (task == null)
        {
            if (ongoingTasks.GetKeysByValue(currentPhase).Count == 0)
            {
                currentPhase++;
                if (currentPhase > phases)
                {
                    Debug.Log("pizza ready!");
                    order.ItemCompleted(this.itemID);
                    return null;
                }
                else
                {
                    task = CheckIfCurrentPhaseHasPendingTask();
                }
            }
        }
        return task;
    }

    Task CheckIfCurrentPhaseHasPendingTask()
    {
        Task r = null;
        foreach (KeyValuePair<Task, int> item in pendingTasks)
        {
            if (item.Value == currentPhase)
            {
                r = item.Key;
                pendingTasks.Remove(item.Key);
                ongoingTasks.Add(item.Key, item.Value);
                break;
            }
        }
        return r;
    }

    public bool FinishTask(Task task)
    {
        if (ongoingTasks.ContainsKey(task))
        {
            Debug.Log("task finished!");
            ongoingTasks.Remove(task);
            return true;
        }

        return false;
    }
}