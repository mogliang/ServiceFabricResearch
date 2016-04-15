using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace AgentActor.Interfaces
{
	/// <summary>
	/// This interface represents the actions a client app can perform on an actor.
	/// It MUST derive from IActor and all methods MUST return a Task.
	/// </summary>
	public interface IAgentActor : IActor
	{
		Task UpdateProfileAsync(ProfileInfo info);
		Task<ProfileInfo> GetProfileAsync();
		Task RegisterActivityAnalysisJob();
		Task UnRegisterActivityAnalysisJob();
	}

	public class ProfileInfo
	{
		public int EmployeeID { set; get; }
		public string Name { set; get; }
		public string Title { set; get; }
		public bool? IsAvailable { set; get; }
		public double? Efficiency { set; get; }
	}
}
