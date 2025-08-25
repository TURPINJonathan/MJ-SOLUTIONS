using api.Data;
using api.DTOs;
using api.Helpers;
using api.Enums;
using api.Models;
using api.Services;
using api.Utils;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CompaniesController : BaseController
	{
		private readonly IMapper _mapper;

		public CompaniesController(
				AppDbContext context,
				ILogger<CompaniesController> logger,
				ILogger<FileService> fileLogger,
				IMapper mapper
		) : base(context, logger, fileLogger, mapper)
		{
			_mapper = mapper;
		}

		// GET: api/companies
		[HttpGet]
		public async Task<ActionResult<IEnumerable<CompanyResponseDTO>>> GetCompanies(
			[FromQuery] bool includeDeleted = false,
			[FromQuery] bool includeProspect = false
		)
		{
			var query = _context.Companies
					.Include(c => c.Files)
					.Include(c => c.Projects)
					.Include(c => c.Skills)
					.Include(c => c.Contacts)
					.AsQueryable();

			if (!includeDeleted)
				query = query.Where(c => c.DeletedAt == null);

			if (!includeProspect)
					query = query.Where(c => c.RelationType != CompanyRelationTypeEnum.PROSPECT);

			var companies = await query.ToListAsync();

			var responses = new List<CompanyResponseDTO>();
			foreach (var company in companies)
			{
				var response = _mapper.Map<CompanyResponseDTO>(company);
				await UserUtils.FillModelUsersAsync(_context, _mapper, company, response);
				responses.Add(response);
			}

			_logger.LogInformation($"Consultation de la liste des companies par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
			AuditLogHelper.AddAudit(_context, "Consultation liste companies", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", null);
			await _context.SaveChangesAsync();

			return Ok(responses);
		}

		// GET: api/companies/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<CompanyResponseDTO>> GetCompany(int id)
		{
			var company = await _context.Companies
					.Include(c => c.Files)
					.Include(c => c.Projects)
					.Include(c => c.Skills)
					.Include(c => c.Contacts)
					.FirstOrDefaultAsync(c => c.Id == id);

			if (company == null)
			{
				_logger.LogWarning($"Company id={id} introuvable pour {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec consultation company id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			var response = _mapper.Map<CompanyResponseDTO>(company);
			await UserUtils.FillModelUsersAsync(_context, _mapper, company, response);

			_logger.LogInformation($"Consultation de la company {company.Name} par {ConnectedUserEmail ?? "un visiteur"}");
			AuditLogHelper.AddAudit(_context, $"Consultation company {company.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", company.Id);
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// POST: api/companies
		[HttpPost]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "CREATE_COMPANY")]
		public async Task<ActionResult<CompanyResponseDTO>> CreateCompany([FromForm] CompanyCreateDTO model)
		{
			if (ConnectedUserId == null)
			{
				_logger.LogWarning($"Tentative de création de company sans authentification depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, "Echec création company (utilisateur non authentifié)", "visiteur", ConnectedUserIp ?? "inconnue", "Company", null);
				await _context.SaveChangesAsync();
				return BadRequest("Utilisateur non authentifié.");
			}

			try
			{
				var projects = await _context.Projects.Where(p => model.ProjectIds.Contains(p.Id)).ToListAsync();
				var skills = await _context.Skills.Where(s => model.SkillIds.Contains(s.Id)).ToListAsync();
				var contacts = await _context.Contacts.Where(ct => model.ContactIds.Contains(ct.Id)).ToListAsync();

				var company = new Company
				{
					Name = model.Name,
					Address = model.Address,
					City = model.City,
					Country = model.Country,
					PhoneNumber = model.PhoneNumber,
					Email = model.Email,
					Website = model.Website,
					Description = model.Description,
					Color = model.Color,
					RelationType = model.RelationType,
					ContractStartAt = model.ContractStartAt,
					ContractEndAt = model.ContractEndAt,
					Files = new List<FileResource>(),
					Projects = projects,
					Skills = skills,
					Contacts = contacts,
					CreatedAt = DateUtils.CurrentDateTimeUtils(),
					CreatedById = ConnectedUserId.Value
				};

				_context.Companies.Add(company);
				await _context.SaveChangesAsync();

				var fileService = new FileService(_fileLogger, _context);
				var fileResources = fileService.SaveFilesCompressed(
						model.Files ?? new List<IFormFile>(),
						model.FilesMeta,
						company.Id,
						"Company",
						ConnectedUserEmail,
						ConnectedUserIp
				);

				foreach (var fr in fileResources)
				{
					fr.OwnerId = company.Id;
					fr.OwnerType = "Company";
					_context.FileResources.Add(fr);
					company.Files.Add(fr);
				}
				await _context.SaveChangesAsync();

				var response = _mapper.Map<CompanyResponseDTO>(company);
				await UserUtils.FillModelUsersAsync(_context, _mapper, company, response);

				_logger.LogInformation($"Création de la company {company.Name} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Création company {company.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", company.Id);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erreur lors de la création d'une company par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec création company : {ex.Message}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", null);
				await _context.SaveChangesAsync();
				return StatusCode(500, "Erreur interne du serveur.");
			}
		}

		// PATCH: api/companies/{id}
		[HttpPatch("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "UPDATE_COMPANY")]
		public async Task<ActionResult<CompanyResponseDTO>> PatchCompany(int id, [FromForm] CompanyUpdateDTO model)
		{
			var company = await _context.Companies
					.Include(c => c.Files)
					.Include(c => c.Projects)
					.Include(c => c.Skills)
					.Include(c => c.Contacts)
					.FirstOrDefaultAsync(c => c.Id == id);

			if (company == null)
			{
				_logger.LogWarning($"Tentative de modification d'une company inexistant id={id} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec modification company id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			if (model.Name != null) company.Name = model.Name;
			if (model.Address != null) company.Address = model.Address;
			if (model.City != null) company.City = model.City;
			if (model.Country != null) company.Country = model.Country;
			if (model.PhoneNumber != null) company.PhoneNumber = model.PhoneNumber;
			if (model.Email != null) company.Email = model.Email;
			if (model.Website != null) company.Website = model.Website;
			if (model.Description != null) company.Description = model.Description;
			if (model.Color != null) company.Color = model.Color;
			if (model.RelationType.HasValue) company.RelationType = model.RelationType.Value;
			if (model.ContractStartAt.HasValue) company.ContractStartAt = model.ContractStartAt.Value;
			if (model.ContractEndAt.HasValue) company.ContractEndAt = model.ContractEndAt.Value;
			company.UpdatedAt = DateUtils.CurrentDateTimeUtils();
			company.UpdatedById = ConnectedUserId;

			if (model.ProjectIds != null)
			{
				company.Projects = await _context.Projects.Where(p => model.ProjectIds.Contains(p.Id)).ToListAsync();
			}
			if (model.SkillIds != null)
			{
				company.Skills = await _context.Skills.Where(s => model.SkillIds.Contains(s.Id)).ToListAsync();
			}
			if (model.ContactIds != null)
			{
				company.Contacts = await _context.Contacts.Where(ct => model.ContactIds.Contains(ct.Id)).ToListAsync();
			}

			var fileService = new FileService(_fileLogger, _context);
			var fileResources = fileService.SaveFilesCompressed(
					model.Files ?? new List<IFormFile>(),
					model.FilesMeta,
					company.Id,
					"Company",
					ConnectedUserEmail,
					ConnectedUserIp
			);

			if (company.Files == null)
			{
					company.Files = new List<FileResource>();
			}
			
			foreach (var fr in fileResources)
			{
				fr.OwnerId = company.Id;
				fr.OwnerType = "Company";
				_context.FileResources.Add(fr);
				company.Files.Add(fr);
			}

			_context.Companies.Update(company);
			await _context.SaveChangesAsync();

			var response = _mapper.Map<CompanyResponseDTO>(company);
			await UserUtils.FillModelUsersAsync(_context, _mapper, company, response);

			_logger.LogInformation($"Modification de la company {company.Name} par {ConnectedUserEmail ?? "un visiteur"}");
			AuditLogHelper.AddAudit(_context, $"Modification company {company.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", company.Id);
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// DELETE: api/companies/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "DELETE_COMPANY")]
		public async Task<IActionResult> DeleteCompany(int id)
		{
			var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);

			if (company == null)
			{
				_logger.LogWarning($"Tentative de suppression d'une company inexistant id={id} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec suppression company id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			company.DeletedAt = DateUtils.CurrentDateTimeUtils();
			company.DeletedById = ConnectedUserId;

			_context.Companies.Update(company);
			await _context.SaveChangesAsync();

			_logger.LogInformation($"Suppression (soft) de la company {company.Name} par {ConnectedUserEmail ?? "un visiteur"}");
			AuditLogHelper.AddAudit(_context, $"Suppression (soft) company {company.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Company", company.Id);
			await _context.SaveChangesAsync();

			return NoContent();
		}

	}
		
}