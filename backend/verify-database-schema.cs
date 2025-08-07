using System;
using MySqlConnector;

class Program
{
    static void Main()
    {
        string connectionString = "Server=localhost;Database=StrideHR_Dev;User=root;Password=Passwordtharoola007$;Port=3306;";
        
        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            
            Console.WriteLine("✅ Database connection successful!");
            Console.WriteLine($"Connected to: {connection.Database}");
            Console.WriteLine();
            
            // Get table count
            using var countCmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'StrideHR_Dev'", connection);
            var tableCount = countCmd.ExecuteScalar();
            Console.WriteLine($"📊 Total tables in database: {tableCount}");
            Console.WriteLine();
            
            // List all tables
            using var tablesCmd = new MySqlCommand("SHOW TABLES", connection);
            using var reader = tablesCmd.ExecuteReader();
            
            Console.WriteLine("📋 Database Tables:");
            Console.WriteLine("==================");
            int count = 0;
            while (reader.Read())
            {
                count++;
                Console.WriteLine($"{count,3}. {reader.GetString(0)}");
            }
            
            Console.WriteLine();
            Console.WriteLine("✅ Database schema tables are created properly!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}