using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListPool<T>
{
    //Generic List Pool Class ( <T> )
    //T for Template

    //The stack is used to store a collection of pooled lists


    private static Stack<List<T>> stack = new Stack<List<T>>();

    public static List<T> Get()
    {
        //to get a list out of the pool
        //if the stack isn't empty, we'll take the top list and return it
        //otherwise a new list is created
        if (stack.Count > 0)
        {
            return stack.Pop();
        }
        return new List<T>();
    }

    public static void Add(List<T> list)
    {
        //to reuse lists, we have to add them to the pool once we're done with them
        //The list is cleaned and pushed on the stack

        list.Clear();
        stack.Push(list);
    }
}
