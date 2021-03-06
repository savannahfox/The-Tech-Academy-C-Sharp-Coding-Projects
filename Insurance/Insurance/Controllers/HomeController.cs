﻿using Insurance.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Insurance.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = @"Data Source=MYDELL-PC\SQLEXPRESS;Initial Catalog=Insurance;
                                            Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;
                                            ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Application(string firstName, string lastName, string emailAddress, string dateOfBirth, string carYear,
                                        string carMake, string carModel, string dUI, string speedingTickets, string coverageType)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(emailAddress) ||
                string.IsNullOrEmpty(dateOfBirth) || string.IsNullOrEmpty(carYear) || string.IsNullOrEmpty(carMake) ||
                string.IsNullOrEmpty(carModel) || string.IsNullOrEmpty(dUI) || string.IsNullOrEmpty(speedingTickets) ||
                string.IsNullOrEmpty(coverageType))
            {
                return View("~/Shared/Error.cshtml");
            }
            else
            {
                string queryString = @"INSERT INTO Users (FirstName, LastName, EmailAddress, DateOfBirth, CarYear, CarMake, CarModel, Dui, SpeedingTickets, CoverageType, Quote) VALUES 
                                        (@FirstName, @LastName, @EmailAddress, @DateOfBirth, @CarYear, @CarMake, @CarModel, @Dui, @SpeedingTickets, @CoverageType, @Quote)";

                var Quote = 50;

                //Age-related fees
                DateTime DOB = Convert.ToDateTime(dateOfBirth);
                TimeSpan age = DateTime.Now - DOB;
                int applicantAge = Convert.ToInt32(age.Days / 365.25);

                if (applicantAge < 25)
                {
                    Quote += 25;
                }
                else if (applicantAge < 18)
                {
                    Quote += 100;
                }

                int applicantCar = Convert.ToInt32(carYear);

                //Car-related fees
                if (applicantCar < 2000)
                {
                    Quote += 25;
                }

                if (applicantCar > 2015)
                {
                    Quote += 25;
                }

                if (carMake.ToLower() == "porsche")
                {
                    Quote += 25;
                }

                if (carMake.ToLower() == "porsche" && carModel.ToLower() == "911 carrera")
                {
                    Quote += 50;
                }

                //Driving habit fees
                int speedingFee = 10 * Convert.ToInt32(speedingTickets);
                Quote += speedingFee;

                if (dUI.ToLower() == "yes" || dUI.ToLower() == "yea" || dUI.ToLower() == "yeah")
                {
                    Quote += (Quote / 4);
                }

                if (coverageType.ToLower() == "full coverage" || coverageType.ToLower() == "full-coverage" || coverageType.ToLower() == "full")
                {
                    Quote += (Quote / 2);
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.Add("@FirstName", SqlDbType.VarChar);
                    command.Parameters.Add("@LastName", SqlDbType.VarChar);
                    command.Parameters.Add("@EmailAddress", SqlDbType.VarChar);
                    command.Parameters.Add("@DateOfBirth", SqlDbType.Date);
                    command.Parameters.Add("@CarYear", SqlDbType.Int);
                    command.Parameters.Add("@CarMake", SqlDbType.VarChar);
                    command.Parameters.Add("@CarModel", SqlDbType.VarChar);
                    command.Parameters.Add("@Dui", SqlDbType.VarChar);
                    command.Parameters.Add("@SpeedingTickets", SqlDbType.Int);
                    command.Parameters.Add("@CoverageType", SqlDbType.VarChar);
                    command.Parameters.Add("@Quote", SqlDbType.Money);

                    command.Parameters["@FirstName"].Value = firstName;
                    command.Parameters["@LastName"].Value = lastName;
                    command.Parameters["@EmailAddress"].Value = emailAddress;
                    command.Parameters["@DateOfBirth"].Value = dateOfBirth;
                    command.Parameters["@CarYear"].Value = Convert.ToInt32(carYear);
                    command.Parameters["@CarMake"].Value = carMake;
                    command.Parameters["@CarModel"].Value = carModel;
                    command.Parameters["@Dui"].Value = dUI;
                    command.Parameters["@SpeedingTickets"].Value = Convert.ToInt32(speedingTickets);
                    command.Parameters["@CoverageType"].Value = coverageType;
                    command.Parameters["@Quote"].Value = Quote;

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }


                return View("Quote");
            }
        }

        public ActionResult Admin()
        {
            string queryString = @"SELECT FirstName, LastName, EmailAddress, Quote from Users";
            List<Applicant> applicants = new List<Applicant>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var applicant = new Applicant();
                    applicant.FirstName = reader["FirstName"].ToString();
                    applicant.LastName = reader["LastName"].ToString();
                    applicant.EmailAddress = reader["EmailAddress"].ToString();
                    applicant.Quote = Convert.ToDecimal(reader["Quote"]);
                    applicants.Add(applicant);
                }
            }
            return View(applicants);

        }
    }

}