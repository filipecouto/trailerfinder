using MovieTrailers.Classes.DataObjects;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MovieTrailers.Classes.DataFetch
{
    /*
    Todo:
    Superclass / Interface that allows us to add more services with same methods.
    Add a parseXML method if (incredibly) a web service of movies does not return in json...
    */
    public class OmdbMovieService
    {
        public string Api
        {
            get
            {
                return "http://www.omdbapi.com/";
            }

            protected set
            {
                Api = value;
            }
        }

        public async Task<OmdbMovieData> searchViaId(OmdbMovieData movieData)
        {
            string result;
            string parameters = "?i=" + movieData._imdbID;
            var client = new HttpClient();
            string requestUri = Api + parameters;
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadAsStringAsync();

            string _plot = getPlot(result);

            movieData._plot = _plot;

            return movieData;
        }

        public async Task<List<OmdbMovieData>> searchTitle(string movieTitle)
        {
            string result;
            string parameters = "?s=" + movieTitle + "&type=movie";
            var client = new HttpClient();
            string requestUri = Api + parameters;
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadAsStringAsync();

            return parseJsonResponse(result);
        }

        public string getPlot(string response)
        {
            string plot = "";

            JavaScriptSerializer JavaScriptSerializerInstance = new JavaScriptSerializer();

            var graph = JavaScriptSerializerInstance.DeserializeObject(response);

            if (graph.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> responseDictionary = (Dictionary<string, object>)graph;

                object _plot;
                responseDictionary.TryGetValue("Plot", out _plot);

                if(_plot.GetType() == typeof(string))
                {
                    plot = (string)_plot;
                }
            }

            return plot;
        }

        /*
        Expected formats from web service http://www.omdbapi.com/?s=movieTitle+type=movie
        {
            Response
            Error
        }
        or
        {
            Search {
                MovieList
            }
        }

        That tells us that, when an error occurs or results are not found, we shall get two top level attributes called Response and Error.
        When results are actually found we get a top level attribute called Search and inside we get a List of Movies.

        Additional information:
            Only 10 results at a time are expected to be returned.
        */
        public List<OmdbMovieData> parseJsonResponse(string response)
        {
            JavaScriptSerializer JavaScriptSerializerInstance = new JavaScriptSerializer();

            var graph = JavaScriptSerializerInstance.DeserializeObject(response);

            List<OmdbMovieData> moviesList = new List<OmdbMovieData>();

            /*
            Let's see if the graph is really a Dictionary<string, object> so we don't get any surprises eh.
            */
            if (graph.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> responseDictionary = (Dictionary<string, object>) graph;

                if(responseDictionary.Count == 1)
                    /* Contains format (let's still check it):
                    * Search: {list of movies}
                    */

                {
                    foreach(KeyValuePair<string, object> topElement in responseDictionary)
                    {
                        string key = topElement.Key.Trim();
                        var value = (object[])topElement.Value;

                        if(key.Equals("Search"))
                        {
                            /*
                            It's pretty clear that we're dealing with the expected case.
                            Let's now create the movie list data.
                            */

                            moviesList = OmdbMovieData.retrieveList(value);
                        }
                    }
                }
            }


            return moviesList;
        }
    }
}