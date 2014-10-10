﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TestLibrary.Models;
namespace TestLibrary.ViewModels
{
    public class TopTenViewer
    {
        List<Member> TopMember = new List<Member>();
        List<Book> TopBook = new List<Book>();

        public List<Member> getTopMember()
        {
            return TopMember;
        }

        public void SetTopMember(List<Member> list)
        {
            TopMember = list;
        }

        public List<Book> getTopBook()
        {
            return TopBook;
        }

        public void SetTopBook(List<Book> list)
        {
            TopBook = list;
        }
    }
}