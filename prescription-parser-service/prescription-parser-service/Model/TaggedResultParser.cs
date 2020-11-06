using Microsoft.AspNetCore.Server.IIS.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
// using prescription_parser_service.Controllers;
using static prescription_parser_service.Controllers.SigController;

namespace prescription_parser_service.TaggedResultParser {
    public class Whole {
        public List<DrugTime> days  = new List<DrugTime>();

        public Whole(List<SigResponse> taggedResult) {
            days = parseTaggedResult(taggedResult);
        }

        private List<DrugTime> parseTaggedResult(List<SigResponse> taggedResult) {
            List<DrugTime> toReturn = new List<DrugTime>();
            List<Drug> drugList = extractAllDrugs(taggedResult); // Extract all drug's name, dose, and frequency
            // int size = calculateThePeriod(taggedResult); // Calculate how many days

            List<DrugTime> drugTimeList = convertDrugWithFreqToTime(drugList); // Convert Drug List to List of Drug Time [DrugTime] 

            return toReturn;
        }

        public int calculateThePeriod(List<SigResponse> taggedResult) {
            // Problem: INACCURATE due to algorithmic deficiency
            int maxSize = 1; // default to be a day
            for(int i = 0; i < taggedResult.Count; i++) {
                if(taggedResult[i].Tag.Equals("FOR") && (i+2) <= taggedResult.Count) {
                    if(taggedResult[i+2].Tag.Equals("UnitTime") && (taggedResult[i+1].Tag.Equals("CD") || taggedResult[i+1].Tag.Equals("CDUnit"))) {
                        int cd = 0;
                        if(taggedResult[i+1].Tag.Equals("CD")) {
                            cd = Int32.Parse(taggedResult[i+1].Token); // for 30 days, not thirty days or 40 and a half days
                        } else {
                            EnglishWordToInt ewi = new EnglishWordToInt();
                            cd = ewi.ParseEnglish(taggedResult[i+1].Token); // for thirty days
                        }
                        
                        String timeUnit = taggedResult[i+2].Token.ToLower();
                        switch (timeUnit)
                        {
                            case "day.":
                                if(maxSize < cd) {
                                    maxSize = cd;
                                }
                                break;
                            case "days.":
                                if(maxSize < cd) {
                                    maxSize = cd;
                                }
                                break;
                            case "week.":
                                if(maxSize < cd*7) {
                                    maxSize = cd*7;
                                }
                                break;
                            case "weeks.":
                                if(maxSize < cd*7) {
                                    maxSize = cd*7;
                                }
                                break;
                            case "month.":
                                if(maxSize < cd*31) {
                                    maxSize = cd*31;
                                }
                                break;
                            case "months.":
                                if(maxSize < cd*31) {
                                    maxSize = cd*31;
                                }
                                break;
                            default:
                                Console.WriteLine("Cannot recognize the time unit");
                                break;
                        }
                        i += 3;
                    }
                }
            }
            return maxSize;
        }

