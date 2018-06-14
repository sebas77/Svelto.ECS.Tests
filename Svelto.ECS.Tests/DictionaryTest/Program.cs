using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;

namespace Svelto.DataStructures
{

    class Program
    {
        struct Test
        {
            public int v;
            private int c;
            private int v1;
            private int c1;
            private int v2;
            private int c2;
            public int i;

            public Test(int i) : this()
            {
                this.i = i;
            }
        }
        static void Main(string[] args)
        {
        //    Tests();
            Profiling();
        }

        static void Tests()
        {
            FasterDictionary<int, Test> test = new FasterDictionary<int, Test>();

            int dictionarysize = 10000000;

            int[] numbers = new int[dictionarysize];

            for (int i = 1; i < dictionarysize; i++)
                numbers[i] = (numbers[i - 1] + i * HashHelpers.ExpandPrime(dictionarysize));

            for (int i = 0; i < dictionarysize; i++)
                test[i] = new Test(numbers[i]);

            for (int i = 0; i < dictionarysize; i++)
                if (test[i].i != numbers[i])
                    throw new Exception();

            System.Console.WriteLine("test passed");

            for (int i = 0; i < dictionarysize; i += 2)
                if (test.Remove(i) == false)
                    throw new Exception();

            System.Console.WriteLine("test passed");

            for (int i = 1; i < dictionarysize; i += 2)
                if (test[i].i != numbers[i])
                    throw new Exception();

            System.Console.WriteLine("test passed");


            for (int i = 0; i < dictionarysize; i++)
                test[i] = new Test(numbers[i]);

            for (int i = 0; i < dictionarysize; i++)
                if (test[i].i != numbers[i])
                    throw new Exception();

            System.Console.WriteLine("test passed");

            for (int i = dictionarysize - 1; i >= 0; i -= 3)
                if (test.Remove(i) == false)
                    throw new Exception();

            System.Console.WriteLine("test passed");

            for (int i = dictionarysize - 1; i >= 0; i -= 3)
                test[i] = new Test(numbers[i]);

            System.Console.WriteLine("test passed");

            for (int i = 0; i < dictionarysize; i++)
                if (test[i].i != numbers[i])
                    throw new Exception();

            System.Console.WriteLine("test passed");

            for (int i = 0; i < dictionarysize; i ++)
                if (test.Remove(i) == false)
                    throw new Exception();

            System.Console.WriteLine("test passed");

            for (int i = 0; i < dictionarysize; i++)
                if (test.Remove(i) == true)
                    throw new Exception();

            System.Console.WriteLine("test passed");

            test.Clear();
            for (int i = 0; i < dictionarysize; i++) test[numbers[i]] = new Test(i);

            for (int i = 0; i < dictionarysize; i++)
            {
                Test JapaneseCalendar = test[numbers[i]];
                if (JapaneseCalendar.i != i)
                    throw new Exception("read back test failed");
            }

            System.Console.WriteLine("test passed");

            test = new FasterDictionary<int, Test>();
            for (int i = 0; i < dictionarysize; i++) test[numbers[i]] = new Test(i);

            for (int i = 0; i < dictionarysize; i++)
            {
                Test JapaneseCalendar = test[numbers[i]];
                if (JapaneseCalendar.i != i)
                    throw new Exception("read back test failed");
            }

            System.Console.WriteLine("test passed");
        }

        private static void Profiling()
        {
            int dictionarysize = 10000000;

            int[] numbers = new int[dictionarysize];
            var r = new Random();
            for (int i = 0; i < dictionarysize; i++) numbers[i] = r.Next();

            FasterDictionary<int, Test> fasterDictionary = new FasterDictionary<int, Test>();
            Dictionary<int, Test> dictionary = new Dictionary<int, Test>();

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

            System.Console.WriteLine("insert");
            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) dictionary[numbers[i]] = new Test(i);
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) fasterDictionary[numbers[i]] = new Test(i); 
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);
/*
            fasterDictionary = new FasterDictionary<int, Test>();
            dictionary = new Dictionary<int, Test>();
            System.Console.WriteLine("add after new");
            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) dictionary.Add(numbers[i], new Test(i));
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) fasterDictionary.Add(numbers[i], new Test(i));
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);
            */
            fasterDictionary = new FasterDictionary<int, Test>(dictionarysize);
            dictionary = new Dictionary<int, Test>();
            System.Console.WriteLine("insert after new with presize");
            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) dictionary[numbers[i]] = new Test(i);
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) fasterDictionary[numbers[i]] = new Test(i);
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);

            dictionary.Clear();
            fasterDictionary.Clear();
            System.Console.WriteLine("insert after clear");
            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) dictionary[numbers[i]] = new Test(i);
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) fasterDictionary[numbers[i]] = new Test(i);
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);

            System.Console.WriteLine("read");

            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++)
            {
                Test JapaneseCalendar;
                JapaneseCalendar = dictionary[numbers[i]];
            }

            watch.Stop();

            System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++)
            {
                Test JapaneseCalendar;
                JapaneseCalendar = fasterDictionary[numbers[i]];
            }

            watch.Stop();

            System.Console.WriteLine(watch.ElapsedMilliseconds);

            System.Console.WriteLine("iterate values");

            watch.Reset();
            watch.Start();
            for (int i = 0; i < 1; i++)
            {
                Test JapaneseCalendar;
                foreach (var VARIABLE in dictionary.Values)
                {
                    JapaneseCalendar = VARIABLE;
                }
            }

            watch.Stop();

            System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
            watch.Start();
            for (int i = 0; i < 1; i++)
            {
                Test JapaneseCalendar;
                int count;
                var buffer = fasterDictionary.GetFasterValuesBuffer(out count);
                for (int j = 0; j < count; j++)
                {
                    JapaneseCalendar = buffer[j];
                }
            }

            watch.Stop();

            System.Console.WriteLine(watch.ElapsedMilliseconds);

            System.Console.WriteLine("remove");
            watch.Reset();
                        watch.Start();
                        for (int i = 0; i < dictionarysize; i++)
                        {
                            dictionary.Remove(numbers[i]);
                        }

                        watch.Stop();

                        System.Console.WriteLine(watch.ElapsedMilliseconds);

            watch.Reset();
                        watch.Start();
                        for (int i = 0; i < dictionarysize; i++)
                        {
                            fasterDictionary.Remove(numbers[i]);
                        }

                        watch.Stop();

            System.Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}

