using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Willinks.Api.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        // Check if this request is coming from the links subdomain
        var isLinksSubdomain = HttpContext.Items["IsLinksSubdomain"] as bool? == true;
        
        // If not on the links subdomain, return 404
        // The redirect endpoint will handle it on willc.pro domain
        if (!isLinksSubdomain)
        {
            HttpContext.Response.StatusCode = 404;
        }
    }
}
