using System.Runtime.CompilerServices;

namespace OneBRC;

public static class FastParse
{
    private const byte Minus = (byte)'-';
    private const byte Point = (byte)'.';
    
    public static float FastParseFloat(ReadOnlySpan<byte> bytes)
    {
        bool negative = bytes[0] == Minus;
        if (negative)
        {
            bytes = bytes[1..];
        }

        int whole = 0;
        int fraction = 0;
        int fractionDigits = 0;
        while (bytes.Length > 0 && bytes[0] != Point)
        {
            whole *= 10;
            whole += FastParseDigit(bytes[0]);
            bytes = bytes[1..];
        }

        if (bytes.Length == 0) return negative ? -whole : whole;

        if (bytes[0] == Point)
        {
            bytes = bytes[1..];
            fractionDigits = bytes.Length;
            while (bytes.Length > 0)
            {
                fraction *= 10;
                fraction += FastParseDigit(bytes[0]);
                bytes = bytes[1..];
            }
        }

        int divisor = fractionDigits switch
        {
            1 => 10,
            2 => 100,
            3 => 1000,
            4 => 10000,
            5 => 100000,
            6 => 1000000,
            7 => 10000000,
            8 => 100000000,
            9 => 1000000000,
            _ => 1
        };
        
        float value = whole + ((float)fraction / divisor);
        return negative ? -value : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastParseDigit(byte b) => b - 48;
}