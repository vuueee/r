
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Aur.Models;

namespace Aur.ViewModels
{
    public class GroupViewModel
    {
        /*
        public int Id { get; set; }

        [Required(ErrorMessage = "Название группы не может быть пустым")]
        [DisplayName("Название")]
        [StringLength(160)]
        public string Title { get; set; }*/

        public Group groupField { get; set; }
        public List<AppUser> AppUsers { get; set; }
        public bool[] MngWrk { get; set; }
    }
}
