using AgentActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ProfileMidTier.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProfileMidTier
{
	/// <summary>
	/// The FabricRuntime creates an instance of this class for each service type instance.
	/// </summary>
	internal sealed class ProfileMidTier : StatefulService, IAgentService
	{
		public async Task<IEnumerable<ProfileInfo>> ListAgentProfiles()
		{
			var dict = await this.StateManager.TryGetAsync<IReliableDictionary<int, ProfileInfo>>("agentdict");
			var list = new List<ProfileInfo>();

			if (dict.HasValue)
			{
				using (var tx = this.StateManager.CreateTransaction())
				{
					var infolist = await dict.Value.CreateEnumerableAsync(tx);
					foreach (var kv in infolist)
					{
						ActorId agentid = new ActorId(kv.Key);
						IAgentActor agentproxy = ActorProxy.Create<IAgentActor>(agentid, __agentActorUri);
						list.Add(await agentproxy.GetProfileAsync());
					}
					await tx.CommitAsync();
				}
			}
			return list;
		}

		static Uri __agentActorUri = new Uri("fabric:/ProfileFS2/AgentActorService");
		public async Task RegisterOrUpdateAgent(ProfileInfo info)
		{
			try {
				var dict = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, ProfileInfo>>("agentdict");

				using (var tx = this.StateManager.CreateTransaction())
				{
					ActorId agentid = new ActorId(info.EmployeeID);
					IAgentActor agentproxy = ActorProxy.Create<IAgentActor>(agentid, __agentActorUri);
					await agentproxy.UpdateProfileAsync(info);
					await agentproxy.RegisterActivityAnalysisJob();

					await dict.TryAddAsync(tx, info.EmployeeID, info);
					await tx.CommitAsync();

					ServiceEventSource.Current.Message("Agent registered. agent id: " + info.EmployeeID);
				}
			}
			catch(Exception ex)
			{
				ServiceEventSource.Current.Message("Exception occured when register agent. agentid: " + info.EmployeeID);
			}
		}

		/// <summary>
		/// Optional override to create listeners (like tcp, http) for this service replica.
		/// </summary>
		/// <returns>The collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
		{
			// TODO: If your service needs to handle user requests, return a list of ServiceReplicaListeners here.
			return new[]
			{
				new ServiceReplicaListener(
					param=>new ServiceRemotingListener<IAgentService>(
						this.ServiceInitializationParameters,
						this))
			};
		}

		/// <summary>
		/// This is the main entry point for your service's partition replica. 
		/// RunAsync executes when the primary replica for this partition has write status.
		/// </summary>
		/// <param name="cancelServicePartitionReplica">Canceled when Service Fabric terminates this partition's replica.</param>
		protected override async Task RunAsync(CancellationToken cancelServicePartitionReplica)
		{
			// This partition's replica continues processing until the replica is terminated.
			while (!cancelServicePartitionReplica.IsCancellationRequested)
			{
				// Pause for 1 second before continue processing.
				await Task.Delay(TimeSpan.FromSeconds(1), cancelServicePartitionReplica);
			}
		}
	}
}
