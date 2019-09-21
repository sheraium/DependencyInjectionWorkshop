using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }

    public class ProfileDAO : IProfile
    {
        public string GetPassword(string accountId)
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