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
        [Fact]
        public async Task this_does_not_seem_to_Work()
        {
            // Setup our connection
            var conn = new MySqlConnection("Data Source=localhost;Initial Catalog=framework_test;Uid=root;Pwd=Password00!;SslMode=none;");

            const string insertSql = "INSERT INTO testenumentity (`Name`, `FriendlyName`, `SortOrder`) VALUES (@name, @fName, @sort);";
            const string getSql = "SELECT `THIS`.`Name`, `THIS`.`FriendlyName`, `THIS`.`SortOrder`, `THIS`.`Id` FROM testenumentity `THIS`;";

            await conn.OpenAsync();

            Func<IDbTransaction, Task> insertFn = (IDbTransaction tran) => 
                conn.ExecuteAsync(insertSql, new { name = "Name", fName = "FriendlyName", sort = 1 }, tran);

            // Insert our record
            await ExecuteFromTransactionAsync(conn, insertFn);


            /*
             * At this point, the initial insert execution transaction has been closed.

             * SELECT * FROM framework_test.testenumentity;
             * 
             * The above query should properly show that our record has been created
             */


            // Read our record
            using (var tran = await conn.BeginTransactionAsync())
            {
                var reader = await conn.ExecuteReaderAsync(getSql, null, tran);

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
                tran.Dispose();
            }
        }
    }
}
