namespace PathHinter
{
    public static class F
    {
        /// <summary>
        /// Navigates through the indexer bounds of an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">The array.</param>
        /// <param name="currentIndex">Index of the current.</param>
        /// <param name="up">if set to <c>true</c> [up].</param>
        /// <returns></returns>
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