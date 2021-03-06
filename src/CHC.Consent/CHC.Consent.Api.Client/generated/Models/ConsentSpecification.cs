// <auto-generated>
// (C) 2018 CHC  License: TBC
// </auto-generated>

namespace CHC.Consent.Api.Client.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class ConsentSpecification
    {
        /// <summary>
        /// Initializes a new instance of the ConsentSpecification class.
        /// </summary>
        public ConsentSpecification()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ConsentSpecification class.
        /// </summary>
        public ConsentSpecification(long studyId, string subjectIdentifier, long personId, System.DateTime dateGiven, IList<IIdentifierValueDto> evidence, long? givenBy = default(long?))
        {
            StudyId = studyId;
            SubjectIdentifier = subjectIdentifier;
            PersonId = personId;
            DateGiven = dateGiven;
            Evidence = evidence;
            GivenBy = givenBy;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "studyId")]
        public long StudyId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "subjectIdentifier")]
        public string SubjectIdentifier { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "personId")]
        public long PersonId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "dateGiven")]
        public System.DateTime DateGiven { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "evidence")]
        public IList<IIdentifierValueDto> Evidence { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "givenBy")]
        public long? GivenBy { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (SubjectIdentifier == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "SubjectIdentifier");
            }
            if (Evidence == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Evidence");
            }
        }
    }
}
