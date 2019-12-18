using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using StudentExercisesAPI.Models;
using Microsoft.AspNetCore.Http;

namespace StudentExercisesAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ExerciseController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string language)
        {
            string sqlStatement = "SELECT Id, ExerciseName, ProgrammingLanguage FROM Exercise";


            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sqlStatement;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    List<Exercises> exercises = new List<Exercises>();

                    while (reader.Read())
                    {
                        Exercises exercise = new Exercises
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            ProgrammingLanguage = reader.GetString(reader.GetOrdinal("ProgrammingLanguage"))
                        };

                        exercises.Add(exercise);
                    }
                    reader.Close();

                    return Ok(exercises);
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetStudentExercises([FromQuery]int Id, string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    List<Exercises> exercises = new List<Exercises>();
                    // if they are looking for exercises
                    if (include != null)
                    {
                        cmd.CommandText = @"SELECT s.FirstName, s.LastName, s.Id, se.Id, se.StudentId, se.ExerciseId, e.ExerciseName, e.Id, e.ProgrammingLanguage
                                            FROM StudentExercises se
                                            LEFT JOIN Student s ON s.Id = se.StudentId
                                            LEFT JOIN Exercise e ON e.Id = se.ExerciseId";
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT se.Id, se.StudentId, se.ExerciseId, e.ExerciseName, e.Id, e.ProgrammingLanguage
                                            FROM StudentExercises se
                                            LEFT JOIN Exercise e ON e.Id = se.ExerciseId";
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Students> students = new List<Students>();
                    //List<Exercise> this is where we are storing all of the exercises from the sql statement
                    while (reader.Read())
                    {
                        
                        if (include != null)
                        {
                            Students student = new Students
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),

                            };
                                students.Add(student);
                        }
                        Exercises exercise = new Exercises
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            ProgrammingLanguage = reader.GetString(reader.GetOrdinal("ProgrammingLanguage"))
                        
                            //this is considered all of the student exercises they are currently working on
                            student = students
                        };
                    exercises.Add(exercise);
                }
                reader.Close();
                return Ok(exercises);
            }
        }
    }

    













    [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
           
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, ExerciseName, ProgrammingLanguage
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Exercises exercise = null;

                    if (reader.Read())
                    {
                        exercise = new Exercises
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                            ProgrammingLanguage = reader.GetString(reader.GetOrdinal("ProgrammingLanguage"))
                        };
                    }
                    reader.Close();

                    return Ok(exercise);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercises exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Exercise (ExerciseName, ProgrammingLanguage)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @language)";
                    cmd.Parameters.Add(new SqlParameter("@name", exercise.ExerciseName));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.ProgrammingLanguage));

                    int newId = (int)await cmd.ExecuteScalarAsync();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercises exercise)
        {
           
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Exercise
                                            SET ExerciseName = @exercisename,
                                                ProgrammingLanguage = @programminglanguage
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@exercisename", exercise.ExerciseName));
                        cmd.Parameters.Add(new SqlParameter("@programminglanguage", exercise.ProgrammingLanguage));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            
            
        


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Exercise WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, ExerciseName, ProgrammingLanguage
                        FROM Exercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}