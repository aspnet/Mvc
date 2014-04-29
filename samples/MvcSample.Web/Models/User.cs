﻿using System.ComponentModel.DataAnnotations;

namespace MvcSample.Web.Models
{
    public class User
    {
        [Required]
        [MinLength(4)]
        [DisplayFormat(ConvertEmptyStringToNull=true, NullDisplayText="Please enter name")]
        public string Name { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public decimal GPA { get; set; }
        public User Dependent { get; set; }
        public bool Alive { get; set; }
        public string Password { get; set; }
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "Please enter a description")]
        public string About { get; set; }
    }
}