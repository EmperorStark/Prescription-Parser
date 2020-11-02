using System;
using System.Collections.Generic;

namespace prescription_parser_service.TaggedResultParser {
    public class Whole {
        List<Day> days  = new List<Day>();

        public Whole(List<SigResponse> taggedResult) {
            days = parseTaggedResult(taggedResult);
        }

        private List<Day> parseTaggedResult(List<SigResponse> taggedResult) {
            List<Day> toReturn = new List<Day>();
            int size = calculateThePeriod(taggedResult); // Calculate how many days

            List<Drug> drugList = extractAllDrugs(taggedResult); // Extract all drug's name, dose, and frequency
            List<DrugTime> drugTimeList = convertDrugWithFreqToTime(drugList); // Convert Drug List to List of Drug Time [DrugTime] 

            int i = 0;
            while(i < size) {
                Day day = new Day(DateTime dateAndTime);
                day.dealWithTheDay(drugTimeList[i]); // Use Drug Time to add drug with dose to morning, afternoon, and evening of the day
                toReturn.Add(day);
                i++;
            }

            return toReturn;
        }

        public int calculateThePeriod(List<SigResponse> taggedResult) {

        }

        public List<Drug> extractAllDrugs(List<SigResponse> taggedResult) {

        }

        public List<DrugTime> convertDrugWithFreqToTime(List<Drug> drugList) {

        } 
    }

    public class Day {
        DateTime date;
        Interval morning;
        Interval afternoon;
        Interval evening;

        public Day() {
            date = null; // the date of this day
            morning = new Interval("morning");
            afternoon = new Interval("afternoon");
            evening = new Interval("evening");
        }

        public void dealWithTheDay(List<DrugTime> drugTimes) {
            
        }
    }

    public class Interval {
        String name;
        List<DrugTime> drugWithTimes;

        public Interval(String name) {
            this.name = name;
        }
    }

    public class DrugTime {
        Drug drug;
        DateTime time;

        public DrugTime(Drug drug, DateTime time) {
            this.drug = drug;
            this.time = 
        }
    }

    public class Drug {
        String name;
        String dose;
        double frequency;

        public Drug(String name, String dose, double frequency) {
            this.name = name;
            this.dose = dose;
            this.frequency = frequency;
        }

    }
}