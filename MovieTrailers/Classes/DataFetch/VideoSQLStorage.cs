using MovieTrailers.Classes.Database;
using MovieTrailers.Classes.DataObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace MovieTrailers.Classes.DataFetch
{
    public class AvailableVideos
    {
        public List<OmdbYoutubeData> VideosList;

        public bool needSaving;

        public AvailableVideos()
        {
            needSaving = true;
        }
    }

    public class VideoSQLStorage
    {
        public string Table
        {
            get
            {
                return "movies";
            }
            set
            {
                Table = value;
            }
        }

        public DatabaseImplementation db;

        public VideoSQLStorage()
        {
            db = new DatabaseImplementation();
        }


        public AvailableVideos areVideosAvailable(List<OmdbMovieData> movieData)
        {
            AvailableVideos returnData = new AvailableVideos();
            
            List<OmdbYoutubeData> aggregatedData = new List<OmdbYoutubeData>();

            SqlConnection connection = db.retrieveConnection();

            SqlCommand command = new SqlCommand(null, connection);

            command.CommandText = "SELECT m1.* FROM " + Table + " m1 JOIN (SELECT MAX(tmp.id) id, tmp.imdbID FROM " + Table + " tmp WHERE DATEDIFF(week, tmp.lastUpdated, GETDATE()) = 0 AND tmp.imdbID IN (";

            for(var x = 0; x < movieData.Count; x++)
            {
                OmdbMovieData currentElement = movieData.ElementAt(x);

                command.CommandText += "@imdbID" + x;

                if((x+1) < movieData.Count)
                {
                    command.CommandText += ",";
                }

                SqlParameter imdbIDParam = new SqlParameter("@imdbID" + x, SqlDbType.NVarChar);
                imdbIDParam.Value = currentElement._imdbID;
                command.Parameters.Add(imdbIDParam);
            }

            command.CommandText += ") GROUP BY tmp.imdbID) m2 ON m1.id = m2.id AND m1.imdbID = m2.imdbID ORDER BY m1.id ASC;";

            SqlDataReader reader = command.ExecuteReader();

            /*
            Now I need to see if all the records are available or I need to update some.
            */

            if(reader.HasRows)
            {
                /*
                Fetch information
                */

                while(reader.Read())
                {
                    OmdbYoutubeData newElement = new OmdbYoutubeData();
                    OmdbMovieData omdbData = new OmdbMovieData();
                    YoutubeMovieData youtubeData = new YoutubeMovieData();

                    omdbData._title = reader.GetString(1);
                    omdbData._year = "" + reader.GetInt32(2);
                    omdbData._imdbID = reader.GetString(3);
                    omdbData._type = reader.GetString(4);
                    omdbData._poster = reader.GetString(5);
                    youtubeData.videoId = reader.GetString(6);
                    omdbData._plot = reader.GetString(8);

                    newElement.movieData = omdbData;
                    newElement.youtubeData = youtubeData;

                    aggregatedData.Add(newElement);

                    returnData.needSaving = false;
                }
            }
            else
            {
                /*
                Use current information
                */
                foreach(OmdbMovieData currentElement in movieData)
                {
                    OmdbYoutubeData newElement = new OmdbYoutubeData();
                    newElement.movieData = currentElement;
                    newElement.movieData._needSaving = true;

                    YoutubeMovieData tempYoutubeData = new YoutubeMovieData();
                    tempYoutubeData.videoId = null;
                    newElement.youtubeData = tempYoutubeData;

                    aggregatedData.Add(newElement);

                    returnData.needSaving = true;
                }
            }

            if(!returnData.needSaving)
            {
                foreach(OmdbMovieData currentMovieData in movieData)
                {
                    var _currentImdbID = currentMovieData._imdbID;

                    if (!checkListForImdbId(aggregatedData, _currentImdbID))
                    {
                        OmdbYoutubeData newElement = new OmdbYoutubeData();
                        newElement.movieData = currentMovieData;
                        newElement.movieData._needSaving = true;

                        YoutubeMovieData tempYoutubeData = new YoutubeMovieData();
                        tempYoutubeData.videoId = null;
                        newElement.youtubeData = tempYoutubeData;

                        aggregatedData.Add(newElement);

                        returnData.needSaving = true;
                    }
                }
            }

            connection.Close();

            returnData.VideosList = aggregatedData;

            return returnData;
        }

        private bool checkListForImdbId(List<OmdbYoutubeData> data, string _imdbID)
        {
            foreach(OmdbYoutubeData currentElement in data)
            {
                if (currentElement.movieData._imdbID.Equals(_imdbID))
                {
                    return true;
                }
            }

            return false;
        }

        private List<OmdbYoutubeData> removeUselessData(List<OmdbYoutubeData> data)
        {
            List<OmdbYoutubeData> newData = new List<OmdbYoutubeData>();
            foreach(OmdbYoutubeData currentElement in data)
            {
               if(currentElement.movieData._needSaving)
                {
                    newData.Add(currentElement);
                }
            }
            return newData;
        }

        public void saveVideos(List<OmdbYoutubeData> aggregatedData)
        {
            /*
            Let's remove all the non-needed insertion data
            */
            aggregatedData = removeUselessData(aggregatedData);

            if(aggregatedData.Count == 0)
            {
                return;
            }

            SqlConnection connection = db.retrieveConnection();

            SqlCommand command = new SqlCommand(null, connection);

            command.CommandText = "INSERT INTO " + Table + " (title, year, imdbID, type, poster, youtubeID, plot, lastUpdated) VALUES ";

            for(var x = 0; x < aggregatedData.Count; x++) 
            {
                OmdbYoutubeData currentElement = aggregatedData.ElementAt(x);

                command.CommandText += "(@title" + x + ", @year" + x + ", @imdbID" + x + ", @type" + x + ", @poster" + x + ", @youtubeID" + x + ", @plot" + x + ", GETDATE())";
                if((x+1) < aggregatedData.Count)
                {
                    command.CommandText += ",";
                }

                SqlParameter titleParam = new SqlParameter("@title" + x, SqlDbType.NVarChar, 200);
                SqlParameter yearParam = new SqlParameter("@year" + x, SqlDbType.Int);
                SqlParameter imdbIDParam = new SqlParameter("@imdbID" + x, SqlDbType.NVarChar, 200);
                SqlParameter typeParam = new SqlParameter("@type" + x, SqlDbType.NChar, 10);
                SqlParameter posterParam = new SqlParameter("@poster" + x, SqlDbType.NVarChar, 450);
                SqlParameter youtubeIdParam = new SqlParameter("@youtubeID" + x, SqlDbType.NVarChar, 200);
                SqlParameter plotParam = new SqlParameter("@plot" + x, SqlDbType.NVarChar, 1000);
                titleParam.Value = currentElement.movieData._title;
                yearParam.Value = currentElement.movieData._year;
                imdbIDParam.Value = currentElement.movieData._imdbID;
                typeParam.Value = currentElement.movieData._type;
                posterParam.Value = currentElement.movieData._poster;
                youtubeIdParam.Value = currentElement.youtubeData.videoId;
                plotParam.Value = currentElement.movieData._plot;
                command.Parameters.Add(titleParam);
                command.Parameters.Add(yearParam);
                command.Parameters.Add(imdbIDParam);
                command.Parameters.Add(typeParam);
                command.Parameters.Add(posterParam);
                command.Parameters.Add(youtubeIdParam);
                command.Parameters.Add(plotParam);
            }

            command.Prepare();
            command.ExecuteNonQuery();



            connection.Close();
        }
    }
}