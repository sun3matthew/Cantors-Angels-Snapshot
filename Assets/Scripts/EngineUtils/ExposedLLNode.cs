
// LinkedListNode but public.
public class ExposedLLNode<T>
{
    public T Value;
    public ExposedLLNode<T> Next;
    public ExposedLLNode<T> Previous;
    public ExposedLLNode(T value, ExposedLLNode<T> next, ExposedLLNode<T> previous)
    {
        Value = value;
        Next = next;
        Previous = previous;
    }

    public static ExposedLLNode<T>[] Merge(ExposedLLNode<T>[] l1, ExposedLLNode<T>[] l2){
        if (l1 == null)
            return l2;
        if (l2 == null)
            return l1;
        l1[1].Next = l2[0];
        l2[0].Previous = l1[1];
        l1[1] = l2[1];
        return l1;
    }
    public static int Length(ExposedLLNode<T> node){
        ExposedLLNode<T> current = node;
        int length = 0;
        while (current != null){
            length++;
            current = current.Next;
        }
        return length;
    }
}