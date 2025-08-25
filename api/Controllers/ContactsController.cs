using api.Data;
using api.DTOs;
using api.Helpers;
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
	public class ContactsController : BaseController
	{
		private readonly IMapper _mapper;

		public ContactsController(
			AppDbContext context,
			ILogger<ContactsController> logger,
			ILogger<FileService> fileLogger,
			IMapper mapper
		) : base(context, logger, fileLogger, mapper)
		{
			_mapper = mapper;
		}

		// GET api/contacts
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ContactResponseDTO>>> GetContacts(
			[FromQuery] bool includeDeleted = false
		)
		{
			var query = _context.Contacts
				.Include(c => c.Files)
				.AsQueryable();

			if (!includeDeleted)
			{
				query = query.Where(c => c.DeletedAt == null);
			}

			var contacts = await query.ToListAsync();

			var responses = new List<ContactResponseDTO>();
			foreach (var contact in contacts)
			{
				var response = _mapper.Map<ContactResponseDTO>(contact);
				responses.Add(response);
			}

			_logger.LogInformation($"Liste des contacts consultée par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
			AuditLogHelper.AddAudit(_context, "Consultation liste des contacts", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Contact", null);
			await _context.SaveChangesAsync();

			return Ok(responses);
		}

		// GET: api/contacts/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<IEnumerable<ContactResponseDTO>>> GetContact(int id)
		{
			var contact = await _context.Contacts
				.Include(c => c.Files)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (contact == null)
			{
				_logger.LogWarning($"Contact ID {id} introuvable pour {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Echec consultation contact ID {id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Contact", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			var response = _mapper.Map<ContactResponseDTO>(contact);

			_logger.LogInformation($"Consultation du contact {contact.FirstName} {contact.LastName} par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
			AuditLogHelper.AddAudit(_context, $"Consultation contact {contact.FirstName} {contact.LastName}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Contact", contact.Id);
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// POST: api/contacts
		[HttpPost]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "CREATE_CONTACT")]
		public async Task<ActionResult> CreateContact([FromForm] ContactCreateDTO model)
		{
			if (ConnectedUserId == null)
			{
				_logger.LogWarning($"Tentative de création d'un contact sans authentification depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, "Echec création contact (utilisateur non authentifié)", "visiteur", ConnectedUserIp ?? "inconnue", "Contact", null);
				await _context.SaveChangesAsync();
				return BadRequest("Utilisateur non authentifié");
			}

			try
			{
				var contact = new Contact
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					Email = model.Email,
					PhoneNumber = model.PhoneNumber,
					Position = model.Position,
					Note = model.Note,
					Files = new List<FileResource>(),
					CreatedAt = DateUtils.CurrentDateTimeUtils()
				};

				_context.Contacts.Add(contact);
				await _context.SaveChangesAsync();

				var fileService = new FileService(_fileLogger, _context);
				var fileResources = fileService.SaveFilesCompressed(
					model.Files ?? new List<IFormFile>(),
					model.FilesMeta,
					contact.Id,
					"Contact",
					ConnectedUserEmail,
					ConnectedUserIp
				);

				if (contact.Files == null)
				{
					contact.Files = new List<FileResource>();
				}
				foreach (var file in fileResources)
				{
					file.OwnerId = contact.Id;
					file.OwnerType = "Contact";
					_context.FileResources.Add(file);
					contact.Files.Add(file);
				}

				await _context.SaveChangesAsync();

				var response = _mapper.Map<ContactResponseDTO>(contact);

				_logger.LogInformation($"Contact '{contact.FirstName} {contact.LastName}' (ID: {contact.Id}) créé par l'utilisateur '{ConnectedUserEmail}' depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Création contact {contact.LastName} {contact.FirstName}", ConnectedUserEmail ?? "inconnu", ConnectedUserIp ?? "inconnue", "Contact", contact.Id);
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erreur lors de la création du contact {model.FirstName} {model.LastName} par l'utilisateur '{ConnectedUserEmail ?? "inconnu"}' depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Echec création contact : {ex.Message}", ConnectedUserEmail ?? "inconnu", ConnectedUserIp ?? "inconnue", "Contact", null);
				await _context.SaveChangesAsync();
				return StatusCode(500, "Erreur interne du serveur");
			}
		}

		// PATCH: api/contact/{id}
		[HttpPatch("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "UPDATE_CONTACT")]
		public async Task<ActionResult<ContactResponseDTO>> UpdateContact(int id, [FromForm] ContactUpdateDTO model)
		{
			var contact = await _context.Contacts
				.Include(c => c.Files)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (contact == null)
			{
				_logger.LogWarning($"Tentative de modification d'un contact inexistant id={id} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec modification contact id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Contact", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			contact.FirstName = model.FirstName ?? contact.FirstName;
			contact.LastName = model.LastName ?? contact.LastName;
			contact.Email = model.Email ?? contact.Email;
			contact.PhoneNumber = model.PhoneNumber ?? contact.PhoneNumber;
			contact.Position = model.Position ?? contact.Position;
			contact.Note = model.Note ?? contact.Note;
			contact.UpdatedAt = DateUtils.CurrentDateTimeUtils();

			var fileService = new FileService(_fileLogger, _context);
			var fileResources = fileService.SaveFilesCompressed(
				model.Files ?? new List<IFormFile>(),
				model.FilesMeta,
				contact.Id,
				"Contact",
				ConnectedUserEmail,
				ConnectedUserIp
			);

			if (contact.Files == null)
			{
					contact.Files = new List<FileResource>();
			}

			foreach (var file in fileResources)
			{
				file.OwnerId = contact.Id;
				file.OwnerType = "Contact";
				_context.FileResources.Add(file);
				contact.Files.Add(file);
			}

			_context.Contacts.Update(contact);
			await _context.SaveChangesAsync();

			var response = _mapper.Map<ContactResponseDTO>(contact);

			_logger.LogInformation($"Contact '{contact.FirstName} {contact.LastName}' (ID: {contact.Id}) modifié par l'utilisateur '{ConnectedUserEmail}' depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Modification contact {contact.FirstName} {contact.LastName}", ConnectedUserEmail ?? "inconnu", ConnectedUserIp ?? "inconnue", "Contact", contact.Id);
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// DELETE: api/contacts/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "DELETE_CONTACT")]
		public async Task<ActionResult> DeleteContact(int id)
		{
			var contact = await _context.Contacts
					.FirstOrDefaultAsync(c => c.Id == id);

			if (contact == null)
			{
				_logger.LogWarning($"Tentative de suppression d'un contact inexistant id={id} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec suppression contact id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Contact", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			contact.DeletedAt = DateUtils.CurrentDateTimeUtils();

			_context.Contacts.Update(contact);
			await _context.SaveChangesAsync();

			_logger.LogInformation($"Suppression (soft) du contact {contact.FirstName} {contact.LastName} par {ConnectedUserEmail ?? "un visiteur"}");
			AuditLogHelper.AddAudit(_context, $"Suppression (soft) contact {contact.FirstName} {contact.LastName}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue", "Contact", contact.Id);
			await _context.SaveChangesAsync();

			return NoContent();
		}

	}

}