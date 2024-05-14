using System.Runtime.CompilerServices;

namespace Shared;

public static class FastParse
{
    private const byte Minus = (byte)'-';
    private const byte Point = (byte)'.';
    private const byte NewLine = (byte)'\n';
    
    public static float FastParseFloat(ReadOnlySpan<byte> bytes)
    {
        bool negative = bytes[0] == Minus;
        if (negative)
        {
            bytes = bytes[1..];
        }

        int whole = 0;
        while (bytes.Length > 0)
        {
            if (bytes[0] != Point)
            {
                whole *= 10;
                whole += FastParseDigit(bytes[0]);
            }
            bytes = bytes[1..];
        }

        float value = whole / 1000f;

        return negative ? -value : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastParseDigit(byte b) => b - 48;
    
    public static int FastParseIntFromFloat(in ReadOnlySpan<byte> bytes)
    {
        int sign = 1;
        int value = 0;

        for (int i = 0; i < bytes.Length; i++)
        {
            switch (bytes[i])
            {
                case Minus:
                    sign = -1;
                    break;
                case Point:
                    continue;
                default:
                    value = value * 10 + FastParseDigit(bytes[i]);
                    break;
            }
        }

        return value * sign;
    }

    public static unsafe int FastParseIntFromFloat(ref byte* pointer)
    {
        int sign = 1;
        int value = 0;

        if (*pointer == Minus)
        {
            sign = -1;
            ++pointer;
        }
        
        for (; *pointer != NewLine; ++pointer)
        {
            if (*pointer == Point) continue;
            value = value * 10 + FastParseDigit(*pointer);
        }

        return value * sign;
    }
}