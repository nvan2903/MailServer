using System.Data.SqlClient;

namespace DAL
{
    public class DatabaseConnection
    {
        protected string connString = "Data Source=.;Initial Catalog=Mail;Integrated Security=True";
        protected SqlConnection conn;

        public DatabaseConnection()
        {
            conn = new SqlConnection(connString);
        }

        public void OpenConnection()
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public void CloseConnection()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
}