        public List<DrugTime> convertDrugWithFreqToTime(List<Drug> drugList)
        {
            List<DrugTime> drugTimes = new List<DrugTime>();
            
            foreach(Drug drug in drugList)
            {
                DateTime now = DateTime.Now;

                String caution = drug.caution;
                double frequency = drug.frequency;
                int count = drug.count;

                while(count > 0)
                {
                    DateTime drugDateTime = calculateTime(now, drug.count, count, frequency, caution);
                    DrugTime drugTime = new DrugTime(drug, drugDateTime);
                    drugTimes.Add(drugTime);
                    count--;
                }
            }

            drugTimes.Sort();

            return drugTimes;
        }
        public List<Drug> extractAllDrugs(List<SigResponse> taggedResult) { // name, dose, disorder, frequency, count ASSUMING ALL INFORMATION WITHIN A PERIOD
            List<Drug> toReturn = new List<Drug>();
            List<List<SigResponse>> sentences = new List<List<SigResponse>>();
            List<SigResponse> sentence = new List<SigResponse>();

            // Separate tagged results to sentences
            for (int i = 0; i < taggedResult.Count; i++) {
                String word = taggedResult[i].Token;
                sentence.Add(taggedResult[i]);
                if (word.EndsWith("."))
                {
                    sentences.Add(sentence);
                    sentence = new List<SigResponse>();
                }
            }

            foreach(List<SigResponse> pres in sentences)
            {
                String name = "";
                String dose = "";
                String disorder = "";
                String caution = "";
                double frequency = 0;
                int count = 0;

                for (int i = 0; i < pres.Count; i++) // Take 1 pillet of med by mouth as needed for anxiety every 2 hours for 10 days
                {
                    String currentTag = pres[i].Tag;
                    // Extract name
                    // Problem: single drug word only, no conditions like "with ..." and warning like "not with ..."
                    if (currentTag.Equals("Unit") && (i + 2) <= pres.Count) // "pillet med"
                    {
                        if(pres[i+1].Tag.Equals("Drug"))
                        {
                            name = pres[i + 1].Token + " ";
                        }
                    }
                    if (currentTag.Equals("Unit") && (i+3) <= pres.Count) // "pillet of med"
                    {
                        if (pres[i + 1].Tag.Equals("OF") && pres[i + 2].Tag.Equals("Drug"))
                        {
                            name = pres[i + 2].Token + " ";
                        }
                    }

                    // Extract dose
                    // Problem: single action word "inject, take, etc."
                    if(currentTag.Equals("VB") && (i+3) <= pres.Count) // a single quantity
                    {
                        if(pres[i+2].Tag.Equals("Unit")) // Not counting quantity like four and a half
                        {
                            dose = "";
                            dose += pres[i].Token + " ";
                            dose += doseNumberHelper(pres[i + 1]) + " ";
                            dose += pres[i + 2].Tag + " ";
                        }
                    }
                    if (currentTag.Equals("VB") && (i + 5) <= pres.Count) // two quantity "one to two"
                    {
                        if (pres[i + 4].Tag.Equals("Unit") && pres[i + 2].Tag.Equals("TO")) // Not counting quantity like four and a half
                        {
                            dose = "";
                            dose += pres[i].Token + " ";
                            dose += doseNumberHelper(pres[i + 1]) + " ";
                            dose += "to" + " ";
                            dose += doseNumberHelper(pres[i + 3]) + " ";
                            dose += pres[i + 2].Tag + " ";
                        }
                    }
                    if(currentTag.Equals("BY") && (i+2) <= pres.Count)
                    {
                        if(pres[i + 1].Tag.Equals("Body"))
                        {
                            dose += (("by " + pres[i+1].Token) + " ");
                        }
                    }
                    if (currentTag.Equals("Route"))
                    {
                        dose += ("by " + pres[i].Token) + " ";
                    }

                    // Extract disorder
                    // Problem: multiple disorders only deal with single words, no compound disorders, only single word for disorder and modifier
                    if (currentTag.Equals("PRN") && (i + 3) <= pres.Count) // assume "PRN" appears always before disorders 
                    {
                        if(pres[i + 1].Tag.Equals("FOR") && pres[i + 2].Tag.Equals("Disorder")) // single disorder "needed for anxiety"
                        {
                            disorder = "";
                            disorder += pres[i + 2].Token + " ";
                        } else if ((pres[i + 1].Tag.Equals("Modifier") || pres[i + 1].Tag.Equals("Body")) &&
                          pres[i + 2].Tag.Equals("Disorder")) // no for condition "needed chest pain" - compound
                        {
                            disorder = "";
                            disorder += pres[i + 1].Token + " ";
                            disorder += pres[i + 2].Token + " ";
                        }
                    }
                    if (currentTag.Equals("PRN") && (i + 4) <= pres.Count)
                    {
                        if (pres[i + 1].Tag.Equals("FOR") && 
                            (pres[i + 2].Tag.Equals("Modifier") || pres[i + 2].Tag.Equals("Body")) &&
                            pres[i + 3].Tag.Equals("Disorder")) // one compound disorder "needed for chest pain"
                        {
                            disorder = "";
                            disorder += pres[i + 2].Token + " ";
                            disorder += pres[i + 3].Token + " ";
                        } else if (pres[i + 1].Tag.Equals("Disorder") &&
                             (pres[i + 2].Tag.Equals("AND") || pres[i + 2].Tag.Equals("OR") || pres[i + 2].Tag.Equals("/")) &&
                             pres[i + 3].Tag.Equals("Disorder")) // two disorders no "FOR" condition 
                        {
                            disorder = "";
                            disorder += pres[i + 1].Token + " ";
                            disorder += pres[i + 2].Token + " ";
                            disorder += pres[i + 3].Token + " ";
                        }
                    }
                    if (currentTag.Equals("PRN") && (i + 5) <= pres.Count) // assume "PRN" appears always before disorders
                    {
                        if(pres[i+1].Tag.Equals("FOR"))
                        {
                            if(pres[i + 2].Tag.Equals("Disorder") && 
                                (pres[i + 3].Tag.Equals("AND") || pres[i + 3].Tag.Equals("OR") || pres[i + 3].Tag.Equals("/")) && 
                                pres[i + 4].Tag.Equals("Disorder")) // "needed for anxiety and/or pain " no compound
                            {
                                disorder = "";
                                disorder += pres[i + 2].Token + " ";
                                disorder += pres[i + 3].Token + " ";
                                disorder += pres[i + 4].Token + " ";
                            }
                        }
                    }

                    // Extract frequency
                    // Problem: "3 times every two weeks"
                    if ((currentTag.Equals("WITH") ||
                        currentTag.Equals("BEFORE") ||
                        currentTag.Equals("DURING") ||
                        currentTag.Equals("AFTER")) && (i + 2) <= pres.Count) // "with lunch"
                    {
                        if (pres[i + 1].Tag.Equals("Meal") || pres[i + 1].Tag.Equals("TimeDay")) // with lunch, before night
                        {
                            caution = (pres[i].Token + " " + pres[i+1].Token + " ");
                            frequency = 0.0;
                            frequency = FrequencyHelper(1, pres[i + 1].Token);
                        }
                    }
                    if (currentTag.Equals("Quant") && (i + 2) <= pres.Count) // "every morning"
                    {
                        if (pres[i + 1].Tag.Equals("UnitTime") || pres[i + 1].Tag.Equals("TimeDay"))
                        {
                            caution = pres[i + 1].Token + " " + pres[i + 1].Token + " ";
                            frequency = FrequencyHelper(1, pres[i + 1].Token);
                        }
                    }
                    if (currentTag.Equals("Frequency") && (i + 2) <= pres.Count) // "twice daily"
                    {
                        if (pres[i + 1].Tag.Equals("Interval"))
                        {
                            int freq = quantHelper(pres[i].Token);
                            frequency = FrequencyHelper(freq, pres[i + 1].Token);
                        }
                    }
                    if (currentTag.Equals("Quant") && (i + 3) <= pres.Count) // "every 2 hours"
                    {
                        if(pres[i+2].Tag.Equals("UnitTime"))
                        {
                            int freq = Int32.Parse(doseNumberHelper(pres[i + 1]));
                            frequency = FrequencyHelper(freq, pres[i + 2].Token);
                        }
                    }
                    if (currentTag.Equals("Frequency") && (i + 3) <= pres.Count) // "twice per week"
                    {
                        if(pres[i+1].Tag.Equals("PER") && pres[i + 2].Tag.Equals("UnitTime"))
                        {
                            int freq = Int32.Parse(doseNumberHelper(pres[i]));
                            frequency = FrequencyHelper(freq, pres[i + 2].Token);
                        }
                    }
                    if ((currentTag.Equals("CD") || currentTag.Equals("CDUnit")) && (i+4) <= pres.Count) // "three times per week"
                    {
                        if(pres[i+1].Tag.Equals("TIMES") &&
                            (pres[i+2].Tag.Equals("PER") || pres[i + 2].Tag.Equals("A")) &&
                            pres[i + 3].Tag.Equals("UnitTime"))
                        {
                            int freq = Int32.Parse(doseNumberHelper(pres[i]));
                            frequency = FrequencyHelper(freq, pres[i + 3].Token);
                        }
                    }

                    // Extract Count
                    // Problem: "for half a month"
                    if (currentTag.Equals("FOR") && (i + 3) <= pres.Count) // for two months
                    {
                        if((pres[i+1].Tag.Equals("CD") || pres[i + 1].Tag.Equals("CDUnit")) && pres[i + 2].Tag.Equals("UnitTime"))
                        {
                            int leng = Int32.Parse(doseNumberHelper(pres[i+1]));
                            int hours = calculateHoursIn(leng, pres[i+2].Token);
                            count = (int) (frequency * hours);
                        }
                    }
                }

                Drug drug = new Drug(name, dose, disorder, caution, frequency, count);
                toReturn.Add(drug);
            }


            return toReturn;
        }

