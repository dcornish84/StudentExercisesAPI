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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, 
                                       s.CohortId, c.CohortName,
                                       se.ExerciseId, e.ExerciseName, e.ProgrammingLanguage
                                  FROM Student s INNER JOIN Cohort c ON s.CohortId = c.id
                                       LEFT JOIN StudentExercises se on se.StudentId = s.id
                                       LEFT JOIN Exercise e on se.ExerciseId = e.Id";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Students> students = new List<Students>();

                    while (reader.Read())
                    {


                            Students student = new Students
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohorts()
                                {
                                    CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortId"))
                                }
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
