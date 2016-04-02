using MovieTrailers.Classes.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace MovieTrailers.Classes.DataFetch
{
    public class YoutubeMovieService
    {
        public string Api
        {
            get
            {
                return "https://www.googleapis.com/youtube/v3/search?part=snippet";
            }

            protected set
            {
                Api = value;
            }
        }

        private string ApiKey
        {
            get
            {
                return "&key=AIzaSyDBxcDzpO69hLUh7u1Wu_0mOGnixm5vsAc";
            }
        }

        public async Task<YoutubeMovieData> searchVideo(string movieTitle)
        {
            string result;
            String parameters = "&q=" + HttpUtility.UrlEncode(movieTitle) + " trailer&type=video";
            var client = new HttpClient();
            string requestUri = Api + parameters + ApiKey;
            HttpResponseMessage response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadAsStringAsync();

            return parseJsonResponse(result);
        }

        public YoutubeMovieData parseJsonResponse(string response)
        {
            JavaScriptSerializer JavaScriptSerializerInstance = new JavaScriptSerializer();

            var graph = JavaScriptSerializerInstance.DeserializeObject(response);

            /*
            Let's see if the graph is really a Dictionary<string, object> so we don't get any surprises eh.
            */
            if (graph.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> responseDictionary = (Dictionary<string, object>)graph;

                object _items;
                responseDictionary.TryGetValue("items", out _items);

                if (_items != null && _items.GetType() == typeof(object[]))
                {
                    object[] _itemsArray = (object[])_items;

                    if(_itemsArray.Length > 0 && _itemsArray[0].GetType() == typeof(Dictionary<string, object>)) {
                        Dictionary<string, object> _itemsDictionary = (Dictionary<string, object>)_itemsArray[0];

                        object _id;

                        _itemsDictionary.TryGetValue("id", out _id);

                        if (_id != null && _id.GetType() == typeof(Dictionary<string, object>))
                        {
                            object _videoId;

                            ((Dictionary<string, object>)_id).TryGetValue("videoId", out _videoId);

                            if (_videoId != null && _videoId.GetType() == typeof(string))
                            {
                                YoutubeMovieData newElement = new YoutubeMovieData();
                                newElement.videoId = (string)_videoId;

                                return newElement;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}