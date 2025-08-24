using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using api.Data;
using api.Services;
using api.DTOs;
using api.Models;
using api.Helpers;
using api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using api.Enums;

namespace api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProjectsController : BaseController
	{
		private readonly IMapper _mapper;

		public ProjectsController(
				AppDbContext context,
				ILogger<ProjectsController> logger,
				ILogger<FileService> fileLogger,
				IMapper mapper
		) : base(context, logger, fileLogger, mapper)
		{
			_mapper = mapper;
		}

		// GET: api/projects
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProjectResponseDTO>>> GetProjects(
				[FromQuery] bool includeDeleted = false,
				[FromQuery] bool allStatus = false)
		{
			try
			{
				var query = _context.Projects
						.Include(p => p.Skills).ThenInclude(s => s.Files)
						.Include(p => p.Files)
						.AsQueryable();

				// Par défaut, on ne prend que les projets publiés et non supprimés
				if (!includeDeleted)
				{
					query = query.Where(p => p.DeletedAt == null);
				}

				if (!allStatus)
				{
					query = query.Where(p => p.Status == StatusEnum.PUBLISHED);
				}

				var projects = await query.ToListAsync();

				var responses = new List<ProjectResponseDTO>();
				foreach (var project in projects)
				{
					var response = _mapper.Map<ProjectResponseDTO>(project);
					await UserUtils.FillProjectUsersAsync(_context, _mapper, project, response);
					responses.Add(response);
				}

				_logger.LogInformation($"Consultation de la liste des projets par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, "Consultation liste projets", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();

				return Ok(responses);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erreur lors de la consultation de la liste des projets par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Echec consultation liste projets : {ex.Message}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();
				return StatusCode(500, "Erreur interne du serveur.");
			}
		}

		// GET: api/projects/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<ProjectResponseDTO>> GetProject(int id)
		{
			try
			{
				var project = await _context.Projects
						.Include(p => p.Skills).ThenInclude(s => s.Files)
						.Include(p => p.Files)
						.FirstOrDefaultAsync(p => p.Id == id);

				if (project == null)
				{
					_logger.LogWarning($"Projet id={id} introuvable pour {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
					AuditLogHelper.AddAudit(_context, $"Echec consultation projet id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
					await _context.SaveChangesAsync();
					return NotFound();
				}

				var response = _mapper.Map<ProjectResponseDTO>(project);
				await UserUtils.FillProjectUsersAsync(_context, _mapper, project, response);

				_logger.LogInformation($"Consultation du projet {project.Name} par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Consultation projet {project.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erreur lors de la consultation du projet id={id} par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Echec consultation projet id={id} : {ex.Message}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();
				return StatusCode(500, "Erreur interne du serveur.");
			}
		}

		// POST: api/projects
		[HttpPost]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "CREATE_PROJECT")]
		public async Task<ActionResult<ProjectResponseDTO>> CreateProject([FromForm] ProjectCreateDTO model)
		{
			if (ConnectedUserId == null)
			{
				_logger.LogWarning($"Tentative de création de projet sans authentification depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, "Echec création projet (utilisateur non authentifié)", "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();
				return BadRequest("Utilisateur non authentifié.");
			}

			try
			{
				var skills = await _context.Skills
						.Include(s => s.Files)
						.Where(s => model.SkillIds.Contains(s.Id))
						.ToListAsync();

				var slug = !string.IsNullOrWhiteSpace(model.Slug)
						? StringUtils.SanitizeString(model.Slug)
						: StringUtils.SanitizeString(model.Name);

				var project = new Project
				{
					Name = model.Name,
					Overview = model.Overview,
					Description = model.Description,
					Slug = slug,
					Url = model.Url,
					GithubUrl = model.GithubUrl,
					DeveloperRole = model.DeveloperRole,
					Status = model.Status,
					IsOnline = model.IsOnline ?? false,
					Skills = skills,
					Files = new List<FileResource>(),
					CreatedAt = DateUtils.CurrentDateTimeUtils(),
					CreatedById = ConnectedUserId.Value
				};

				_context.Projects.Add(project);
				await _context.SaveChangesAsync();

				var fileService = new FileService(_fileLogger, _context);
				var fileResources = fileService.SaveFilesCompressed(
						model.Files,
						model.FilesMeta,
						project.Id,
						"Project",
						ConnectedUserEmail,
						ConnectedUserIp
				);

				foreach (var fr in fileResources)
				{
					fr.OwnerId = project.Id;
					fr.OwnerType = "Project";
					_context.FileResources.Add(fr);
					project.Files.Add(fr);
				}
				await _context.SaveChangesAsync();

				var response = _mapper.Map<ProjectResponseDTO>(project);
				await UserUtils.FillProjectUsersAsync(_context, _mapper, project, response);

				_logger.LogInformation($"Création du projet {project.Name} par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Création projet {project.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();

				return CreatedAtAction(nameof(GetProject), new { id = project.Id }, response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Erreur lors de la création d'un projet par {ConnectedUserEmail ?? "un visiteur"} depuis l'IP {ConnectedUserIp ?? "inconnue"}");
				AuditLogHelper.AddAudit(_context, $"Echec création projet : {ex.Message}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();
				return StatusCode(500, "Erreur interne du serveur.");
			}
		}

		// PATCH: api/projects/{id}
		[HttpPatch("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "UPDATE_PROJECT")]
		public async Task<ActionResult<ProjectResponseDTO>> PatchProject(int id, [FromBody] ProjectUpdateDTO model)
		{
			var project = await _context.Projects
					.Include(p => p.Skills).ThenInclude(s => s.Files)
					.Include(p => p.Files)
					.FirstOrDefaultAsync(p => p.Id == id);

			if (project == null)
			{
				_logger.LogWarning($"Tentative de modification d'un projet inexistant id={id} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec modification projet id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();
				return NotFound();
			}

			// Mise à jour partielle
			if (model.Name != null) project.Name = model.Name;
			if (model.Overview != null) project.Overview = model.Overview;
			if (model.Description != null) project.Description = model.Description;
			if (model.Slug != null) project.Slug = StringUtils.SanitizeString(model.Slug);
			if (model.Url != null) project.Url = model.Url;
			if (model.GithubUrl != null) project.GithubUrl = model.GithubUrl;
			if (model.DeveloperRole.HasValue) project.DeveloperRole = model.DeveloperRole.Value;
			if (model.Status.HasValue)
			{
				if (model.Status.Value == StatusEnum.PUBLISHED && project.Status != StatusEnum.PUBLISHED)
				{
					project.PublishedAt = DateUtils.CurrentDateTimeUtils();
					project.PublishedById = ConnectedUserId;
				}
				project.Status = model.Status.Value;
			}
			if (model.IsOnline.HasValue) project.IsOnline = model.IsOnline.Value;
			project.UpdatedAt = DateUtils.CurrentDateTimeUtils();
			project.UpdatedById = ConnectedUserId;

			_context.Projects.Update(project);
			await _context.SaveChangesAsync();

			var response = _mapper.Map<ProjectResponseDTO>(project);
			await UserUtils.FillProjectUsersAsync(_context, _mapper, project, response);

			_logger.LogInformation($"Modification du projet {project.Name} par {ConnectedUserEmail ?? "un visiteur"}");
			AuditLogHelper.AddAudit(_context, $"Modification projet {project.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// DELETE: api/projects/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "DELETE_PROJECT")]
		public async Task<IActionResult> DeleteProject(int id)
		{
			var project = await _context.Projects
					.FirstOrDefaultAsync(p => p.Id == id);

			if (project == null)
			{
				_logger.LogWarning($"Tentative de suppression d'un projet inexistant id={id} par {ConnectedUserEmail ?? "un visiteur"}");
				AuditLogHelper.AddAudit(_context, $"Echec suppression projet id={id} (introuvable)", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
				await _context.SaveChangesAsync();
				return NotFound();
			}

			project.DeletedAt = DateUtils.CurrentDateTimeUtils();
			project.DeletedById = ConnectedUserId;
			project.Status = StatusEnum.ARCHIVED;

			_context.Projects.Update(project);
			await _context.SaveChangesAsync();

			_logger.LogInformation($"Suppression (soft) du projet {project.Name} par {ConnectedUserEmail ?? "un visiteur"}");
			AuditLogHelper.AddAudit(_context, $"Suppression (soft) projet {project.Name}", ConnectedUserEmail ?? "visiteur", ConnectedUserIp ?? "inconnue");
			await _context.SaveChangesAsync();

			return NoContent();
		}

	}
		
}