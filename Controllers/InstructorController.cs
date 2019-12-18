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
    public class InstructorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public InstructorController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.Id, i.FirstName, i.LastName, i.SlackHandle, i.Specialty,
                                            i.CohortId, c.CohortName
                                            FROM Instructors i INNER JOIN Cohorts c ON i.CohortId = c.id
                                            WHERE Instructors LIKE q";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Instructors> instructors = new List<Instructors>();

                    while (reader.Read())
                    {


                        Instructors instructor = new Instructors
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohorts()
                            {
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName")),
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId"))
                            }
                        };

                        instructors.Add(instructor);



                    }
                    reader.Close();

                    return Ok(instructors);
                }
            }
        }
    }
}
   