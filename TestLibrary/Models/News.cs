﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TestLibrary.Models
{
    public class News
    {

        public int NewsID { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Detail { get; set; }

        
        public DateTime PostTime{get; set;}
    }
}