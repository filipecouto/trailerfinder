using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieTrailers.Classes.DataObjects
{
    public class OmdbMovieData
    {
        public string _title { get; set; }
        public string _year { get; set; }
        public string _imdbID { get; set; }
        public string _type { get; set; }
        public string _poster { get; set; }
        public string _plot { get; set; }
        public bool _needSaving { get; set; }

        public OmdbMovieData()
        {
            _needSaving = false;
        }

        public static List<OmdbMovieData> retrieveList(object[] jsonList)
        {
            List<OmdbMovieData> retrievedList = new List<OmdbMovieData>();

            foreach(object element in jsonList)
            {
                if (element.GetType() == typeof(Dictionary<string, object>))
                {
                    var elementDictionary = (Dictionary<string, object>)element;

                    OmdbMovieData newElement = new OmdbMovieData();

                    object _title;
                    object _year;
                    object _imdbID;
                    object _type;
                    object _poster;

                    elementDictionary.TryGetValue("Title", out _title);
                    elementDictionary.TryGetValue("Year", out _year);
                    elementDictionary.TryGetValue("imdbID", out _imdbID);
                    elementDictionary.TryGetValue("Type", out _type);
                    elementDictionary.TryGetValue("Poster", out _poster);

                    newElement._title = (string) _title;
                    newElement._year = (string) _year;
                    newElement._imdbID = (string) _imdbID;
                    newElement._type = (string) _type;
                    newElement._poster = (string) _poster;

                    retrievedList.Add(newElement);

                }
            }

            return retrievedList;
        }
    }
}