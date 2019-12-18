using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentExercisesAPI.Models;
using Microsoft.AspNetCore.Http;


namespace StudentExercisesAPI.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery]int Id, string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    List<Students> students = new List<Students>();
                    // if they are looking for exercises
                    if (include != null)
                    {
                        cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, e.Id AS ExerciseId, e.ExerciseName, e.ProgrammingLanguage, c.CohortName, c.Id AS CoId
                                        FROM Student s
                                        LEFT JOIN Cohort c ON s.CohortId = c.Id
                                        INNER JOIN  StudentExercises se ON se.StudentId = s.Id
                                        INNER JOIN Exercise e ON e.Id = se.ExerciseId
                                        Where 1=1";
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId, c.Id AS CoId, c.CohortName 
                                        FROM Student s
                                        LEFT JOIN  Cohort c ON s.CohortId = c.Id";
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    //List<Exercise> this is where we are storing all of the exercises from the sql statement
                    while (reader.Read())
                    {
                        List<Exercises> exercises = new List<Exercises>();
                        if (include != null)
                        {
                            Exercises exercise = new Exercises
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                ExerciseName = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                ProgrammingLanguage = reader.GetString(reader.GetOrdinal("ProgrammingLanguage"))
                            };
                            exercises.Add(exercise);
                        }
                        Students student = new Students
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohorts()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CoId")),
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                            },
                            //this is considered all of the student exercises they are currently working on
                            Exercises = exercises
                        };
                        students.Add(student);
                    }
                    reader.Close();
                    return Ok(students);
                }
            }
        }
    }
}
