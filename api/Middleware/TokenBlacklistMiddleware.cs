using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using api.Data;
using System.Collections.Concurrent;
using System.Linq;
using System;
using System.Threading;

public class TokenBlacklistMiddleware
{
	private readonly RequestDelegate _next;
	private static ConcurrentDictionary<string, byte> _blacklistedTokensCache = new ConcurrentDictionary<string, byte>();
	private static DateTime _lastRefresh = DateTime.MinValue;
	private static readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);
	private static readonly object _cacheLock = new object();

	public TokenBlacklistMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	private static void RefreshCache(AppDbContext db)
	{
		var tokens = db.BlacklistedTokens.Select(t => t.Token).ToList();
		var newCache = new ConcurrentDictionary<string, byte>(tokens.Select(t => new KeyValuePair<string, byte>(t, 0)));
		lock (_cacheLock)
		{
			_blacklistedTokensCache = newCache;
			_lastRefresh = DateTime.UtcNow;
		}
	}

	public async Task InvokeAsync(HttpContext context, AppDbContext db)
	{
		// Rafraîchit le cache si nécessaire
		if (DateTime.UtcNow - _lastRefresh > _refreshInterval)
		{
			RefreshCache(db);
		}

		var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
		if (!string.IsNullOrEmpty(token) && _blacklistedTokensCache.ContainsKey(token))
		{
			context.Response.StatusCode = 401;
			await context.Response.WriteAsync("Token blacklisted.");
			return;
		}
		await _next(context);
	}
		
}