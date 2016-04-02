using MovieTrailers.Classes.Database;
using MovieTrailers.Classes.DataFetch;
using MovieTrailers.Classes.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MovieTrailers.Classes
{
    public class MovieController : ApiController
    {
        [HttpGet]
        public async Task<List<OmdbYoutubeData>> getMovieData(string title)
        {
            List<OmdbYoutubeData> aggregatedData = new List<OmdbYoutubeData>();

            OmdbMovieService movieService = new OmdbMovieService();

            /*
            Here we are going to search the movies by title.
            We expect a List<OmdbMovieData>, but we don't know yet if it contains movies or not.
            */
            List<OmdbMovieData> movieResults = await movieService.searchTitle(title);

            if(movieResults.Count > 0)
            {
                /*
                There are movies here.

                Now we need to see if we have trailer and plot information for this videos stored
                */

                VideoSQLStorage videoDatabase = new VideoSQLStorage();

                AvailableVideos videos = videoDatabase.areVideosAvailable(movieResults);

                aggregatedData = videos.VideosList;

                YoutubeMovieService youtubeService = new YoutubeMovieService();

                for (var x = 0; x < aggregatedData.Count; x++)
                {
                    OmdbYoutubeData element = aggregatedData.ElementAt(x);

                    if (element.youtubeData.videoId == null)
                    {
                        YoutubeMovieData youtubeData = await youtubeService.searchVideo(element.movieData._title);

                        OmdbMovieData movieData = await movieService.searchViaId(element.movieData);
                        
                        element.youtubeData.videoId = youtubeData.videoId;
                        element.movieData = movieData;
                    }
                }

                if(videos.needSaving)
                {
                    videoDatabase.saveVideos(aggregatedData);
                }
            }

            return aggregatedData;
        }
    }
}
