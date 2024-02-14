using System.Diagnostics;

namespace IntArraySumParallelCalculator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            int[] aSizes = { 100000, 1000000, 10000000 }, // Кол-во элементов в исходном массиве
                  tCounts = { 4, 8, 16, 32 }; // Кол-во потоков для параллельного вычисления
            foreach(int aSize in aSizes)
            {
                // Генерация массива
                List<int> arr = new List<int>();
                for (int i = 0; i < aSize; i++) 
                {
                    arr.Add(new Random().Next());
                }
                stopwatch.Reset();
                stopwatch.Start();
                // Обычное последовательное вычисление суммы элементов массива
                long sum = arr.Sum(_ => (long)_);
                stopwatch.Stop();
                Console.WriteLine($"Сумма {aSize} элементов при последовательном вычислении: {sum}, время выполнения: {stopwatch.ElapsedMilliseconds} мс");
                // Параллельное вычисление суммы элементов массива
                foreach (int tCount in tCounts)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    int subArrSize = (int)(arr.Count / (float)tCount);
                    List<int[]> subArrays = new List<int[]>(); // Подмассивы по числу потоков
                    for (int i = 0; i < tCount; i++)
                    {
                        int[] subArr = new int[subArrSize];
                        arr.CopyTo(i * subArrSize, subArr, 0, subArrSize);
                        subArrays.Add(subArr);
                    }
                    List<Thread> threads = new List<Thread>();
                    sum = 0;
                    foreach(int[] subArr in subArrays)
                    {
                        Thread t = new Thread(() => { Interlocked.Add(ref sum, subArr.Sum(_ => (long)_)); });
                        t.Start();
                        threads.Add(t);
                    }
                    foreach (Thread t in threads)
                        t.Join();
                    stopwatch.Stop();
                    Console.WriteLine($"Сумма {aSize} элементов при параллельном вычислении на {tCount} потоках: {sum}, время выполнения: {stopwatch.ElapsedMilliseconds} мс");

                    stopwatch.Reset();
                    stopwatch.Start();
                    sum = arr.AsParallel().WithDegreeOfParallelism(tCount).Sum(_ => (long)_);
                    stopwatch.Stop();
                    Console.WriteLine($"Сумма {aSize} элементов при параллельном вычислении с помощью LINQ на {tCount} потоках: {sum}, время выполнения: {stopwatch.ElapsedMilliseconds} мс");

                }
                Console.WriteLine($"*****************************************************************************************************************************");
            }
            Console.Write("Press ENTER to exit... ");
            Console.ReadLine();
        }
    }
}
