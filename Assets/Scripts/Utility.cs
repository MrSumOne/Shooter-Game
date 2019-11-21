using System.Collections;

public static class Utility
{
    //Fisher-Yates shuffle an array
    //https://youtu.be/q7BL-lboRXo?t=59
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        //prng = psudo random number generator
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;

        }
        return array;
    }
}
