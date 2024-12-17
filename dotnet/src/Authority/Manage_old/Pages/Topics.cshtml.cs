using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Agience.Authority.Manage.Services;
using Agience.Authority.Models.Manage;

namespace Agience.Authority.Manage.Pages
{
    public class TopicModel : PageModel
    {
        public List<Topic> Topics { get; private set; } = new();

        private readonly ILogger<TopicModel> _logger;
        private readonly AgienceAuthorityService _authorityService;

        public string? AuthorityUri => _authorityService.AuthorityUri;

        public TopicModel(AgienceAuthorityService agienceAuthorityService, ILogger<TopicModel> logger)
        {
            _authorityService = agienceAuthorityService;
            _logger = logger;
        }
        public void SetView(string? activeTab, string? activePage)
        {
            TempData["ActiveTab"] = activeTab;
            TempData["ActivePage"] = activePage;
        }

    }
}