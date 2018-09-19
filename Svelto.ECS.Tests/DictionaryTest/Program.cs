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
            public int i;

            public Test(int i) : this()
            {
                this.i = i;
            }
        }
        static void Main(string[] args)
        {
       //     Tests();
            Profiling();
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
            for (int i = 0; i < dictionarysize; i++) dictionary[numbers[i]] = new Test(i);
            watch.Reset();
            watch.Start();
            for (int i = 0; i < dictionarysize; i++) dictionary[numbers[i]] = new Test(i);
            watch.Stop();
            System.Console.WriteLine(watch.ElapsedMilliseconds);
            for (int i = 0; i < dictionarysize; i++) fasterDictionary[numbers[i]] = new Test(i);
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
                var buffer = fasterDictionary.GetValuesArray(out count);
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

