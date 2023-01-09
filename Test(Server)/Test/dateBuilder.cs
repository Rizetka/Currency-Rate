using System.ComponentModel.DataAnnotations;

namespace Test
{
    public class dateBuilder
    {
        [Required]
        [Range(1, 9999)]
        public int year { get; set; }
        [Required]
        [Range(1, 12)]
        public int mounth { get; set; }
        [Required]
        [Range(1, 31)]
        public int day { get; set; }

        private bool isValid { get; set; }

        private dateBuilder(int year, int mounth, int day)
        {
            this.year = year;
            this.mounth = mounth;
            this.day = day;
        }
       
        public dateBuilder()
        {

        }

        public void checkDate(int year, int mounth, int day)
        {
            this.year   = year;
            this.mounth = mounth;
            this.day    = day;

            dateBuilder dt = new dateBuilder(year, mounth, day);

            var context = new ValidationContext(dt);
            var results = new List<ValidationResult>();

            //Console.WriteLine("валидуем...");

            //Console.WriteLine(year);
            //Console.WriteLine(mounth);
            //Console.WriteLine(day);

            //Console.WriteLine(!Validator.TryValidateObject(dt, context, results));

            if (!Validator.TryValidateObject(dt, context, results, true))
            {
                //Console.WriteLine("Не удалось создать объект date");
               
                //foreach (var error in results)
                //{
                //    Console.WriteLine(error.ErrorMessage);
                //}

                isValid = false;
            }
            else
            {
                //Console.WriteLine($"Объект date успешно создан. Name: {dt.year} {dt.mounth} {dt.day}");
                isValid = true;
            }
        }

        public bool isCorrect()
        {
            return isValid; 
        }

        public DateTime createDate()
        {
            return new DateTime(year, mounth, day);
        }
    }
}