        private String doseNumberHelper(SigResponse sig)
        {
            if (sig.Tag.Equals("CD"))
            {
                return sig.Token;
            }
            else if (sig.Tag.Equals("CDUnit") || sig.Tag.Equals("CDTeen") || sig.Tag.Equals("CDTens"))
            {
                EnglishWordToInt ew = new EnglishWordToInt();
                return ew.ParseEnglish(sig.Token).ToString();
            }
            else if (sig.Tag.Equals("Frac"))
            {
                if (sig.Token.Equals("half"))
                    return "0.5";
            }
            throw new ApplicationException("Unrecognized quantity: " + sig.Token);
        }

        private int quantHelper(String Quant)
        {
            switch(Quant)
            {
                case "every":
                    return 1;
                case "twice":
                    return 2;
                default:
                    throw new ApplicationException("Unrecognized interval: " + Quant);
            }
        }

        private double FrequencyHelper(int freq, String interval)
        {
            double frequency = 0.0;
            String period = interval.ToLower();
            if (period.EndsWith("."))
                period = period.Substring(0, period.Length - 1);

            switch (interval)
            {
                case "hour":
                    frequency = freq;
                    break;
                case "hours":
                    frequency = 1 / freq;
                    break;
                case "hr":
                    frequency = freq;
                    break;
                case "hrs":
                    frequency = 1 / freq;
                    break;
                case "morning":
                    frequency = 1 / 12;
                    break;
                case "noon":
                    frequency = 1 / 12;
                    break;
                case "afternoon":
                    frequency = 1 / 12;
                    break;
                case "evening":
                    frequency = 1 / 12;
                    break;
                case "breakfast":
                    frequency = 1 / 12;
                    break;
                case "lunch":
                    frequency = 1 / 12;
                    break;
                case "brunch":
                    frequency = 1 / 12;
                    break;
                case "dinner":
                    frequency = 1 / 12;
                    break;
                case "night":
                    frequency = 1 / 12;
                    break;
                case "day":
                    frequency = 1 / 24;
                    break;
                case "days":
                    frequency = 1 / (freq * 24);
                    break;
                case "daily":
                    frequency = 1 / 24;
                    break;
                case "week":
                    frequency = 1 / (7 * 24);
                    break;
                case "weeks":
                    frequency = 1 / (freq * 7 * 24);
                    break;
                default:
                    throw new ApplicationException("Unrecognized frequency: " + interval);
            }
            return frequency;
        }
        
