using System;
using System.Collections.Generic;

public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> heap;

    public PriorityQueue()
    {
        heap = new List<T>();
    }

    public int Count
    {
        get { return heap.Count; }
    }

    public void Enqueue(T item)
    {
        heap.Add(item);
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
        {
            throw new InvalidOperationException("Priority queue is empty.");
        }

        T item = heap[0];
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);

        return item;
    }

    private void HeapifyUp(int index)
    {
        int parent = (index - 1) / 2;

        while (index > 0 && heap[index].CompareTo(heap[parent]) < 0)
        {
            Swap(index, parent);
            index = parent;
            parent = (index - 1) / 2;
        }
    }

    private void HeapifyDown(int index)
    {
        int leftChild = 2 * index + 1;
        int rightChild = 2 * index + 2;
        int smallest = index;

        if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
        {
            smallest = leftChild;
        }

        if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
        {
            smallest = rightChild;
        }

        if (smallest != index)
        {
            Swap(index, smallest);
            HeapifyDown(smallest);
        }
    }

    private void Swap(int i, int j)
    {
        T temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}
