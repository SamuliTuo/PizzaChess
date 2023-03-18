using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    private List<Order> orders = new List<Order>();
    private int currentID = 0;
    public int GenerateID() { 
        currentID++;
        return currentID;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    public void AddNewOrder(Recipe[] items)
    {
        print("order received, items: " + items.Length);
        Order o = new Order(items, GenerateID());
        orders.Add(o);
    }

    // Contains < orderID, itemID, Task >
    public Tuple<int, int, Task> GetTask()
    {
        Tuple<int, int, Task> r = new Tuple<int, int, Task>(-1, -1, null);
        for (int i = 0; i < orders.Count; i++)
        {
            r = orders[i].GetTaskFromOrder();
            if (r.Item3 != null)
            {
                break;
            }
        }
        return r;
    }

    public void FinishTask(int orderID, int itemID, Task task)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].orderID == orderID)
            {
                orders[i].FinishTask(itemID, task);
                break;
            }
        }
    }

    public void OrderCompleted(int orderID)
    {
        Order o = null;
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].orderID == orderID)
            {
                o = orders[i];
                break;
            }
        }
        if (o != null)
        {
            print("OrderCompleted");
            orders.Remove(o);
        }
    }
}

// _________________ O R D E R __________________ \\
public class Order
{
    public List<Item> orderItems;
    public int orderID;

    public Order(Recipe[] _orderItems, int _orderID)
    {
        orderID = _orderID;
        orderItems = new List<Item>();
        foreach (var recipe in _orderItems)
        {
            orderItems.Add(new Item(this, recipe.phases, recipe.GetAllTasksWithPhases()));
        }
    }

    public Tuple<int, int, Task> GetTaskFromOrder()
    {
        Tuple<int, int, Task> r = new Tuple<int, int, Task>(-1, -1, null);
        for (int i = 0; i < orderItems.Count; i++)
        {
            Tuple<int, Task> item = orderItems[i].GetTaskFromItem();
            if (item.Item2 != null)
            {
                r = new Tuple<int, int, Task>(orderID, item.Item1, item.Item2);
                break;
            }
        }
        return r;
    }

    public void FinishTask(int itemID, Task task)
    {
        for (int i = 0; i < orderItems.Count; i++)
        {
            if (orderItems[i].itemID == itemID)
            {
                orderItems[i].FinishTask(task);
            }
        }
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
        pendingTasks = new Dictionary<Task, int>();
        foreach (KeyValuePair<Task, int> pair in _tasks)
        {
            pendingTasks.Add(pair.Key, pair.Value);
        }
        ongoingTasks = new Dictionary<Task, int>();
    }

    public Tuple<int, Task> GetTaskFromItem()
    {
        KeyValuePair<Task, int> r = new KeyValuePair<Task, int>(null, -1);
        foreach (KeyValuePair<Task, int> task in pendingTasks)
        {
            if (task.Value == currentPhase)
            {
                r = task;
                break;
            }
        }

        if (r.Key != null)
        {
            pendingTasks.Remove(r.Key);
            ongoingTasks.Add(r.Key, r.Value);
            return new Tuple<int, Task>(itemID, r.Key);
        }
        return new Tuple<int, Task>(-1, null);
    }

    public bool FinishTask(Task task)
    {
        if (ongoingTasks.ContainsKey(task))
        {
            Debug.Log("task finished!");
            ongoingTasks.Remove(task);
            if (IsPhaseDone())
            {
                currentPhase++;
                if (currentPhase > phases)
                {
                    order.ItemCompleted(this.itemID);
                }
            }
            return true;
        }
        return false;
    }

    public bool IsPhaseDone()
    {
        foreach (var task in pendingTasks)
        {
            if (task.Value == currentPhase)
            {
                return false;
            }
        }
        foreach (var task in ongoingTasks)
        {
            if (task.Value == currentPhase)
            {
                return false;
            }
        }
        return true;
    }
}