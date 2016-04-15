using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AgentActor.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ProfileMidTier.Interfaces;

namespace WebFront.Controllers
{
	[Route("api/[controller]")]
	public class AgentController : Controller
	{
		static Uri __agentServiceUri = new Uri("fabric:/ProfileFS2/ProfileMidTier");
		[HttpGet("{compid}")]
		public Task<IEnumerable<ProfileInfo>> Get(int compid)
		{
			IAgentService agentproxy = ServiceProxy.Create<IAgentService>(compid, __agentServiceUri);
			return agentproxy.ListAgentProfiles();
		}

		// PUT api/values/5
		[HttpPut("{compid}")]
		public async Task Put(int compid, [FromBody]ProfileInfo value)
		{
			IAgentService agentproxy = ServiceProxy.Create<IAgentService>(compid, __agentServiceUri);
			await agentproxy.RegisterOrUpdateAgent(value);
		}
	}
}
