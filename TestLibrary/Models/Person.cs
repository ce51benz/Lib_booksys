﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TestLibrary.Models
{
    public abstract class Person
    {
        private string _UserName;
        [MinLength(6,ErrorMessage="Username length must more than 6 characters.")]
        [Required]
        public string UserName { get { return _UserName; } set { _UserName = value; } }

        private string _Name;
        [Required]
        public string Name { get { return _Name; } set { _Name = value; } }


        private string _Password;
        
        [MinLength(8,ErrorMessage="For security,password length must more than 8 characters.")]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get { return _Password; } set { _Password = value; } }

        private string _Email;
        
        [EmailAddress]
        [Required]
        public string Email { get { return _Email; } set { _Email = value; } }

        private int _UserID;
        [Key]
        public int UserID { get { return _UserID; } set { _UserID = value; } }
        public abstract string Identify();
    }
}