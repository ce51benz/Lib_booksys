using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ParatabLib.Models
{
    //This class is representation of Librarian which inherit from Person class.
    public class Librarian:Person
    {

        public override string Identify()
        {
            return "Librarian "+this.UserName;
        }

        public List<News> getAnnoucedNews()
        {
            ParatabLib.DataAccess.LibraryRepository libRepo = new ParatabLib.DataAccess.LibraryRepository();
            return libRepo.NewsRepo.ListWhere(target => ParatabLib.Utilities.IntUtil.isEqual(target.AncID, UserID));
        }
    }
}