using Microsoft.AspNet.Mvc;
using System;

namespace RoutingWebSite
{
    [Route("/Teams", Order = 1)]
    public class TeamController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public TeamController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [HttpGet("/Team/{teamId}", Order = 1)]
        public ActionResult GetTeam(int teamId)
        {
            return _generator.Generate("/Team/" + teamId);
        }

        [HttpGet("/Team/{teamId}")]
        public ActionResult GetOrganization(int teamId)
        {
            return _generator.Generate("/Team/" + teamId);
        }

        [HttpGet("")]
        public ActionResult GetTeams()
        {
            return _generator.Generate("/Teams");
        }

        [HttpGet("", Order = 0)]
        public ActionResult GetOrganizations()
        {
            return _generator.Generate("/Teams");
        }
    }
}