using AgentActor.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileMidTier.Interfaces
{
    public interface IAgentService: IService
	{
		Task RegisterOrUpdateAgent(ProfileInfo info);
		Task<IEnumerable<ProfileInfo>> ListAgentProfiles();
	}
}
