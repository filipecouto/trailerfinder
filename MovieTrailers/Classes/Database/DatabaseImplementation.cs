using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace MovieTrailers.Classes.Database
{
    public class DatabaseImplementation
    {

        public SqlConnection retrieveConnection()
        {
            SqlConnection connection = new SqlConnection(WebConfigurationManager.ConnectionStrings["MovieTrailersConnectionString"].ConnectionString);

            connection.Open();

            return connection;
        }
    }
}