        private int calculateHoursIn(int length, String interval)
        {
            String period = interval.ToLower();
            if (period.EndsWith("."))
                period = period.Substring(0, period.Length - 1);
            switch (period)
            {
                case "day":
                    return 1 * 24;
                case "days":
                    return length * 24;
                case "week":
                    return 1 * 24 * 7;
                case "weeks":
                    return length * 24 * 7;
                case "month":
                    return 1 * 24 * 30;
                case "months":
                    return length * 24 * 30;
                default:
                    throw new ApplicationException("Unrecognized hours in: " + interval);
            }
        }
        
        private DateTime calculateTime(DateTime now, int originalCount, int count, double frequency, String caution)
        {
            DateTime timeToTake = now;
            int countDealt = originalCount - count;

            double timeElapsed = 1 / frequency;
            int hourElapsed = (int)timeElapsed;
            int minutesElapsed = (int)((timeElapsed - Math.Truncate(timeElapsed)) * 60);

            hourElapsed = (countDealt * hourElapsed) + (int)(minutesElapsed * countDealt / 60);
            minutesElapsed = (minutesElapsed * countDealt) % 60;

            TimeSpan interval = new TimeSpan(hourElapsed, minutesElapsed, 0);

            return timeToTake.Add(interval);
        }
    }

