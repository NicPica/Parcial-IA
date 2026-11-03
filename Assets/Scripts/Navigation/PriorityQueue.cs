using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<(T Item, float Priority)> _heap = new();

    public int Count => _heap.Count;

    public void Enqueue(T item, float priority)
    {
        _heap.Add((item, priority));
        int index = _heap.Count - 1;
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (_heap[index].Priority >= _heap[parent].Priority) break;
            Swap(index, parent);
            index = parent;
        }
    }

    public T Dequeue()
    {
        T root = _heap[0].Item;
        int last = _heap.Count - 1;
        _heap[0] = _heap[last];
        _heap.RemoveAt(last);

        int index = 0;
        while (true)
        {
            int left = index * 2 + 1;
            int right = left + 1;
            if (left >= _heap.Count) break;
            int smallest = right < _heap.Count && _heap[right].Priority < _heap[left].Priority ? right : left;
            if (_heap[index].Priority <= _heap[smallest].Priority) break;
            Swap(index, smallest);
            index = smallest;
        }

        return root;
    }

    private void Swap(int a, int b) => (_heap[a], _heap[b]) = (_heap[b], _heap[a]);
}
