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
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
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
                    cmd.CommandText = @"SELECT s.Id AS StudentId, 
	                           s.FirstName AS StudentFirstName, 
	                           s.LastName AS StudentLastName, 
                               s.SlackHandle AS StudentSlack, 
                               s.CohortId AS StudentCohortId,
							   c.Id AS CohortId,
	                           c.CohortName,
                               i.FirstName AS InstructorFirstName, 
	                           i.LastName AS InstructorLastName,
                        	   i.SlackHandle AS InstructorSlack,
                               i.CohortId AS InstructorCohortId,
                                i.Specialty,
	                           i.Id AS InstructorId
                                    FROM Cohort c LEFT JOIN Student s ON s.CohortId = c.id
                                    LEFT JOIN Instructor i ON i.CohortId = c.id";
                    SqlDataReader reader = cmd.ExecuteReader();


                    List<Cohorts> cohorts = new List<Cohorts>();

                    while (reader.Read())
                    {
                        Cohorts newCohort = null;
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.Any(c => c.Id == cohortId))
                        {
                            newCohort = new Cohorts()
                            {
                                Id = cohortId,
                                CohortName = reader.GetString(reader.GetOrdinal("CohortName"))
                            };

                            cohorts.Add(newCohort);
                        }

                        Cohorts existingCohort = cohorts.Find(c => c.Id == cohortId);
                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!existingCohort.students.Any(s => s.Id == studentId))
                            {
                                Students newStudent = new Students()
                                {
                                    Id = studentId,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                                    
                                };
                                existingCohort.students.Add(newStudent);
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!existingCohort.instructors.Any(i => i.Id == instructorId))
                            {
                                Instructors newInstructor = new Instructors()
                                {
                                    Id = instructorId,
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortId"))
                                };
                                existingCohort.instructors.Add(newInstructor);
                            };
                        }
                    }
                    reader.Close();

                    return Ok(cohorts);

                }

            }
        }

    }
}
                        