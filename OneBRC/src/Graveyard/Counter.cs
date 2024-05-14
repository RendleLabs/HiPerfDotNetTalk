using System.Buffers;

namespace OneBRC;

public class Counter
{
    private float _total;
    private int _count;
    private float _min = float.MaxValue;
    private float _max = float.MinValue;

    public void Count(float value)
    {
        if (value < _min) _min = value;
        if (value > _max) _max = value;
        _total += value;
        ++_count;
    }

    public float Mean => _total / _count;
    public float Min => _min;
    public float Max => _max;
}