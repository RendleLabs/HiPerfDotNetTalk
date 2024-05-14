namespace OneBRC;

public struct ValueCounter
{
    private string _name;
    private int _count;
    private double _total;
    private double _min;
    private double _max;

    public void SetName(string name)
    {
        _name = name;
    }

    public void Record(double value)
    {
        if (_count == 0)
        {
            _min = _max = value;
        }
        else
        {
            if (value < _min) _min = value;
            if (value > _max) _max = value;
        }

        _total += value;
        ++_count;
    }

    public void Combine(ValueCounter other)
    {
        _count += other._count;
        _total += other._total;
        if (other._min < _min) _min = other._min;
        if (other._max < _max) _max = other._max;
    }

    public double Mean => _total / _count;
    public double Min => _min;
    public double Max => _max;
    public int Count => _count;

    public string Name => _name;
}