    public class DrugTime : IComparer<DateTime> {
        public Drug drug { get; set; }
        public DateTime time { get; set; }
        public String interval { get; set; }

        public DrugTime(Drug drug, DateTime time) {
            this.drug = drug;
            this.time = time;
            this.interval = calculateInterval();
        }

        public String calculateInterval() { // 12:00am - 12:00pm Morning; 12:00pm - 6:00pm Afternoon; 6:00pm - 12:00am Evening
            TimeSpan temp = time.TimeOfDay;
            TimeSpan morningStart = new TimeSpan(00, 0, 0); //0 o'clock
            TimeSpan morningEnd = new TimeSpan(12, 0, 0); //12 o'clock
            TimeSpan eveningStart = new TimeSpan(18, 0, 0); //18 o'clock
            TimeSpan eveningEnd = new TimeSpan(24, 0, 0); //24 o'clock

            if ((temp > morningStart) && (temp < morningEnd))
            {
                return "Morning";
            } else if ((temp > morningEnd) && (temp < eveningStart))
            {
                return "Afternoon";
            } else if ((temp > eveningStart) && (temp < eveningEnd))
            {
                return "Evening";
            } else
            {
                throw new ApplicationException("Unrecognized interval: " + drug.name + " and time: " + time.ToString());
            }
        }

        public int CompareTo(object obj)
        {
            return time.CompareTo(obj);
        }

        public int Compare([AllowNull] DateTime x, [AllowNull] DateTime y)
        {
            return x.CompareTo(y);
        }
    }

    public class Drug {
        public String name { get; set; }
        public String dose { get; set; }
        public String disorder { get; set; }
        public String caution { get; set; }
        public double frequency { get; set; }
        public int count { get; set; }

        public Drug(String name, String dose, String disorder, String caution, double frequency, int count) {
            this.name = name;
            this.dose = dose;
            this.disorder = disorder;
            this.caution = caution;
            this.frequency = frequency;
            this.count = count;
        }
    }

    public class EnglishWordToInt {

        public EnglishWordToInt(){}
        public int ParseEnglish(string number) {
            string[] words = number.ToLower().Split(new char[] {' ', '-', ','}, StringSplitOptions.RemoveEmptyEntries);
            string[] ones = {"one", "two", "three", "four", "five", "six", "seven", "eight", "nine"};
            string[] teens = {"eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"};
            string[] tens = {"ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"};
            Dictionary<string, int> modifiers = new Dictionary<string, int>() {
                {"billion", 1000000000},
                {"million", 1000000},
                {"thousand", 1000},
                {"hundred", 100}
            };

            if(number == "eleventy billion")
                return int.MaxValue; // 110,000,000,000 is out of range for an int!

            int result = 0;
            int currentResult = 0;
            int lastModifier = 1;

            foreach(string word in words) {
                if(modifiers.ContainsKey(word)) {
                    lastModifier *= modifiers[word];
                } else {
                    int n;

                    if(lastModifier > 1) {
                        result += currentResult * lastModifier;
                        lastModifier = 1;
                        currentResult = 0;
                    }

                    if((n = Array.IndexOf(ones, word) + 1) > 0) {
                        currentResult += n;
                    } else if((n = Array.IndexOf(teens, word) + 1) > 0) {
                        currentResult += n + 10;
                    } else if((n = Array.IndexOf(tens, word) + 1) > 0) {
                        currentResult += n * 10;
                    } else if(word != "and") {
                        throw new ApplicationException("Unrecognized word: " + word);
                    }
                }
            }

            return result + currentResult * lastModifier;
        }
    }

}