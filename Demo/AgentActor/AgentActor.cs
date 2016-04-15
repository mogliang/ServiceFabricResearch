using AgentActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AgentActor
{
	/// <remarks>
	/// Each ActorID maps to an instance of this class.
	/// The IProjName  interface (in a separate DLL that client code can
	/// reference) defines the operations exposed by ProjName objects.
	/// </remarks>
	internal class AgentActor : StatefulActor<AgentActor.ActorState>, IAgentActor, IRemindable
	{
		/// <summary>
		/// This class contains each actor's replicated state.
		/// Each instance of this class is serialized and replicated every time an actor's state is saved.
		/// For more information, see http://aka.ms/servicefabricactorsstateserialization
		/// </summary>
		[DataContract]
		internal sealed class ActorState
		{
			[DataMember]
			public ProfileInfo ProfileInfo { set; get; }
		}

		/// <summary>
		/// This method is called whenever an actor is activated.
		/// </summary>
		protected override Task OnActivateAsync()
		{
			// initialize state
			if (State == null)
				State = new ActorState
				{
					ProfileInfo = new ProfileInfo()
				};

			ActorEventSource.Current.ActorMessage(this, "Actor activated.");
			return Task.FromResult(true);
		}

		static Random __rand = new Random();
		public Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
		{
			if (reminderName == __taskname)
			{
				if (State.ProfileInfo.Efficiency == null)
					State.ProfileInfo.Efficiency = 5;
				State.ProfileInfo.Efficiency += (__rand.Next(11) - 5) / 10.0;

				//// availability
				State.ProfileInfo.IsAvailable = __rand.Next(100) >= 50;
				ActorEventSource.Current.ActorMessage(this, "Agent profile daily update. AgentActorId:{0}", this.GetActorId());
			}
			return Task.FromResult<object>(null);
		}

		[Readonly]
		public Task<ProfileInfo> GetProfileAsync()
		{
			return Task.FromResult<ProfileInfo>(State.ProfileInfo);
		}

		public Task UpdateProfileAsync(ProfileInfo info)
		{
			if (State.ProfileInfo.EmployeeID == 0)
				State.ProfileInfo.EmployeeID = int.Parse(this.GetActorId().ToString());
			if (info.Name != null)
				State.ProfileInfo.Name = info.Name;
			if (info.Title != null)
				State.ProfileInfo.Title = info.Title;
			if (info.Efficiency != null)
				State.ProfileInfo.Efficiency = info.Efficiency;
			if (info.IsAvailable != null)
				State.ProfileInfo.IsAvailable = info.IsAvailable;

			ActorEventSource.Current.ActorMessage(this, "Agent profile updated. AgentActorId:{0}", this.GetActorId());

			return Task.FromResult<object>(null);
		}

		const string __taskname = "activity reminder";
		public Task RegisterActivityAnalysisJob()
		{
			Task<IActorReminder> reminderRegistration = RegisterReminderAsync(
				__taskname,
				null,
				TimeSpan.FromSeconds(10),
				TimeSpan.FromSeconds(10),
				ActorReminderAttributes.None);

			return reminderRegistration;
		}

		public Task UnRegisterActivityAnalysisJob()
		{
			IActorReminder reminder = GetReminder(__taskname);
			return UnregisterReminderAsync(reminder);
		}
	}
}
