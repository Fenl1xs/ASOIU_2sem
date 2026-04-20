using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

internal abstract class Figure(string type) : IComparable
{
    public string Type { get; } = type;
    public abstract double Area { get; }
    public override string ToString() => $"{Type} площадью {Area}";

    public int CompareTo(object? obj)
    {
        if (obj is Figure other)
            return Area.CompareTo(other.Area);
        throw new ArgumentException("объект не является фигурой");
    }
}

internal class Circle(double radius) : Figure("Круг")
{
    public override double Area => Math.PI * radius * radius;
}

internal class Rectangle(double height, double width, string type = "Прямоугольник") : Figure(type)
{
    public override double Area => width * height;
}

internal class Square(double size) : Rectangle(size, size, "Квадрат")
{
}

public interface IMatrixCheckEmpty<T>
{
    T GetEmptyElement();
    bool CheckEmptyElement(T element);
}

internal class FigureMatrixCheckEmpty : IMatrixCheckEmpty<Figure>
{
    public Figure GetEmptyElement() => null!;
    public bool CheckEmptyElement(Figure element) => element is null;
}

public class Matrix<T>(int maxX, int maxY, IMatrixCheckEmpty<T> checkEmpty)
{
    readonly Dictionary<(int x, int y), T> _matrix = [];

    public T this[int x, int y]
    {
        set
        {
            CheckBounds(x, y);
            _matrix[(x, y)] = value;
        }
        get
        {
            CheckBounds(x, y);
            return _matrix.TryGetValue((x, y), out var element) ? element : checkEmpty.GetEmptyElement();
        }
    }

    void CheckBounds(int x, int y)
    {
        if (x < 0 || x >= maxX)
            throw new ArgumentOutOfRangeException(nameof(x), $"x={x} выходит за границы");
        if (y < 0 || y >= maxY)
            throw new ArgumentOutOfRangeException(nameof(y), $"y={y} выходит за границы");
    }

    public int ColumnWidth { get; set; } = 32;

    public override string ToString()
    {
        var b = new StringBuilder();
        for (int j = 0; j < maxY; j++)
        {
            b.Append('|');
            for (int i = 0; i < maxX; i++)
            {
                string cell = !checkEmpty.CheckEmptyElement(this[i, j]) ? $"{this[i, j]}" : "-";
                b.Append(cell.PadRight(ColumnWidth));
                b.Append('|');
            }
            b.AppendLine();
        }
        return b.ToString();
    }
}

public class SimpleListItem<T>(T data)
{
    public T Data { get; set; } = data;
    public SimpleListItem<T>? Next { get; set; }
}

public class SimpleList<T> : IEnumerable<T> where T : IComparable
{
    protected SimpleListItem<T>? first;
    protected SimpleListItem<T>? last;
    public int Count { get; protected set; }

    public void Add(T element)
    {
        var newItem = new SimpleListItem<T>(element);
        Count++;
        if (last is null)
        {
            first = newItem;
            last = newItem;
        }
        else
        {
            last.Next = newItem;
            last = newItem;
        }
    }

    public SimpleListItem<T> GetItem(int number)
    {
        if (number < 0 || number >= Count)
            throw new IndexOutOfRangeException($"индекс {number} выходит за границы списка");
        var current = first;
        for (int i = 0; i < number; i++)
            current = current!.Next;
        return current!;
    }

    public T Get(int number) => GetItem(number).Data;

    public IEnumerator<T> GetEnumerator()
    {
        var current = first;
        while (current is not null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Sort() => Sort(0, Count - 1);

    private void Sort(int low, int high)
    {
        int i = low;
        int j = high;
        T x = Get((low + high) / 2);
        do
        {
            while (Get(i).CompareTo(x) < 0) ++i;
            while (Get(j).CompareTo(x) > 0) --j;
            if (i <= j)
            {
                Swap(i, j);
                i++;
                j--;
            }
        } while (i <= j);
        if (low < j) Sort(low, j);
        if (i < high) Sort(i, high);
    }

    private void Swap(int i, int j)
    {
        var ci = GetItem(i);
        var cj = GetItem(j);
        (ci.Data, cj.Data) = (cj.Data, ci.Data);
    }
}

class SimpleStack<T> : SimpleList<T> where T : IComparable
{
    public void Push(T element) => Add(element);

    public T Pop()
    {
        T result;
        if (Count == 0)
            return default!;
        else if (Count == 1)
        {
            result = first!.Data;
            first = null;
            last = null;
        }
        else
        {
            var newLast = GetItem(Count - 2);
            result = newLast.Next!.Data;
            last = newLast;
            newLast.Next = null;
        }
        Count--;
        return result;
    }
}

class Program
{
    static void Main()
    {
        Rectangle rect = new(5, 4);
        Square square = new(5);
        Circle circle = new(5);
        Console.WriteLine(rect);
        Console.WriteLine(square);
        Console.WriteLine(circle);

        Console.WriteLine("\nматрица");
        Matrix<Figure> matrix = new(3, 3, new FigureMatrixCheckEmpty());
        matrix[0, 0] = rect;
        matrix[1, 1] = square;
        matrix[2, 2] = circle;
        Console.WriteLine(matrix);

        SimpleList<Figure> list = new();
        list.Add(circle);
        list.Add(rect);
        list.Add(square);
        Console.WriteLine("\nперед сортировкой списка:");
        foreach (var x in list)
            Console.WriteLine(x);
        list.Sort();
        Console.WriteLine("\nпосле сортировки списка:");
        foreach (var x in list)
            Console.WriteLine(x);

        SimpleStack<Figure> stack = new();
        stack.Push(rect);
        stack.Push(square);
        stack.Push(circle);
        Console.WriteLine("\nвывод данных стека:");
        while (stack.Count > 0)
        {
            Figure f = stack.Pop();
            Console.WriteLine(f);
        }
    }
}