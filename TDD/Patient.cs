using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDD
{
    public class Patient : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "The name should be between 2 & 40 characters.")]
        public String Name { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\d{7,12})$", ErrorMessage = "Not a valid phone number")]
        public String PhoneNumber { get; set; }

        [Required]
        [Range(1, 150)]
        public int Age { get; set; }

        [Required]
        public String Gender { get; set; }

        public Boolean IsAdmitted { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Only Male, Female or Other gender are allowed
            if (Gender.Equals("Male", System.StringComparison.CurrentCultureIgnoreCase) == false &&
                Gender.Equals("Female", System.StringComparison.CurrentCultureIgnoreCase) == false &&
                Gender.Equals("Other", System.StringComparison.CurrentCultureIgnoreCase) == false)
            {
                yield return new ValidationResult("The gender can either be Male, Female or Other");
            }

            yield return ValidationResult.Success;
        }
    }
}