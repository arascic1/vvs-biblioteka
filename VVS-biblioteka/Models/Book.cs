using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VVS_biblioteka.Models
{
    public class Book
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id {  get; set; }

        public string Title { get; set; }
        public string Author {  get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
    }
}
