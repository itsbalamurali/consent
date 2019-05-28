﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Identity
{
    using ProducesResponseTypeAttribute = Infrastructure.Web.ProducesResponseTypeAttribute;
    [Route("/identities")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class IdentityController : Controller
    {
        private readonly IdentifierDefinitionRegistry registry;
        private PersonIdentifiersDtosIdentifierDtoMarshaller IdentifierDtoMarshaller { get; }
        private IIdentityRepository IdentityRepository { get; }
        private IUserProvider UserProvider { get; }

        public IdentityController(
            IIdentityRepository identityRepository, 
            IdentifierDefinitionRegistry registry,
            IUserProvider userProvider)
        {
            IdentityRepository = identityRepository;
            UserProvider = userProvider;
            this.registry = registry;
            IdentifierDtoMarshaller = new PersonIdentifiersDtosIdentifierDtoMarshaller(this.registry);
        }


        [Route("{id:int}")]
        [HttpGet]
        [ProducesResponseType(HttpStatusCode.OK, Type=typeof(IEnumerable<IIdentifierValueDto>))]
        [AutoCommit]
        public IActionResult GetPerson(long id)
        {
            return Ok(IdentifierDtoMarshaller.MarshallToDtos(IdentityRepository.GetPersonIdentifiers(id)));
        }

        [HttpPost("search")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(SearchResult))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult FindPerson([FromBody, Required] MatchSpecification[] match)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            
            var person = FindMatchingPerson(match);

            return
                person == null
                    ? (IActionResult) NotFound()
                    : Ok(new SearchResult {PersonId = person});
        }

        private PersonIdentity FindMatchingPerson(IEnumerable<MatchSpecification> match)
        {
            return IdentityRepository.FindPerson(match.Select(_ => IdentifierDtoMarshaller.ConvertToIdentifiers(_.Identifiers)));
        }

        [HttpPut]
        [ProducesResponseType(typeof(PersonCreatedResult), HttpStatusCode.Created)]
        [ProducesResponseType(HttpStatusCode.SeeOther, Type=typeof(PersonCreatedResult))]
        [AutoCommit]
        public IActionResult PutPerson([FromBody, Required]PersonSpecification specification)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var authority = IdentityRepository.GetAuthority(specification.Authority);
            if(authority == null)
                ModelState.AddModelError(nameof(specification.Authority), $"Authority '{specification.Authority}' does not exist");
            ValidateIdentifierTypes(specification.Identifiers, nameof(specification.Identifiers));
            ValidateIdentifierTypes(
                specification.MatchSpecifications.SelectMany(_ => _.Identifiers),
                nameof(specification.MatchSpecifications));
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            //identifierChecker.EnsureHasNoInvalidDuplicates(specification.Identifiers);

            var identifiers = IdentifierDtoMarshaller.ConvertToIdentifiers(specification.Identifiers);
            

            var person = FindMatchingPerson(specification.MatchSpecifications);

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(identifiers, authority);
                return CreatedAtAction("GetPerson", new {id = person.Id}, new PersonCreatedResult {PersonId = person});
            }
            else
            {
                IdentityRepository.UpdatePerson(person, identifiers, authority);

                return new SeeOtherOjectActionResult(
                    "GetPerson",
                    routeValues: new {id = person.Id},
                    result: new PersonCreatedResult {PersonId = person});
            }
        }

        [HttpGet]
        [ProducesResponseType(HttpStatusCode.BadRequest)]
        [ProducesResponseType(HttpStatusCode.NotFound)]
        [ProducesResponseType(HttpStatusCode.OK, Type=typeof(IEnumerable<IIdentifierValueDto>))]
        [AutoCommit]
        public IActionResult GetPersonForAgency(long id, [Required, NotNull]string agencySystemName)
        {
            var agency = IdentityRepository.GetAgency(agencySystemName);
            if(agency == null) ModelState.AddModelError(nameof(agencySystemName), $"{agencySystemName} was not found");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Debug.Assert(agency != null, nameof(agency) + " != null");

            if (!IdentityRepository
                .GetPeopleWithIdentifiers(new[] {new PersonIdentity(id)}, agency.Fields, UserProvider)
                .TryGetValue(new PersonIdentity(id), out var identifiers))
            {
                return NotFound(id);
            }

            return Ok(IdentifierDtoMarshaller.MarshallToDtos(identifiers));
        }


        private void ValidateIdentifierTypes(IEnumerable<IIdentifierValueDto> identifiers, string modelStateName)
        {
            foreach (var identifier in identifiers)
            {
                if (registry.IsValidIdentifierType(identifier)) continue;
                ModelState.AddModelError(
                    modelStateName,
                    $"'{identifier.SystemName}' is not a valid identifier type");
            }
        }
    }
}