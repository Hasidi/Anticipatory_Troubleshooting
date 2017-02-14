using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticipatoryTroubleShooting
{
    class UsefulFunctions
    {
        public static Random RANDOM = new Random(1);
        //-----------------------------------------------------------------------------------------------------------------------------
        //create sample according to a given distribution- eq. the value that will choose be the one with the most probability
        public static int createSample(double[] distributionArray)
        {
            try
            {
                double[] ranges = new double[distributionArray.Length];
                ranges[0] = distributionArray[0];
                for (int i = 1; i < distributionArray.Length; i++)
                {
                    ranges[i] = distributionArray[i] + ranges[i - 1];
                }
                //Random random = new Random();
                double rand = RANDOM.NextDouble();
                //Console.WriteLine("createSample: " +rand);
                for (int i = 0; i < ranges.Length; i++)
                {
                    if (rand <= ranges[i])
                        return i;
                }
                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------------
        public static double[] excludeFirstFromDistribution(double[] distribution)
        {
            double d = distribution[0];
            double plus = d / ((double)(distribution.Length - 1));
            double[] ans = new double[distribution.Length - 1];
            double check = 0;
            for (int i = 0; i < ans.Length; i++)
            {
                ans[i] = distribution[i + 1] + plus;
                check += distribution[i + 1] + plus;
            }
            if (check != 1)
                Console.WriteLine("Total Prob is not exactly 1");
            return ans;
        }
        //--------------------------------------------------------------------------------------------------------------
        public static int randFromGivenSet(List<int> set)
        {
            int count = set.Count;
            int rand = RANDOM.Next(count);
            //Console.WriteLine("randFromGivenSet int: " +rand);

            return set[rand];
        }
        public static double randFromGivenSet(List<double> set)
        {
            int count = set.Count;
            int rand = RANDOM.Next(count);
            //Console.WriteLine("randFromGivenSet double: " + rand);

            return set[rand];
        }
        //-------------------------------------------------------------------------------------------------------------
        public static int samplingFromDic(Dictionary<int, double> setProbs)
        {
            try
            {
                double sum = 0;
                foreach (var x in setProbs)
                {
                    sum += x.Value;
                }
                Dictionary<int, double> normalizeProbs = new Dictionary<int, double>();
                foreach (var x in setProbs)
                {
                    normalizeProbs.Add(x.Key, (x.Value / sum));
                }
                int chosenPlace = createSample(normalizeProbs.Values.ToArray());
                int chosenVar = setProbs.ElementAt(chosenPlace).Key;
                return chosenVar;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        //--------------------------------------------------------------------------------------------------------------
        public static double randFromSpecificRange(double minRange, double maxRange)
        {
            //Random random = new Random();
            double rand = RANDOM.NextDouble();
            if (rand == 1)
                rand -= 0.01;
            double ans = minRange + (maxRange - minRange) * rand;
            //Console.WriteLine("randFromSpecificRange: " + rand + "__faultTime: " + ans);

            return ans;
        }

        public static int sampleFromSpecificRange(Dictionary<int, double> probs)
        {
            //assumption: 2 elements only in probs
            double maxRange = probs.ElementAt(0).Value + probs.ElementAt(1).Value;

            double rand = randFromSpecificRange(0, maxRange);
            Dictionary<int, double> ranges = new Dictionary<int, double>();
            ranges.Add(probs.ElementAt(0).Key, probs.ElementAt(0).Value);
            ranges.Add(probs.ElementAt(1).Key, maxRange);
            foreach (var x in ranges)
            {
                if (rand <= x.Value)
                    return x.Key;
            }

            return -1;
        }
        //--------------------------------------------------------------------------------------------------------------
        //public static int samplingFromGroup(Dictionary<int, double> setProbs)
        //{
        //    Dictionary<int, int> winsSet = new Dictionary<int, int>();
        //    Dictionary<int, double> currSetProbs = new Dictionary<int, double>();
        //    foreach (var x in setProbs)
        //    {
        //        winsSet.Add(x.Key, 0);
        //        currSetProbs.Add(x.Key, x.Value);
        //    }
        //    while (winsSet.Count > 1)
        //    {
        //        foreach (var x in currSetProbs)
        //        {
        //            foreach (var y in currSetProbs)
        //            {
        //                if (x.Key == y.Key)
        //                    continue;
        //                Dictionary<int, double> currRound = new Dictionary<int, double>();
        //                currRound.Add(x.Key, x.Value);
        //                currRound.Add(y.Key, y.Value);
        //                //int chosen = samplingFromDic(currRound);
        //                int chosen = sampleFromSpecificRange(currRound);
        //                if (chosen == x.Key)
        //                    winsSet[x.Key]++;
        //            }

        //        }
        //        //find maximum
        //        int maxWins = 0;
        //        foreach (var x in winsSet)
        //        {
        //            if (x.Value > maxWins)
        //                maxWins = x.Value;
        //        }
        //        List<int> nextRound = new List<int>();

        //        foreach (var x in winsSet)
        //        {
        //            if (x.Value >= maxWins)
        //                nextRound.Add(x.Key);
        //        }
        //        winsSet = new Dictionary<int, int>();
        //        currSetProbs = new Dictionary<int, double>();
        //        foreach (var x in nextRound)
        //        {
        //            winsSet.Add(x, 0);
        //            currSetProbs.Add(x, setProbs[x]);
        //        }
        //    }
        //    return winsSet.ElementAt(0).Key;
        //}
        //--------------------------------------------------------------------------------------------------------------
        public static void fillEqualDistribution(double[] dis)
        {
            double e = (double)1 / (double)dis.Length;
            for (int i = 0; i < dis.Length; i++)
            {
                dis[i] = e;
            }

        }
        //--------------------------------------------------------------------------------------------------------------
        //public static Dictionary<double, double> normalize(Dictionary<double, double> distribution)
        //{
        //    double denominator = 0;
        //    Dictionary<double, double> ans = new Dictionary<double, double>();

        //    for (int i = 0; i < distribution.Count; i++)
        //    {
        //        denominator += distribution.ElementAt(i).Value;
        //    }
        //    for (int i = 0; i < distribution.Count; i++)
        //    {
        //        KeyValuePair<double, double> currElem = distribution.ElementAt(i);
        //        ans.Add(currElem.Key, currElem.Value / denominator);
        //    }
        //    return ans;

        //}

        //public static void normalize(List<Interval> timeDistribution)
        //{
        //    double denominator = 0;
        //    foreach (var interval in timeDistribution)
        //    {
        //        denominator += interval.faultProb;
        //    }
        //    foreach (var interv in timeDistribution)
        //    {
        //        Interval interval = interv;
        //        interval.faultProb = interval.faultProb / denominator;
        //    }

        //}
        //--------------------------------------------------------------------------------------------------------------

        public static Interval createSample(List<Interval> timeDistribution)
        {
            double[] dis = new double[timeDistribution.Count];
            int c = 0;
            foreach (var interval in timeDistribution)
            {
                dis[c] = interval.faultProb;
                c++;
            }
            int cPlace = createSample(dis);
            Interval ans = timeDistribution.ElementAt(cPlace);
            return ans;
        }




        //--------------------------------------------------------------------------------------------------------------

    }
}
