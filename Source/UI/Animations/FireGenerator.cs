namespace Sanguosha.UI.Animations;

public class FireGenerator
{
    #region Private Members

    private readonly Random r = new();

    #endregion

    #region Constructor

    public FireGenerator(int width, int height)
    {
        Width = width;
        Height = height;

        BaseAlphaChannel = new byte[Width * Height];
        FireData = new byte[Width * Height];
    }

    #endregion

    public byte[] FireData { get; }

    public int Height { get; }

    public int Width { get; }

    public byte[] BaseAlphaChannel { get; }

    #region Private Methods

    private void GenerateBaseline()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int nBytePos = GetBytePos(x, y);
                if (BaseAlphaChannel[nBytePos] != 0)
                {
                    FireData[nBytePos] = GetRandomNumber();
                }
            }
        }
    }

    private int fadeFactor;
    public int FadeFactor
    {
        get => fadeFactor;
        set
        {
            if (fadeFactor is < 0 or > 255) return;
            fadeFactor = value;
        }
    }

    public void UpdateFire()
    {
        GenerateBaseline();

        int centerx = Width / 2;
        int centery = Height / 2;
        int radius = Math.Min(Width / 4, Height / 4);

        for (int y = 0; y < Height - 1; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int leftVal;

                if (x == 0)
                    leftVal = FireData[GetBytePos(Width - 1, y)];
                else
                    leftVal = FireData[GetBytePos(x - 1, y)];

                int rightVal;
                if (x == Width - 1)
                    rightVal = FireData[GetBytePos(0, y)];
                else
                    rightVal = FireData[GetBytePos(x + 1, y)];

                int belowVal = FireData[GetBytePos(x, y + 1)];

                int avg;

                int nBytePos = GetBytePos(x, y);
                if (BaseAlphaChannel[nBytePos] != 0)
                {/*
                    int sum = 0;
                    int totalWeight = 0;
                    if (leftVal != 0) { sum += leftVal; totalWeight++; }
                    if (rightVal != 0) { sum += rightVal; totalWeight++; }
                    if (belowVal != 0) { sum += belowVal * 2; totalWeight += 2; }
                    if (totalWeight == 0) continue;
                    avg = sum / totalWeight; */
                    continue;
                }
                else
                {
                    int sum = leftVal + rightVal + (belowVal * 2);
                    avg = sum / 4;
                }

                // auto reduce it so you get lest of the forced fade and more vibrant fire waves
                if (avg > FadeFactor)
                    avg -= FadeFactor;

                if (avg < 0 || avg > 255)
                    throw new Exception("Average color calc is out of range 0-255");

                FireData[GetBytePos(x, y)] = (byte)avg;
            }
        }
    }

    private byte GetRandomNumber()
    {
        int randomValue = r.Next(2);
        if (randomValue == 0)
            return 0;
        else if (randomValue == 1)
            return 255;
        else
            throw new Exception("Random returned out of bounds");
    }

    private int GetBytePos(int x, int y)
    {
        return (y * Width) + x;
    }

    #endregion
}
