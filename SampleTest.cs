using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

/*
 * In order to setup the database for this test, you'll need MySQL installed
 * Simply run the script in Database and provide 2 values:
 * 1) Username for your root user with DB Create capabilities
 * 2) Password for the above user
 * 
 * Execute that script and it will setup the appropriate database for this test demonstration.
 */

namespace Ivy.Data.MySQL.IntegrationTest
{
    public class SampleTest
    {
        const string ConnectionString = "Data Source=localhost;Initial Catalog=framework_test;Uid=root;Pwd=Password00!;SslMode=none;";
        const string InsertSql = "INSERT INTO testenumentity (`Name`, `FriendlyName`, `SortOrder`) VALUES (@name, @fName, @sort);";
        const string GetSql = "SELECT `THIS`.`Name`, `THIS`.`FriendlyName`, `THIS`.`SortOrder`, `THIS`.`Id` FROM testenumentity `THIS`;";

        [Fact]
        public async Task this_does_not_seem_to_Work()
        {
            // Setup our connection
            var conn = new MySqlConnection(ConnectionString);

            await conn.OpenAsync();

            // Insert our record
            using (var tran = await conn.BeginTransactionAsync())
            {
                await conn.ExecuteAsync(InsertSql, new { name = "Name", fName = "FriendlyName", sort = 1 }, tran);

                tran.Commit();
            }


            /*
             * At this point, the initial insert execution transaction has been closed.

             * SELECT * FROM framework_test.testenumentity;
             * 
             * The above query should properly show that our record has been created
             */


            // Read our record
            using (var tran = await conn.BeginTransactionAsync())
            {
                var reader = await conn.ExecuteReaderAsync(GetSql, null, tran);

                Assert.True(reader.FieldCount > 0);
            }
        }

        [Fact]
        public void this_does_seem_to_Work()
        {
            // Setup our connection
            var conn = new MySqlConnection(ConnectionString);

            conn.Open();

            // Insert our record
            using (var tran = conn.BeginTransaction())
            {
                conn.Execute(InsertSql, new { name = "Name", fName = "FriendlyName", sort = 1 }, tran);

                tran.Commit();
            }


            /*
             * At this point, the initial insert execution transaction has been closed.

             * SELECT * FROM framework_test.testenumentity;
             * 
             * The above query should properly show that our record has been created
             */


            // Read our record
            using (var tran = conn.BeginTransaction())
            {
                var reader = conn.ExecuteReader(GetSql, null, tran);

                Assert.True(reader.FieldCount > 0);
            }
        }

        private async Task ExecuteFromTransactionAsync(MySqlConnection conn, Func<IDbTransaction, Task> dbTranFn)
        {
            using (var tran = await conn.BeginTransactionAsync())
            {
                // Insert our record
                await dbTranFn(tran);

                tran.Commit();
            }
        }
    }
}
