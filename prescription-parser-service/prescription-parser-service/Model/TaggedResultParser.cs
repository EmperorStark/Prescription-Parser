using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using prescription_parser_service.Controllers;

namespace prescription_parser_service.TaggedResultParser {
    public class Whole {
        public List<DrugTime> days  = new List<DrugTime>();

        public Whole(List<SigResponse> taggedResult) {
            days = parseTaggedResult(taggedResult);
        }

        private List<DrugTime> parseTaggedResult(List<SigResponse> taggedResult) {
            List<DrugTime> toReturn = new List<DrugTime>();
            int size = calculateThePeriod(taggedResult); // Calculate how many days

            List<Drug> drugList = extractAllDrugs(taggedResult); // Extract all drug's name, dose, and frequency
            List<DrugTime> drugTimeList = convertDrugWithFreqToTime(drugList); // Convert Drug List to List of Drug Time [DrugTime] 

            for(int i = 0; i < size; i++) {
                day.dealWithTheDay(drugTimeList[i]); // Use Drug Time to add drug with dose to morning, afternoon, and evening of the day
                toReturn.Add(day);
                i++;
            }

            return toReturn;
        }

        public int calculateThePeriod(List<SigResponse> taggedResult) {
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
                            case "day":
                                if(maxSize < cd) {
                                    maxSize = cd;
                                }
                                break;
                            case "days":
                                if(maxSize < cd) {
                                    maxSize = cd;
                                }
                                break;
                            case "week":
                                if(maxSize < cd*7) {
                                    maxSize = cd*7;
                                }
                                break;
                            case "weeks":
                                if(maxSize < cd*7) {
                                    maxSize = cd*7;
                                }
                                break;
                            case "month":
                                if(maxSize < cd*31) {
                                    maxSize = cd*31;
                                }
                                break;
                            case "months":
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

        public List<Drug> extractAllDrugs(List<SigResponse> taggedResult) {

        }

        public List<DrugTime> convertDrugWithFreqToTime(List<Drug> drugList) {

        }
    }

    public class DrugTime {
        Drug drug;
        DateTime time;
        String interval;

        public DrugTime(Drug drug, DateTime time) {
            this.drug = drug;
            this.time = calculateTime();
            this.interval = calculateInterval();
        }

        public DateTime calculate

        public String calculateInterval() { // 12:00am - 12:00pm Morning; 12:00pm - 6:00pm Afternoon; 6:00pm - 12:00am Evening

        }
    }

    public class Drug {
        String name;
        String dose;
        double frequency;
        int times;

        public Drug(String name, String dose, double frequency, int times) {
            this.name = name;
            this.dose = dose;
            this.frequency = frequency;
            this.times = times;
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