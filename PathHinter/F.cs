namespace PathHinter
{
    public static class F
    {
        public static int NavigateBounds<T>(this T[] arr, ref int currentIndex, bool up)
        {
            if (arr == null || arr.Length <= 1)
                return currentIndex;

            if (up && currentIndex == 0)
                currentIndex = arr.Length - 1;

            if (!up && currentIndex >= arr.Length - 1)
                currentIndex = 0;

            if (up)
                currentIndex--;
            else
                currentIndex++;

            return currentIndex;
        }
    }
}