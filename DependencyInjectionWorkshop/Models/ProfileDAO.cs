using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDAO
    {
        public string GetPasswordFromDb(string accountId)
        {
            var passwordFromDB = string.Empty;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDB = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDB;
        }
    }
}