﻿using System.ComponentModel.DataAnnotations;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonCreatedResult
    {
        [Required]
        public long PersonId { get; set; }
    }
}