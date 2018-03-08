// <auto-generated>
// (C) 2018 CHC  License: TBC
// </auto-generated>

namespace CHC.Consent.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    [Newtonsoft.Json.JsonObject("uk.nhs.bradfordhospitals.bib4all.medway.address")]
    public partial class UkNhsBradfordhospitalsBib4allMedwayAddress : IPersonIdentifier
    {
        /// <summary>
        /// Initializes a new instance of the
        /// UkNhsBradfordhospitalsBib4allMedwayAddress class.
        /// </summary>
        public UkNhsBradfordhospitalsBib4allMedwayAddress()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// UkNhsBradfordhospitalsBib4allMedwayAddress class.
        /// </summary>
        public UkNhsBradfordhospitalsBib4allMedwayAddress(string addressLine1 = default(string), string addressLine2 = default(string), string addressLine3 = default(string), string addressLine4 = default(string), string addressLine5 = default(string), string postcode = default(string))
        {
            AddressLine1 = addressLine1;
            AddressLine2 = addressLine2;
            AddressLine3 = addressLine3;
            AddressLine4 = addressLine4;
            AddressLine5 = addressLine5;
            Postcode = postcode;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "addressLine1")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "addressLine2")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "addressLine3")]
        public string AddressLine3 { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "addressLine4")]
        public string AddressLine4 { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "addressLine5")]
        public string AddressLine5 { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "postcode")]
        public string Postcode { get; set; }

    }
}