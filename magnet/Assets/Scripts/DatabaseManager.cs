// using UnityEngine;
// using SQLite;
// using System.IO;


// public class Scores
// {
//     [PrimaryKey, AutoIncrement]
//     public int Id { get; set; }

//     public string PlayerName { get; set; }
//     public int Score { get; set; }
//     public float CompletionTime { get; set; }
// }
// public class DatabaseManager : MonoBehaviour
// {
//     public static DatabaseManager Instance { get; private set; }

//     private string dbPath;
//     private SQLiteConnection dbConnection;

//     void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//         DontDestroyOnLoad(gameObject);

//         SetDatabasePath();
//         InitializeDatabase();
//     }

//     void SetDatabasePath()
//     {
//         dbPath = Path.Combine(Application.persistentDataPath, "gamedata.db");
//     }

//     void InitializeDatabase()
//     {
//         dbConnection = new SQLiteConnection(dbPath);
//         CreateScoresTable();
//     }

//     void CreateScoresTable()
//     {
//         dbConnection.CreateTable<Scores>();
//         Debug.Log("Scores table created at: " + dbPath);
//     }
// }