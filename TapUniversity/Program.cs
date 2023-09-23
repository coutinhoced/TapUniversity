using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TapUniversity
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Title.SetTitle("Tap University");

            //STORE ALL THE DETAILS PROVIDED BY THE USER VIA CONSOLE INPUT
            //Stored as key (application type) and value, a Dictionary item (subject lists against its respective values)
            List<KeyValuePair<string, IDictionary<string, int>>> examineeDetailsList = new List<KeyValuePair<string, IDictionary<string, int>>>();

            Subject sbj = new Subject();

            //Initialize a Dictionary based on the All subject values specified in the appSettings JSON file
            IDictionary<string, int> initialDefaultSubjects = sbj.Initialize();

            Console.WriteLine("Please enter the number of examinees");
            int N = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Please enter the type of examinees and scores for each subject and press enter");
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine("Examinee " + (i + 1) + ": ");
                string input = Console.ReadLine();
                string[] inputValues = input.Split(' ');

                //separate out the applicant type variable
                string applicantType = inputValues.FirstOrDefault();

                //separate out the list of subject scores from applicant type variable
                string[] scoreValues = inputValues.Where(o => o != applicantType).ToArray();

                IDictionary<string, int> examineeSubjectScoreDictionary = sbj.AddElements(Array.ConvertAll(scoreValues, item => int.Parse(item)), initialDefaultSubjects);

                //Add mapped values into a list
                examineeDetailsList.Add(new KeyValuePair<string, IDictionary<string, int>>(applicantType, examineeSubjectScoreDictionary));
            }

            //Initialize a list to add to summary grid
            List<string[]> overallSummary = new List<string[]>();
            int passCandidates = sbj.Calculate(examineeDetailsList, out overallSummary);
            Console.WriteLine("\n");
            Console.WriteLine("Number of Passers : " + passCandidates, Console.ForegroundColor = ConsoleColor.Blue);
            Console.ForegroundColor = ConsoleColor.White;

            //Print Summary           
            PrintSummary.PrintLine();
            List<string> gridHeader = new List<string>();
            gridHeader.Add("");
            gridHeader.Add("Division");
            IEnumerable<string> headerSubjectsValues = initialDefaultSubjects.Keys.Select(x => x.ToString());
            gridHeader.AddRange(headerSubjectsValues);
            gridHeader.Add("Applicant Type Sum");
            gridHeader.Add("All Subject");
            gridHeader.Add("Result");
            Console.ForegroundColor = ConsoleColor.Green;
            PrintSummary.PrintRow(gridHeader.ToArray<string>());
            Console.ResetColor();
            PrintSummary.PrintLine();
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var smry in overallSummary)
            {
                PrintSummary.PrintRow(smry);
            }

            Console.ReadLine();
        }
    }

    public class Subject
    {
        public List<string[]> summary = new List<string[]>();
        public List<string> AllSubjects = new List<string>();
        private Dictionary<string, int> subjects = new Dictionary<string, int>();
        public dynamic expandoObject = new System.Dynamic.ExpandoObject();
        public IDictionary<string, int> Initialize()
        {
            string appSettingsDataJson = File.ReadAllText("..\\..\\AppSettings.json");
            //Using ExpandoObject object for dynamically generating JSON structure
            var expandoConverter = new ExpandoObjectConverter();
            expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(
                appSettingsDataJson, expandoConverter);

            VerifyMapping(expandoObject, AllSubjects);

            foreach (var element in expandoObject.AllSubjects)
            {
                subjects.Add(element, 0);
                AllSubjects.Add(element);
            }

            VerifyMapping(expandoObject, AllSubjects);
            return subjects;
        }

        public void VerifyMapping(dynamic expandoObject, List<string> AllSubjects)
        {
            try
            {
                ICollection<KeyValuePair<string, object>> subjectCriteria = (ICollection<KeyValuePair<string, object>>)expandoObject.SubjectCriteria;
                ICollection<KeyValuePair<string, object>> subjectCriteriaRepresentation = (ICollection<KeyValuePair<string, object>>)expandoObject.SubjectCriteriaRepresentation;
                ICollection<KeyValuePair<string, object>> subjectCriteriaSum = (ICollection<KeyValuePair<string, object>>)expandoObject.SubjectCriteriaSum;

                int subjectCriteriaCount = subjectCriteria.Count();
                int subjectCriteriaRepresentationCount = subjectCriteriaRepresentation.Count();
                int subjectCriteriaSumCount = subjectCriteriaSum.Count();


                if ((subjectCriteriaCount != subjectCriteriaRepresentationCount) || (subjectCriteriaCount != subjectCriteriaSumCount))
                {
                    WriteException.PrintException("Issue in Mapping. Please update JSON file accordingly");
                }
            }
            catch (Exception ex)
            {
                WriteException.PrintException(ex.Message);
            }
        }

        /// <summary>
        /// Map elements from console input to elements from JSON 'All Subjects' list to a Dictionary
        /// </summary>
        /// <param name="elements">Input from Console</param>
        /// <param name="initialDefaultSubjects">Dictionary with default values</param>
        /// <returns></returns>
        public IDictionary<string, int> AddElements(int[] elements, IDictionary<string, int> initialDefaultSubjects)
        {
            Dictionary<string, int> examine = new Dictionary<string, int>(initialDefaultSubjects);
            try
            {
                int i = 0;
                foreach (var element in elements)
                {
                    examine[AllSubjects[i]] = element;
                    i++;
                }
            }
            catch (Exception ex)
            {
                WriteException.PrintException(ex.Message);
            }
            return examine;
        }

        /// <summary>
        /// Calculate all the necessay sums dynamically
        /// </summary>
        /// <param name="examineeDetailsList">All the mapped values</param>
        /// <param name="overallSummary">To be used for showing Examinee list in a Grid</param>
        /// <returns></returns>
        public int Calculate(List<KeyValuePair<string, IDictionary<string, int>>> examineeDetailsList, out List<string[]> overallSummary)
        {
            int totalPassCandidates = 0;
            try
            {
                ICollection<KeyValuePair<string, object>> subjectCriteriaRepresentation = (ICollection<KeyValuePair<string, object>>)expandoObject.SubjectCriteriaRepresentation;
                ICollection<KeyValuePair<string, object>> subjectCriteriaList = (ICollection<KeyValuePair<string, object>>)expandoObject.SubjectCriteria;
                ICollection<KeyValuePair<string, object>> subjectCriteriaSum = (ICollection<KeyValuePair<string, object>>)expandoObject.SubjectCriteriaSum;
                int? definedGrandTotal = (int?)expandoObject.GrandTotal;
                int ex = 0;
                string result = string.Empty;

                foreach (KeyValuePair<string, IDictionary<string, int>> element in examineeDetailsList)
                {
                    int totalSum = element.Value.Values.Sum();
                    string subjectCriteria = subjectCriteriaRepresentation.Where(src => src.Value.Equals(element.Key)).FirstOrDefault().Key;
                    IEnumerable<object> subjectCriteriaLists = (IEnumerable<object>)subjectCriteriaList.Where(srl => srl.Key.Equals(subjectCriteria)).FirstOrDefault().Value;
                    IEnumerable<KeyValuePair<string, int>> specificSubjectList = element.Value.Where(e => subjectCriteriaLists.Contains(e.Key));
                    int specificSum = specificSubjectList.Sum(s => s.Value);
                    dynamic definedSpecificSum = subjectCriteriaSum.Where(scs => scs.Key.Equals(subjectCriteria)).FirstOrDefault().Value;

                    if ((totalSum >= definedGrandTotal) && (specificSum >= definedSpecificSum))
                    {
                        totalPassCandidates++;
                        result = "PASS";
                    }
                    else
                    {
                        result = "FAIL";
                    }

                    List<string> individualSummary = new List<string>();
                    individualSummary.Add("Examinee " + (ex++));
                    individualSummary.Add(subjectCriteria);
                    IEnumerable<string> individualSummarySummaryValues = element.Value.Values.Select(x => x.ToString());
                    individualSummary.AddRange(individualSummarySummaryValues);
                    individualSummary.Add(subjectCriteria + " = " + specificSum);
                    individualSummary.Add(totalSum.ToString());
                    individualSummary.Add(result);

                    summary.Add(individualSummary.ToArray<string>());
                }
            }
            catch (Exception ex)
            {
                WriteException.PrintException(ex.Message);
            }
            overallSummary = summary;
            return totalPassCandidates;
        }
    }

    public static class WriteException
    {
        public static void PrintException(string exceptionMessage)
        {
            Console.Beep();
            Console.WriteLine(exceptionMessage, Console.BackgroundColor = ConsoleColor.Red);
            Console.Read();
            Environment.Exit(0);
        }

    }
}
