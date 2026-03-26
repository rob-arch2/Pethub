using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pethub.Data;
using Pethub.Models;

namespace Pethub.Pages.AccountManagement
{
    public class IndexModel : AdminPageModel
    {
        private readonly PethubContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(PethubContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Account> Account { get;set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                _logger?.LogDebug("IndexModel: OnGetAsync() started");
                Account = await _context.Account.ToListAsync();
                _logger?.LogDebug("✓ IndexModel: Loaded {AccountCount} accounts", Account.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ IndexModel: Exception in OnGetAsync");
                Account = new List<Account>();
            }
        }
    }
}

